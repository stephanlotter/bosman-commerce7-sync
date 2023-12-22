/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-20
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.Models.EvolutionSdk {
  public record EvolutionInventoryLevel(
    EvolutionInventoryItemCode Sku,
    EvolutionWarehouseCode WarehouseCode,
    EvolutionInventoryItemId inventoryItemId,
    EvolutionWarehouseId warehouseId,
    Quantity QuantityOnHand,
    Quantity QuantityOnSalesOrder,
    Quantity QuantityReserved) {
    public Quantity AvailableForSaleCount => QuantityOnHand - QuantityOnSalesOrder;
  };
}