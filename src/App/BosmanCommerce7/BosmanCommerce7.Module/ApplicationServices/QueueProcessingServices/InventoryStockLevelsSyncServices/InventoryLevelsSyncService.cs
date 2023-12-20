/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-19
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.BusinessObjects.InventoryItems;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices {

  public class InventoryLevelsSyncService : SyncMasterDataServiceBase, IInventoryLevelsSyncService {
    private readonly IInventoryItemApiClient _inventoryItemApiClient;
    private readonly IInventoryItemsLocalCache _inventoryItemsLocalCache;
    private List<(EvolutionInventoryItemId, EvolutionWarehouseId)> _processedIds = new();

    public InventoryLevelsSyncService(ILogger<InventoryLevelsSyncService> logger,
      ILocalObjectSpaceEvolutionSdkProvider localObjectSpaceEvolutionSdkProvider,
      IInventoryItemApiClient inventoryItemApiClient,
      IInventoryItemsLocalCache inventoryItemsLocalCache) :
      base(logger, localObjectSpaceEvolutionSdkProvider) {
      _inventoryItemApiClient = inventoryItemApiClient;
      _inventoryItemsLocalCache = inventoryItemsLocalCache;
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

      return Result.Success();
    }
  }
}