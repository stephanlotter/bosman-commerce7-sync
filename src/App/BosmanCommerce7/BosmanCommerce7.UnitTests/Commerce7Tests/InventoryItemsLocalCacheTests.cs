/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-19
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.AppDataServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices;
using BosmanCommerce7.Module.Models;

namespace BosmanCommerce7.UnitTests.Commerce7Tests {

  public class InventoryItemsLocalCacheTests : Commerce7ClientApiTestsBase {

    [Fact]
    public void Test() {
      var apiClient = new InventoryItemApiClient(A.Fake<ILogger<InventoryItemApiClient>>(), ApiClientService);

      var sut = new InventoryItemsLocalCache(apiClient, NewAppDataFileManager());



    }
  }
}