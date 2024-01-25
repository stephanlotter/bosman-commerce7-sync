/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-20
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.Models.EvolutionSdk {
  public record EvolutionInventoryLevel {
    public required EvolutionInventoryItemCode Sku { get; init; }

    public required EvolutionWarehouseCode WarehouseCode { get; init; }

    public required EvolutionInventoryItemId inventoryItemId { get; init; }

    public required EvolutionWarehouseId warehouseId { get; init; }

    public Quantity QuantityOnHand { get; init; }

    public Quantity QuantityOnSalesOrder { get; init; }

    public Quantity QuantityReserved { get; init; }

    public Quantity AvailableForSaleCount => QuantityOnHand - QuantityOnSalesOrder;
  };
}