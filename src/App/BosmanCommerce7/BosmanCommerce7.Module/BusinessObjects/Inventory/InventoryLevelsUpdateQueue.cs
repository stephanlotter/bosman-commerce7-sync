/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-20
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.BusinessObjects.InventoryItems {

  [DefaultClassOptions]
  [NavigationItem(true)]
  public class InventoryLevelsUpdateQueue : UpdateQueueBase {
    private EvolutionInventoryItemId _inventoryItemId;
    private EvolutionWarehouseId _warehouseId;

    [ModelDefault("AllowEdit", "false")]
    public EvolutionInventoryItemId InventoryItemId {
      get => _inventoryItemId;
      set => SetPropertyValue(nameof(InventoryItemId), ref _inventoryItemId, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public EvolutionInventoryItemId WarehouseId {
      get => _warehouseId;
      set => SetPropertyValue(nameof(WarehouseId), ref _warehouseId, value);
    }

    [Association("InventoryLevelsUpdateQueueLog-InventoryLevelsUpdateQueueLog")]
    [Aggregated]
    public XPCollection<InventoryLevelsUpdateQueueLog> InventoryLevelsUpdateQueueLogs => GetCollection<InventoryLevelsUpdateQueueLog>(nameof(InventoryLevelsUpdateQueueLogs));

    public InventoryLevelsUpdateQueue(Session session) : base(session) {
    }

    protected override UpdateQueueLogBase CreateLogEntry() {
      var log = new InventoryLevelsUpdateQueueLog(Session);
      InventoryLevelsUpdateQueueLogs.Add(log);
      return log;
    }
  }
}