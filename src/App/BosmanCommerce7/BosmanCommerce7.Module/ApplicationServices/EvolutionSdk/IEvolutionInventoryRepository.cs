/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-20
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Models.EvolutionSdk;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {

  public interface IEvolutionInventoryRepository {

    Result<EvolutionInventoryLevel> Get(EvolutionInventoryItemId inventoryItemId, EvolutionWarehouseId warehouseId);
  }
}