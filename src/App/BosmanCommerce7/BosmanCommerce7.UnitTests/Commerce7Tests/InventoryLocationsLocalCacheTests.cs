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

  public class InventoryLocationsLocalCacheTests : Commerce7ClientApiTestsBase {

    [Fact]
    public void Test_UpdateLocalCache() {
      var apiClient = new InventoryItemApiClient(A.Fake<ILogger<InventoryItemApiClient>>(), ApiClientService);

      var sut = new InventoryLocationsLocalCache(apiClient, NewAppDataFileManager());

      var result = sut.UpdateLocalCache();

      result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("Shop 1", true)]
    [InlineData("Shop 2", true)]
    [InlineData("Some invalid location", false)]
    [InlineData("Another invalid location", false)]
    public void Test_GetLocation(string title, bool foundInCache) {
      var apiClient = new InventoryItemApiClient(A.Fake<ILogger<InventoryItemApiClient>>(), ApiClientService);

      var sut = new InventoryLocationsLocalCache(apiClient, NewAppDataFileManager());

      var result = sut.GetLocationByTitle(title);

      result.IsSuccess.Should().Be(foundInCache);
      if (foundInCache) {
        result.Value.Title.Should().Be(title);
      }
    }
  }
}