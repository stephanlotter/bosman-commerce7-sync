/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-10
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Inventory;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.BusinessObjects.InventoryItems;
using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.EvolutionSdk.Inventory;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices {

  public class InventoryItemsSyncService : SyncMasterDataServiceBase, IInventoryItemsSyncService {
    private readonly InventoryItemsSyncJobOptions _inventorySyncJobOptions;
    private readonly IInventoryItemsLocalMappingService _inventoryLocalMappingService;
    private readonly IInventoryItemsApiClient _apiClient;
    private readonly IEvolutionInventoryItemRepository _evolutionInventoryItemRepository;

    private List<EvolutionInventoryItemId> _processedIds = new();

    public InventoryItemsSyncService(ILogger<InventoryItemsSyncService> logger,
        InventoryItemsSyncJobOptions inventorySyncJobOptions,
        IInventoryItemsLocalMappingService inventoryLocalMappingService,
        IInventoryItemsApiClient apiClient,
        IEvolutionInventoryItemRepository evolutionInventoryRepository,
        ILocalObjectSpaceEvolutionSdkProvider localObjectSpaceEvolutionSdkProvider
    ) : base(logger, localObjectSpaceEvolutionSdkProvider) {
      _inventorySyncJobOptions = inventorySyncJobOptions;
      _inventoryLocalMappingService = inventoryLocalMappingService;
      _apiClient = apiClient;
      _evolutionInventoryItemRepository = evolutionInventoryRepository;
    }

    public Result<InventoryItemsSyncResult> Execute(InventoryItemsSyncContext context) {
      _errorCount = 0;
      _processedIds.Clear();

      Logger.LogDebug("Start {SyncService} records sync.", this.GetType());

      return ProcessQueueItems<InventoryItemsUpdateQueue>(context)
        .Bind(() => BuildResult())
        .Finally(result => {
          Logger.LogDebug("End {SyncService} records sync.", this.GetType());
          return result;
        });

      Result<InventoryItemsSyncResult> BuildResult() {
        return _errorCount == 0 ? Result.Success(new InventoryItemsSyncResult { }) : Result.Failure<InventoryItemsSyncResult>($"Completed with {_errorCount} errors.");
      }
    }

    protected override Result ProcessQueueItem(UpdateQueueBase updateQueueItem) {
      var queueItem = (InventoryItemsUpdateQueue)updateQueueItem;

      if (_processedIds.Contains(queueItem.InventoryItemId)) { return Result.Success(); }
      try {
        var evolutionInventoryResult = _evolutionInventoryItemRepository.Get(new InventoryDescriptor { InventoryItemId = queueItem.InventoryItemId });

        if (evolutionInventoryResult.IsFailure) {
          return Result.Failure($"Could not load inventory with ID {queueItem.InventoryItemId} from Evolution. ({evolutionInventoryResult.Error})");
        }

        var evolutionInventoryItem = evolutionInventoryResult.Value;
        var evolutionInventoryItemSku = $"{evolutionInventoryItem.ShortCode}";

        dynamic? inventoryMaster = null;

        var localMappingResult = _inventoryLocalMappingService.GetLocalId(queueItem.InventoryItemId);

        if (localMappingResult.HasValue) { inventoryMaster = TryFindUsingLocalMapping(); }

        inventoryMaster ??= TryFindUsingEvolutionInventoryName();

        var createInventory = inventoryMaster == null;

        var inventoryDescription = $"{evolutionInventoryItem.Description}";

        if (createInventory) {
          inventoryMaster = _apiClient.CreateInventoryItem(new CreateInventoryItemsRecord {
            Sku = $"{evolutionInventoryItemSku}",
            Description = $"{inventoryDescription}"
          });

          var inventoryMasterId = inventoryMaster.Id;
          _inventoryLocalMappingService.StoreMapping(queueItem.InventoryItemId, inventoryMasterId);
        }
        else {
          // TODO: update the inventory
        }

        _inventoryLocalMappingService.StoreMapping(queueItem.InventoryItemId, inventoryMaster!.Id);

        return Result.Success();

        dynamic? TryFindUsingLocalMapping() {
          var commerce7InventoryId = localMappingResult.Value;
          var commerce7Inventory = _apiClient.GetInventoryItemById(commerce7InventoryId);

          if (commerce7Inventory.IsFailure) {
            if (commerce7Inventory.Error.Contains("404")) {
              _inventoryLocalMappingService.DeleteMapping(queueItem.InventoryItemId);
              return null;
            }

            throw new Exception(commerce7Inventory.Error);
          }

          dynamic? inventory = commerce7Inventory.Value.Data?.FirstOrDefault();

          if (inventory == null) { _inventoryLocalMappingService.DeleteMapping(queueItem.InventoryItemId); }

          return inventory;
        }

        dynamic? TryFindUsingEvolutionInventoryName() {
          if (string.IsNullOrWhiteSpace(evolutionInventoryItemSku)) { return null; }
          var commerce7Inventory = _apiClient.GetInventoryItemBySku(evolutionInventoryItemSku);
          return commerce7Inventory.IsFailure ? throw new Exception(commerce7Inventory.Error) : commerce7Inventory.Value.Data?.FirstOrDefault();
        }
      }
      finally {
        _processedIds.Add(queueItem.InventoryItemId);
      }
    }
  }
}