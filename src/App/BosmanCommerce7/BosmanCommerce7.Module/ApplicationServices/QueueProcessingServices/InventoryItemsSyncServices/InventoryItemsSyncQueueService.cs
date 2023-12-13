/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-13
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using Microsoft.Extensions.Logging;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices {

  public class InventoryItemsSyncQueueService : SyncQueueServiceBase, IInventoryItemsSyncQueueService {
    private readonly IInventoryItemsSyncService _inventoryItemsSyncService;

    public InventoryItemsSyncQueueService(ILogger<InventoryItemsSyncQueueService> logger, IInventoryItemsSyncService inventoryItemsSyncService) : base(logger) {
      _inventoryItemsSyncService = inventoryItemsSyncService;
    }

    protected override void ProcessQueue() {
      var context = new InventoryItemsSyncContext();
      _inventoryItemsSyncService.Execute(context);
    }
  }
}