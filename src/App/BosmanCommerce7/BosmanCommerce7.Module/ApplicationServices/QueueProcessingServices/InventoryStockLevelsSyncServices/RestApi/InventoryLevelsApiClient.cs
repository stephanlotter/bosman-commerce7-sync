/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.RestApiClients;
using BosmanCommerce7.Module.Models.EvolutionSdk;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices.RestApi {

  public class InventoryLevelsApiClient : ApiClientBase, IInventoryLevelsApiClient {
    private readonly IInventoryItemsLocalCache _inventoryItemsLocalCache;
    private readonly IInventoryLocationsLocalCache _inventoryLocationsLocalCache;

    public InventoryLevelsApiClient(ILogger<InventoryLevelsApiClient> logger, IInventoryItemsLocalCache inventoryItemsLocalCache, IInventoryLocationsLocalCache inventoryLocationsLocalCache, IApiClientService apiClientService) : base(logger, apiClientService) {
      _inventoryItemsLocalCache = inventoryItemsLocalCache;
      _inventoryLocationsLocalCache = inventoryLocationsLocalCache;
    }

    public Result<InitializeInventoryResponse> InitializeInventory() {
      throw new NotImplementedException();
    }

    public Result<ListInventoryLevelsResponse> ListInventory() {
      throw new NotImplementedException();
    }

    public Result<ResetInventoryResponse> ResetInventory(ResetInventoryContext resetInventoryContext) {
      var evolutionInventoryLevel = resetInventoryContext.EvolutionInventoryLevel;
      var productRecord = resetInventoryContext.ProductRecord;
      var warehouseLocationMapping = resetInventoryContext.WarehouseLocationMapping;

      return MapLocationTitleToLocationId(productRecord, evolutionInventoryLevel.Sku, warehouseLocationMapping.LocationTitle!)
        .Bind(locationId => InitialiseLocationInventory(productRecord, evolutionInventoryLevel.Sku, locationId))
        .Bind(locationId => ResetLocationInventory(productRecord, evolutionInventoryLevel, locationId));
    }

    private Result<Commerce7LocationId> MapLocationTitleToLocationId(ProductRecord productRecord, EvolutionInventoryItemCode sku, string locationTitle) {
      if (string.IsNullOrWhiteSpace(locationTitle)) { return Result.Failure<Commerce7LocationId>("Location title may not be empty."); }

      return _inventoryLocationsLocalCache.GetLocationByTitle(locationTitle)
        .Bind(a => {
          if (a is null) { return Result.Failure<Commerce7LocationId>($"Location title {locationTitle} not found on Commerce7."); }
          return Result.Success(a.Id);
        });
    }

    private Result<Commerce7LocationId> InitialiseLocationInventory(ProductRecord productRecord, EvolutionInventoryItemCode sku, Commerce7LocationId locationId) {
      var productVariantInventory = productRecord.GetProductVariant(sku)?.GetProductVariantInventoryRecord(locationId);

      if (productVariantInventory is not null) { return Result.Success(locationId); }

      var data = new InitializeInventoryLevelsRecord {
        Sku = sku,
        InventoryPolicy = Commerce7InventoryPolicies.DoNotSell,
        InitialInventory = new[] {
          new Initialinventory {
            InventoryLocationId = locationId,
            AvailableForSale = 0
          }
        }
      };

      return SendSingleResultRequest(new InventoryLevelsInitializeApiRequest(data), () => new InitializeInventoryResponse())
        .Bind(_ => _inventoryItemsLocalCache.UpdateLocalCache())
        .Map(() => locationId);
    }

    private Result<ResetInventoryResponse> ResetLocationInventory(ProductRecord productRecord, EvolutionInventoryLevel evolutionInventoryLevel, Commerce7LocationId locationId) {
      var data = new ResetInventoryLevelsRecord {
        AvailableForSaleCount = evolutionInventoryLevel.AvailableForSaleCount,
        ReserveCount = evolutionInventoryLevel.QuantityReserved,
        InventoryLocationId = locationId,
        Sku = evolutionInventoryLevel.Sku
      };
      return SendSingleResultRequest(new InventoryLevelsResetApiRequest(data), () => new ResetInventoryResponse()).Map(r => (ResetInventoryResponse)r);
    }
  }
}