/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-10
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.BusinessObjects.InventoryItems {

  [DefaultClassOptions]
  public class InventoryItemsUpdateQueueLog : UpdateQueueLogBase {
    private InventoryItemsUpdateQueue _inventoryItemsUpdateQueue = default!;

    [Association("InventoryItemsUpdateQueue-InventoryItemsUpdateQueueLog")]
    public InventoryItemsUpdateQueue InventoryItemsUpdateQueue {
      get => _inventoryItemsUpdateQueue;
      set => SetPropertyValue(nameof(InventoryItemsUpdateQueue), ref _inventoryItemsUpdateQueue, value);
    }

    public InventoryItemsUpdateQueueLog(Session session) : base(session) {
    }
  }
}