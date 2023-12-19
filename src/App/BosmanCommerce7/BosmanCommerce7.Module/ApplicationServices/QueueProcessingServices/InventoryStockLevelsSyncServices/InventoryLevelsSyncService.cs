/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-19
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.BusinessObjects.Customers;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices {

  public class InventoryLevelsSyncService : SyncMasterDataServiceBase, IInventoryLevelsSyncService {
    private readonly IInventoryItemApiClient _inventoryItemApiClient;
    private readonly IInventoryItemsLocalCache _inventoryItemsLocalCache;

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

      return ProcessQueueItems<CustomerUpdateQueue>(context)
        .Bind(() => BuildResult())
        .Finally(result => {
          Logger.LogDebug("End {SyncService} records sync.", this.GetType().Name);
          return result;
        });

      Result<CustomerMasterSyncResult> BuildResult() {
        return _errorCount == 0 ? Result.Success(new CustomerMasterSyncResult { }) : Result.Failure<CustomerMasterSyncResult>($"Completed with {_errorCount} errors.");
      }
    }

    protected override Result ProcessQueueItem(UpdateQueueBase updateQueueItem) {
      throw new NotImplementedException();
    }
  }
}