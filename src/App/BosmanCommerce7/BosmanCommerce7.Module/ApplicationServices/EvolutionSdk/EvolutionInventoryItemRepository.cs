/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using CSharpFunctionalExtensions;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {

  public class EvolutionInventoryItemRepository : EvolutionRepositoryBase, IEvolutionInventoryItemRepository {

    public Result<InventoryItem> Get(string? code) {
      if (string.IsNullOrWhiteSpace(code)) {
        return Result.Failure<InventoryItem>($"Inventory lookup: code may not be empty.");
      }

      int? id = GetId("select StockLink from StkItem where lower(cSimpleCode)=lower(@code)", new { code });

      if (id == null) {
        return Result.Failure<InventoryItem>($"Inventory item with code {code} not found");
      }

      return new InventoryItem(id.Value);
    }
  }

}
