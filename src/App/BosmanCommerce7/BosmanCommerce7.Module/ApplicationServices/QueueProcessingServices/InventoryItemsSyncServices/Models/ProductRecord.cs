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
  }

  public record ProductVariantRecord {
    public required Commerce7InventoryId Id { get; init; }
    public required string Sku { get; init; }
    public ProductVariantInventoryRecord[]? Inventory { get; init; }
  }

  public record ProductVariantInventoryRecord {
    public required string ProductVariantId { get; set; }
    public required string InventoryLocationId { get; set; }
  }
}