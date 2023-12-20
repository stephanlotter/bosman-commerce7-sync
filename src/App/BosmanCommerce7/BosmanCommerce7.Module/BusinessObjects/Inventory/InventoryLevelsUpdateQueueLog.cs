/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-20
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.BusinessObjects.InventoryItems {

  [DefaultClassOptions]
  [NavigationItem("Logs")]
  public class InventoryLevelsUpdateQueueLog : UpdateQueueLogBase {
    private InventoryLevelsUpdateQueue _inventoryLevelsUpdateQueue = default!;

    [Association("InventoryLevelsUpdateQueueLog-InventoryLevelsUpdateQueueLog")]
    public InventoryLevelsUpdateQueue InventoryLevelsUpdateQueue {
      get => _inventoryLevelsUpdateQueue;
      set => SetPropertyValue(nameof(InventoryLevelsUpdateQueue), ref _inventoryLevelsUpdateQueue, value);
    }

    public InventoryLevelsUpdateQueueLog(Session session) : base(session) {
    }
  }
}