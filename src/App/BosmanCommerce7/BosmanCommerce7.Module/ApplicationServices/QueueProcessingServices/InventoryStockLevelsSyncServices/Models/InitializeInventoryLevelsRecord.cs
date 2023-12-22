/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices.Models {
  public record InitializeInventoryLevelsRecord {
    public required Commerce7Sku Sku { get; set; }

    public required Initialinventory[] InitialInventory { get; set; }

    public required Commerce7InventoryPolicy InventoryPolicy { get; set; }
  }

  public record Initialinventory {
    public required Commerce7LocationId InventoryLocationId { get; set; }

    public int AvailableForSale { get; set; }
  }

  public static class Commerce7InventoryPolicies {
    public const string DoNotSell = "Don't sell";

    public const string BackOrder = "backorder";
  }
}