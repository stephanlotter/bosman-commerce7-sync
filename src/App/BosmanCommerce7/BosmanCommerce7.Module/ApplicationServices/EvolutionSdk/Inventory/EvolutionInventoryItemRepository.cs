/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Models.EvolutionSdk.Inventory;
using CSharpFunctionalExtensions;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Inventory {

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

    public Result<InventoryItem> Get(InventoryDescriptor inventoryDescriptor) {
      Result<InventoryItem> Get(EvolutionInventoryItemId id) {
        try {
          var customer = new InventoryItem(id);
          if (customer == null) {
            return Result.Failure<InventoryItem>($"Inventory item with id {id} not found in Evolution");
          }

          return customer;
        }
        catch (Exception ex) {
          return Result.Failure<InventoryItem>($"Inventory item with id {id} not found in Evolution. ({ex.Message})");
        }
      }

      if (inventoryDescriptor.InventoryItemId.HasValue) {
        var c = Get(inventoryDescriptor.InventoryItemId.Value);
        if (c.IsSuccess) { return c; }
        if (string.IsNullOrWhiteSpace(inventoryDescriptor.SimpleCode)) {
          return Result.Failure<InventoryItem>("Cannot find inventory item by Id and no simple code was provided.");
        }
      }

      if (string.IsNullOrWhiteSpace(inventoryDescriptor.SimpleCode)) {
        return Result.Failure<InventoryItem>($"Inventory item lookup: Simple Code may not be empty.");
      }

      EvolutionInventoryItemId? id = GetId("select StockLink from StkItem where lower(cSimpleCode)=lower(@sku)", new { inventoryDescriptor.SimpleCode });

      if (!id.HasValue) {
        return Result.Failure<InventoryItem>($"Inventory item with SKU {inventoryDescriptor.SimpleCode} not found in Evolution.");
      }

      return Get(id.Value);
    }
  }
}