﻿/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-13
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.RestApi;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi {

  public interface IInventoryItemsApiClient {

    Result<InventoryItemsResponse> CreateInventoryItem(CreateInventoryItemsRecord inventoryItemsRecord);

    Result<InventoryItemsResponse> GetInventoryItemBySku(Commerce7Sku sku);

    Result<InventoryItemsResponse> GetInventoryItemById(Commerce7InventoryId commerce7InventoryId);
  }
}