/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-10
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices {

  public class InventoryItemsSyncService : SyncMasterDataServiceBase, IInventoryItemsSyncService {
    private readonly InventorySyncJobOptions _inventorySyncJobOptions;
    private readonly IInventoryLocalMappingService _inventoryLocalMappingService;
    private readonly IInventoryApiClient _apiClient;
    private readonly IEvolutionInventoryRepository _evolutionInventoryRepository;

    private List<EvolutionInventoryId> _processedInventoryIds = new();

    public InventoryItemsSyncService(ILogger<InventoryItemsSyncService> logger,
        InventorySyncJobOptions inventorySyncJobOptions,
        IInventoryLocalMappingService inventoryLocalMappingService,
        IInventoryApiClient apiClient,
        IEvolutionInventoryRepository evolutionInventoryRepository,
        ILocalObjectSpaceProvider localObjectSpaceProvider
    ) : base(logger, localObjectSpaceProvider) {
      _inventorySyncJobOptions = inventorySyncJobOptions;
      _inventoryLocalMappingService = inventoryLocalMappingService;
      _apiClient = apiClient;
      _evolutionInventoryRepository = evolutionInventoryRepository;
    }

    public Result<InventoryItemsSyncResult> Execute(InventoryItemsSyncContext context) {
      _errorCount = 0;
      _processedInventoryIds.Clear();
      Logger.LogDebug("Start {SyncService} inventory records sync.", typeof(InventoryItemsSyncService).Name);

      var result = ProcessQueueItems<InventoryUpdateQueue>(context);

      if (result.IsFailure) {
        return Result.Failure<InventoryItemsSyncResult>(result.Error);
      }

      Logger.LogDebug("End {SyncService} inventory records sync.", typeof(InventoryItemsSyncService).Name);

      return _errorCount == 0 ? Result.Success(BuildResult()) : Result.Failure<InventoryItemsSyncResult>($"Completed with {_errorCount} errors.");

      InventoryItemsSyncResult BuildResult() {
        return new InventoryItemsSyncResult { };
      }
    }

    protected override Result ProcessQueueItem(UpdateQueueBase updateQueueItem) {
      var queueItem = (InventoryUpdateQueue)updateQueueItem;

      if (_processedInventoryIds.Contains(queueItem.InventoryId)) { return Result.Success(); }

      var evolutionInventoryResult = _evolutionInventoryRepository.GetInventory(new Module.Models.EvolutionSdk.InventoryDescriptor { InventoryId = queueItem.InventoryId });

      if (evolutionInventoryResult.IsFailure) {
        return Result.Failure($"Could not load inventory with ID {queueItem.InventoryId} from Evolution. ({evolutionInventoryResult.Error})");
      }

      var evolutionInventory = evolutionInventoryResult.Value;
      var evolutionInventoryName = $"{evolutionInventory.Name}";

      dynamic? inventoryMaster = null;

      var localMappingResult = _inventoryLocalMappingService.GetLocalInventoryId(queueItem.InventoryId);

      if (localMappingResult.HasValue) { inventoryMaster = TryFindUsingLocalMapping(); }

      inventoryMaster ??= TryFindUsingEvolutionInventoryName();

      var createInventory = inventoryMaster == null;

      var inventoryDescription = $"{evolutionInventory.Description}";

      if (createInventory) {
        inventoryMaster = _apiClient.CreateInventory(new CreateInventoryRecord {
          Name = $"{evolutionInventoryName}",
          Description = $"{inventoryDescription}"
        });

        var inventoryMasterId = inventoryMaster.Id;
        _inventoryLocalMappingService.StoreMapping(queueItem.InventoryId, inventoryMasterId);
      }
      else {
        // update the inventory
      }

      _inventoryLocalMappingService.StoreMapping(queueItem.InventoryId, inventoryMaster!.Id);

      _processedInventoryIds.Add(queueItem.InventoryId);

      return Result.Success();

      dynamic? TryFindUsingLocalMapping() {
        var commerce7InventoryId = localMappingResult.Value;
        var commerce7Inventory = _apiClient.GetInventoryMasterById(commerce7InventoryId);

        if (commerce7Inventory.IsFailure) {
          if (commerce7Inventory.Error.Contains("404")) {
            _inventoryLocalMappingService.DeleteMapping(queueItem.InventoryId);
            return null;
          }

          throw new Exception(commerce7Inventory.Error);
        }

        dynamic? inventory = commerce7Inventory.Value.InventoryMasters?.FirstOrDefault();

        if (inventory == null) { _inventoryLocalMappingService.DeleteMapping(queueItem.InventoryId); }

        return inventory;
      }

      dynamic? TryFindUsingEvolutionInventoryName() {
        if (string.IsNullOrWhiteSpace(evolutionInventoryName)) { return null; }
        var commerce7Inventory = _apiClient.GetInventoryMasterByName(evolutionInventoryName);
        return commerce7Inventory.IsFailure ? throw new Exception(commerce7Inventory.Error) : commerce7Inventory.Value.InventoryMasters?.FirstOrDefault();
      }
    }
  }
}