/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-19
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.BusinessObjects.InventoryItems;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices {

  public class InventoryLevelsSyncService : SyncMasterDataServiceBase, IInventoryLevelsSyncService {
    private readonly IInventoryLevelsApiClient _inventoryLevelsApiClient;
    private readonly IInventoryItemsLocalCache _inventoryItemsLocalCache;
    private readonly IWarehouseLocationMappingRepository _warehouseLocationMappingRepository;
    private readonly IEvolutionInventoryRepository _evolutionInventoryRepository;
    private List<(EvolutionInventoryItemId, EvolutionWarehouseId)> _processedIds = new();

    public InventoryLevelsSyncService(ILogger<InventoryLevelsSyncService> logger,
      ILocalObjectSpaceEvolutionSdkProvider localObjectSpaceEvolutionSdkProvider,
      IInventoryLevelsApiClient inventoryLevelsApiClient,
      IInventoryItemsLocalCache inventoryItemsLocalCache,
      IWarehouseLocationMappingRepository warehouseLocationMappingRepository,
      IEvolutionInventoryRepository evolutionInventoryRepository) :
      base(logger, localObjectSpaceEvolutionSdkProvider) {
      _inventoryLevelsApiClient = inventoryLevelsApiClient;
      _inventoryItemsLocalCache = inventoryItemsLocalCache;
      _warehouseLocationMappingRepository = warehouseLocationMappingRepository;
      _evolutionInventoryRepository = evolutionInventoryRepository;
    }

    public Result<InventoryLevelsSyncResult> Execute(InventoryLevelsSyncContext context) {
      _errorCount = 0;
      _processedIds.Clear();

      Logger.LogDebug("Start {SyncService} records sync.", this.GetType().Name);

      return ProcessQueueItems<InventoryLevelsUpdateQueue>(context)
        .Bind(() => BuildResult())
        .Finally(result => {
          Logger.LogDebug("End {SyncService} records sync.", this.GetType().Name);
          return result;
        });

      Result<InventoryLevelsSyncResult> BuildResult() {
        return _errorCount == 0 ? Result.Success(new InventoryLevelsSyncResult { }) : Result.Failure<InventoryLevelsSyncResult>($"Completed with {_errorCount} errors.");
      }
    }

    protected override Result ProcessQueueItem(UpdateQueueBase updateQueueItem) {
      var queueItem = (InventoryLevelsUpdateQueue)updateQueueItem;

      if (_processedIds.Contains((queueItem.InventoryItemId, queueItem.WarehouseId))) {
        queueItem.Status = QueueProcessingStatus.Skipped;
        return Result.Success();
      }

      return _evolutionInventoryRepository.Get(queueItem.InventoryItemId, queueItem.WarehouseId)
      .Map(evolutionInventoryLevel => {
        return _inventoryItemsLocalCache.GetProduct(evolutionInventoryLevel.Sku).Map(a => (a, evolutionInventoryLevel));
      })
      .Bind(a => {
        var (productRecord, evolutionInventoryLevel) = a;

        var result = GetLocationId(productRecord);

        // initialise inventory if not initialised for product.
        // update local cache after initialisation.

        return a;
      })
      .Bind(a => {
        var (productRecord, evolutionInventoryLevel) = a;
        
        _warehouseLocationMappingRepository.FindMapping(ObjectSpace!, evolutionInventoryLevel.WarehouseCode)
        .Bind(locationMapping => {
          if (locationMapping == null) { return Result.Failure($"Warehouse location mapping not found for warehouse code {evolutionInventoryLevel.WarehouseCode}.");  }
          return Result.Success(locationMapping);
          })
        .Bind(locationMapping => {
          // set inventory levels for product.
          
          return Result.Success(locationMapping);
          
          });
        return Result.Success();
      })
        ;

      
    }

    private Result GetLocationId(bool productRecord) {
      throw new NotImplementedException();
    }
  }
}