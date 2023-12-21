/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-21
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;
using BosmanCommerce7.Module.BusinessObjects.Settings;
using BosmanCommerce7.Module.Models.EvolutionSdk;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices.Models {
  public record ResetInventoryContext(
    ProductRecord ProductRecord,
    EvolutionInventoryLevel EvolutionInventoryLevel,
    WarehouseLocationMapping WarehouseLocationMapping
  );
}