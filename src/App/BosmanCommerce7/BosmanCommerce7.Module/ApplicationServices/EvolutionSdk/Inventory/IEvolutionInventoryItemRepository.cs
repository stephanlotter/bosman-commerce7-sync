/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-13
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Models.EvolutionSdk.Inventory;
using CSharpFunctionalExtensions;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Inventory {

  public interface IEvolutionInventoryItemRepository {

    Result<InventoryItem> GetInventoryItem(InventoryDescriptor inventoryDescriptor);
  }
}