/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-13
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models {
  public record ProductRecord {
    public Commerce7InventoryId Id { get; init; }

    public ProductVariantRecord[]? Variants { get; init; }

    public ProductVariantRecord? GetProductVariant(Commerce7Sku sku) {
      return Variants?.FirstOrDefault(x => x.Sku == sku);
    }
  }

  public record ProductVariantRecord {
    public required Commerce7InventoryId Id { get; init; }

    public required string Sku { get; init; }

    public ProductVariantInventoryRecord[]? Inventory { get; init; }

    public ProductVariantInventoryRecord? GetProductVariantInventoryRecord(Commerce7LocationId inventoryLocationId) {
      return Inventory?.FirstOrDefault(x => x.InventoryLocationId == inventoryLocationId);
    }
  }

  public record ProductVariantInventoryRecord {
    public required Commerce7InventoryId ProductVariantId { get; set; }

    public required Commerce7LocationId InventoryLocationId { get; set; }
  }
}