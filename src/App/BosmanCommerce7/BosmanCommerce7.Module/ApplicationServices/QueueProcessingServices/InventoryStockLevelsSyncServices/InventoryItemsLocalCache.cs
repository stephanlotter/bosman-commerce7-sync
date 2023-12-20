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
using CSharpFunctionalExtensions.ValueTasks;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices {

  public class InventoryItemsLocalCache : IInventoryItemsLocalCache {
    private readonly IInventoryItemApiClient _inventoryItemApiClient;
    private readonly IAppDataFileManager _appDataFileManager;
    private ProductRecord[]? _productRecords;

    private const string _folderName = "ProductCache";
    private const string _fileName = "products.json";

    public InventoryItemsLocalCache(IInventoryItemApiClient inventoryItemApiClient, IAppDataFileManager appDataFileManager) {
      _inventoryItemApiClient = inventoryItemApiClient;
      _appDataFileManager = appDataFileManager;
    }

    public Result<ProductRecord> GetProduct(string sku) {
      var tryCount = 0;
      while (true) {
        var r = LoadLocalCache();
        if (r.IsFailure) { return Result.Failure<ProductRecord>(r.Error); }

        var product = _productRecords?.FirstOrDefault(x => x.Variants?.Any(y => y.Sku == sku) ?? false);

        if (product != null) { return Result.Success(product); }

        if (tryCount == 0) {
          r = UpdateLocalCache();
          if (r.IsFailure) { return Result.Failure<ProductRecord>($"Unable to update local product cache. {r.Error}"); }
        }
        else {
          return Result.Failure<ProductRecord>($"SKU {sku} not found on Commerce7.");
        }
        tryCount++;
      }
    }

    private Result LoadLocalCache() {
      if (_productRecords is not null || !_appDataFileManager.FileExists(_folderName, _fileName)) { return Result.Success(); }
      var result = _appDataFileManager.LoadJson<ProductRecord[]>(_folderName, _fileName);
      if (result.IsFailure) { return Result.Failure($"Unable to load local cache. {result.Error}"); }
      _productRecords = result.Value;
      return Result.Success();
    }

    public Result UpdateLocalCache() {
      return _inventoryItemApiClient.GetAllInventoryItems()
         .Bind(x => {
           return MapProductsToProductRecords(x);
         })
         .Bind(y => {
           _appDataFileManager.StoreJson(_folderName, _fileName, y);
           _productRecords = y;
           return Result.Success();
         });
    }

    private Result<ProductRecord[]> MapProductsToProductRecords(InventoryItemsResponse value) {
      var products = value.Data ?? null;

      if (products == null || products == null || products!.Length == 0) { return Result.Failure<ProductRecord[]>("No data"); }

      var list = new List<ProductRecord>();

      bool HasElements(dynamic? v) {
        try {
          return v is not null && v.Count > 0;
        }
        catch {
          return false;
        }
      }

      foreach (var product in products!) {
        var productRecord = new ProductRecord {
          Id = Commerce7InventoryId.Parse($"{product.id}"),
        };

        if (HasElements(product.variants)) {
          var productVariantRecords = new List<ProductVariantRecord>();

          foreach (var variant in product.variants) {
            var productVariantRecord = new ProductVariantRecord {
              Id = Commerce7InventoryId.Parse($"{variant.id}"),
              Sku = variant.sku,
            };

            if (HasElements(variant.inventory)) {
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