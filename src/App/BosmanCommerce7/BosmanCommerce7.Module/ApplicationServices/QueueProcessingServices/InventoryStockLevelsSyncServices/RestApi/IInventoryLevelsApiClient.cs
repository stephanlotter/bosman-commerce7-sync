/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices.RestApi;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi {

  public interface IInventoryLevelsApiClient {

    Result<ListInventoryLevelsResponse> ListInventory();

    Result<InitializeInventoryResponse> InitializeInventory();

    Result<ResetInventoryResponse> ResetInventory();
  }
}