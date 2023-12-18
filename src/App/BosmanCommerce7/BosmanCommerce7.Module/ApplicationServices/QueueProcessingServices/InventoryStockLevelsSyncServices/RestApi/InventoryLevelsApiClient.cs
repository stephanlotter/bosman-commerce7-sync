/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.RestApiClients;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices.RestApi {

  public class InventoryLevelsApiClient : ApiClientBase, IInventoryLevelsApiClient {

    public InventoryLevelsApiClient(ILogger<InventoryLevelsApiClient> logger, IApiClientService apiClientService) : base(logger, apiClientService) {
    }

    public Result<InitializeInventoryResponse> InitializeInventory() {
      throw new NotImplementedException();
    }

    public Result<ListInventoryLevelsResponse> ListInventory() {
      throw new NotImplementedException();
    }

    public Result<ResetInventoryResponse> ResetInventory() {
      throw new NotImplementedException();
    }
  }
}