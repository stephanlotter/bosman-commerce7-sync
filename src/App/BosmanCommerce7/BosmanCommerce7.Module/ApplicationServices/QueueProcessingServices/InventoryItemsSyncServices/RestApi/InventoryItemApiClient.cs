/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-13
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.RestApiClients;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi {

  public class InventoryItemApiClient : ApiClientBase, IInventoryItemApiClient {

    public InventoryItemApiClient(ILogger<InventoryItemApiClient> logger, IApiClientService apiClientService) : base(logger, apiClientService) {
    }

    public Result<InventoryItemsResponse> CreateInventoryItem(CreateInventoryItemsRecord inventoryItemsRecord) {
      throw new NotImplementedException();
    }

    public Result<InventoryItemsResponse> GetInventoryItemById(Commerce7InventoryId commerce7InventoryId) {
      throw new NotImplementedException();
    }

    public Result<InventoryItemsResponse> GetInventoryItemBySku(string sku) {
      throw new NotImplementedException();
    }

    public Result<InventoryLocationsResponse> GetLocations() {
      throw new NotImplementedException();
    }
  }
}