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
  public class InventoryLocationsLocalCache : IInventoryLocationsLocalCache {
    private readonly IInventoryItemApiClient _inventoryItemApiClient;
    private readonly IAppDataFileManager _appDataFileManager;

    public InventoryLocationsLocalCache(IInventoryItemApiClient inventoryItemApiClient, IAppDataFileManager appDataFileManager) {
      _inventoryItemApiClient = inventoryItemApiClient;
      _appDataFileManager = appDataFileManager;
    }

    public Result<InventoryLocationRecord> GetLocationByTitle(string locationTitle) {
      throw new NotImplementedException();
    }

    public Result UpdateLocalCache() {
      throw new NotImplementedException();
    }
  }
}