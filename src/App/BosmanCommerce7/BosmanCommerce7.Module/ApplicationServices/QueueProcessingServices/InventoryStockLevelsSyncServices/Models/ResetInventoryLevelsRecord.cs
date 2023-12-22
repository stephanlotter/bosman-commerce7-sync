/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices.Models {
  public record ResetInventoryLevelsRecord {
    public string Action { get; init; } = "Reset";

    public required Commerce7Sku Sku { get; init; }

    public string Notes { get; init; } = "Reset count to match Sage 200";

    public Quantity AvailableForSaleCount { get; init; }

    public Quantity ReserveCount { get; init; }

    public required Commerce7LocationId InventoryLocationId { get; init; }
  }
}