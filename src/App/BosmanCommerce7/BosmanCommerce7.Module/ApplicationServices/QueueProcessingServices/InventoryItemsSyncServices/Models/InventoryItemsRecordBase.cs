/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-13
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models {
  public abstract record InventoryItemsRecordBase {
    public required string Sku { get; init; }

    public required string Description { get; init; }

  }

  public record CreateInventoryItemsRecord : InventoryItemsRecordBase { }
  public record UpdateInventoryItemsRecord : InventoryItemsRecordBase { }
}