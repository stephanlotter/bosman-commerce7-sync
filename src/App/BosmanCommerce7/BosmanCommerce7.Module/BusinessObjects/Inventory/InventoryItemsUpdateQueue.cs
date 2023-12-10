/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-10
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
  public class InventoryItemsUpdateQueue : UpdateQueueBase {
    private EvolutionInventoryItemId _inventoryItemId;

    [ModelDefault("AllowEdit", "false")]
    public EvolutionInventoryItemId InventoryItemId {
      get => _inventoryItemId;
      set => SetPropertyValue(nameof(InventoryItemId), ref _inventoryItemId, value);
    }

    [Association("InventoryItemsUpdateQueue-InventoryItemsUpdateQueueLog")]
    [Aggregated]
    public XPCollection<InventoryItemsUpdateQueueLog> InventoryItemsUpdateQueueLogs => GetCollection<InventoryItemsUpdateQueueLog>(nameof(InventoryItemsUpdateQueueLogs));

    public InventoryItemsUpdateQueue(Session session) : base(session) {
    }

    protected override UpdateQueueLogBase CreateLogEntry() {
      var log = new InventoryItemsUpdateQueueLog(Session);
      InventoryItemsUpdateQueueLogs.Add(log);
      return log;
    }
  }
}