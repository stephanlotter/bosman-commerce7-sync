/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-13
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.Models.EvolutionSdk.Inventory {
  public record InventoryDescriptor {
    public EvolutionInventoryItemId? InventoryItemId { get; init; }

    public string? Sku { get; init; }
  }
}