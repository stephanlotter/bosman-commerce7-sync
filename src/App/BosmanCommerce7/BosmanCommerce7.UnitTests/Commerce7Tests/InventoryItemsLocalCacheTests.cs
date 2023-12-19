/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-19
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices;

namespace BosmanCommerce7.UnitTests.Commerce7Tests {

  public class InventoryItemsLocalCacheTests : Commerce7ClientApiTestsBase {

    [Fact]
    public void Test_UpdateLocalCache() {
      var apiClient = new InventoryItemApiClient(A.Fake<ILogger<InventoryItemApiClient>>(), ApiClientService);

      var sut = new InventoryItemsLocalCache(apiClient, NewAppDataFileManager());

      var result = sut.UpdateLocalCache();

      result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("2015C", true)]
    [InlineData("2015CSR", true)]
    [InlineData("2015R", true)]
    [InlineData("2015RX", false)]
    public void Test_GetProduct(string sku, bool foundInCache) {
      var apiClient = new InventoryItemApiClient(A.Fake<ILogger<InventoryItemApiClient>>(), ApiClientService);

      var sut = new InventoryItemsLocalCache(apiClient, NewAppDataFileManager());

      var result = sut.GetProduct(sku);

      result.IsSuccess.Should().Be(foundInCache);
      if (foundInCache) {
        result.Value.Variants!.First().Sku.Should().Be(sku);
      }
    }
  }
}