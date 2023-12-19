/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-19
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using Microsoft.Extensions.Logging;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices {

  public class InventoryLevelsSyncQueueService : SyncQueueServiceBase, IInventoryLevelsSyncQueueService {
    private readonly IInventoryLevelsSyncService _inventoryLevelsSyncService;

    public InventoryLevelsSyncQueueService(ILogger<InventoryLevelsSyncQueueService> logger, IInventoryLevelsSyncService inventoryLevelsSyncService) : base(logger) {
      _inventoryLevelsSyncService = inventoryLevelsSyncService;
    }

    protected override void ProcessQueue() {
      var context = new InventoryLevelsSyncContext();
      _inventoryLevelsSyncService.Execute(context);
    }
  }
}