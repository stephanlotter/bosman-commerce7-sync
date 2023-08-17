/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.EvolutionDataAccess;
using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models;
using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.LocalDatabase;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public class SalesOrdersSyncQueueService : SyncQueueServiceBase, ISalesOrdersSyncQueueService {
    private readonly IEvolutionInventoryRepository _evolutionInventoryRepository;
    private readonly ISalesOrdersQueueRepository _salesOrdersQueueRepository;
    private readonly ISalesOrdersSyncService _salesOrdersSyncService;

    public SalesOrdersSyncQueueService(ILogger<SalesOrdersSyncQueueService> logger,
      IEvolutionInventoryRepository evolutionInventoryRepository,
      ISalesOrdersQueueRepository salesOrdersQueueRepository,
      ISalesOrdersSyncService salesOrdersSyncService) : base(logger) {
      _evolutionInventoryRepository = evolutionInventoryRepository;
      _salesOrdersQueueRepository = salesOrdersQueueRepository;
      _salesOrdersSyncService = salesOrdersSyncService;
    }

    protected override void ProcessQueue() {
      // TODO: Implement this

      var pendingQueueItemsResult = _salesOrdersQueueRepository.LoadPendingQueueItems();

      if (pendingQueueItemsResult.IsFailure) { return; }

      var pendingQueueItems = pendingQueueItemsResult.Value;

      var list = new List<string>();

      foreach (var pendingQueueItem in pendingQueueItems) {

        if (list.Any(a => a.Equals(pendingQueueItem.SimpleCode, StringComparison.InvariantCultureIgnoreCase))) {
          QueueItemDuplicate(pendingQueueItem);
          continue;
        }

        list.Add(pendingQueueItem.SimpleCode);

        var inventoryItemResult = _evolutionInventoryRepository.GetItem(pendingQueueItem.SimpleCode);

        if (inventoryItemResult.IsFailure) {
          QueueItemFailure(pendingQueueItem, inventoryItemResult.Error);
          continue;
        }

        var syncContext = new SalesOrdersSyncContext {  };
        var syncResult = _salesOrdersSyncService.Execute(syncContext);

        if (syncResult.IsFailure) {
          QueueItemFailure(pendingQueueItem, syncResult.Error);
          continue;
        }

        QueueItemSuccess(pendingQueueItem);
      }

    }

    private void QueueItemSuccess(SalesOrdersQueueItemDto pendingQueueItem) {
      _salesOrdersQueueRepository.UpdateStatus(pendingQueueItem.OID, QueueItemStatus.Processed);
    }

    private void QueueItemDuplicate(SalesOrdersQueueItemDto pendingQueueItem) {
      _salesOrdersQueueRepository.UpdateStatus(pendingQueueItem.OID, QueueItemStatus.Processed, "Duplicate");
    }

    private void QueueItemFailure(SalesOrdersQueueItemDto pendingQueueItem, string error) {
      _salesOrdersQueueRepository.UpdateStatus(pendingQueueItem.OID, pendingQueueItem.RetryCount < 5 ? QueueItemStatus.Retry : QueueItemStatus.Failed, error);
    }
  }

}
