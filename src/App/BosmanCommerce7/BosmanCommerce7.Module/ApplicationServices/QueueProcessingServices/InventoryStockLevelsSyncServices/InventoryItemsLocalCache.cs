/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-19
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.AppDataServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices {

  public class InventoryItemsLocalCache : IInventoryItemsLocalCache {
    private readonly IInventoryItemApiClient _inventoryItemApiClient;
    private readonly IAppDataFileManager _appDataFileManager;

    public InventoryItemsLocalCache(IInventoryItemApiClient inventoryItemApiClient, IAppDataFileManager appDataFileManager) {
      _inventoryItemApiClient = inventoryItemApiClient;
      _appDataFileManager = appDataFileManager;
    }

    public Result<ProductRecord> GetProduct(string sku) {
      throw new NotImplementedException();
    }

    public Result UpdateLocalCache() {
      return _inventoryItemApiClient.GetAllInventoryItems()
         .Bind(x => {
           return MapProductsToProductRecords(x);
         })
         .Bind(y => {
           _appDataFileManager.StoreJson("ProductCache", "products.json", y);
           return Result.Success();
         });
    }

    private Result<ProductRecord[]> MapProductsToProductRecords(InventoryItemsSyncServices.RestApi.InventoryItemsResponse value) {
      var products = value.Data?.FirstOrDefault() ?? null;

      if (products == null || products?.products == null || !products!.products.Any()) { return Result.Failure<ProductRecord[]>("No data"); }

      var list = new List<ProductRecord>();

      foreach (var product in products!.products!) {
        var productRecord = new ProductRecord {
          Id = Commerce7InventoryId.Parse($"{product.id}"),
        };

        if (product.variants!.Any()) {
          var productVariantRecords = new List<ProductVariantRecord>();

          foreach (var variant in product.variants) {
            var productVariantRecord = new ProductVariantRecord {
              Id = Commerce7InventoryId.Parse($"{variant.id}"),
              Sku = variant.sku,
            };

            if (variant.inventory!.Any()) {
              var productVariantInventoryRecords = new List<ProductVariantInventoryRecord>();

              foreach (var variantInventory in variant.inventory) {
                productVariantInventoryRecords.Add(new ProductVariantInventoryRecord {
                  InventoryLocationId = variantInventory.inventoryLocationId,
                  ProductVariantId = variantInventory.productVariantId,
                });
              }

              productVariantRecord = productVariantRecord with { Inventory = productVariantInventoryRecords.ToArray() };
            }

            productVariantRecords.Add(productVariantRecord);
          }

          productRecord = productRecord with { Variants = productVariantRecords.ToArray() };
        }

        list.Add(productRecord);
      }

      return Result.Success(list.ToArray());
    }
  }
}