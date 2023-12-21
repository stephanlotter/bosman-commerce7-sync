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
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices.RestApi {

  public class InventoryLevelsApiClient : ApiClientBase, IInventoryLevelsApiClient {
    private readonly IInventoryLocationsLocalCache _inventoryLocationsLocalCache;

    public InventoryLevelsApiClient(ILogger<InventoryLevelsApiClient> logger, IInventoryLocationsLocalCache inventoryLocationsLocalCache, IApiClientService apiClientService) : base(logger, apiClientService) {
      _inventoryLocationsLocalCache = inventoryLocationsLocalCache;
    }

    public Result<InitializeInventoryResponse> InitializeInventory() {
      throw new NotImplementedException();
    }

    public Result<ListInventoryLevelsResponse> ListInventory() {
      throw new NotImplementedException();
    }

    public Result<ResetInventoryResponse> ResetInventory(ResetInventoryContext resetInventoryContext) {
      // reset inventory

      var evolutionInventoryLevel = resetInventoryContext.EvolutionInventoryLevel;
      var productRecord = resetInventoryContext.ProductRecord;
      var warehouseLocationMapping = resetInventoryContext.WarehouseLocationMapping;

      return MapLocationTitleToLocationId(productRecord, evolutionInventoryLevel.Sku, warehouseLocationMapping.LocationTitle!)
        .Bind(locationId => InitialiseLocationInventory(productRecord, evolutionInventoryLevel.Sku, locationId))

        .Bind(locationId => Result.Success(new ResetInventoryResponse { }));
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
      // initialise inventory if not initialised for product.
      // update local cache after initialisation.

      productRecord.GetProductVariant(sku);

      return Result.Success(locationId);
    }
  }
}