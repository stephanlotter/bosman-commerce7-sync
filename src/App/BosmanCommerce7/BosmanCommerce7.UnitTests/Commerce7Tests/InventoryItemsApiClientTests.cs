/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-19
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi;

namespace BosmanCommerce7.UnitTests.Commerce7Tests {

  public class InventoryItemsApiClientTests : Commerce7ClientApiTestsBase {

    [Fact]
    public void Fetch_all() {
      var sut = new InventoryItemApiClient(A.Fake<ILogger<InventoryItemApiClient>>(), ApiClientService);

      var result = sut.GetAllInventoryItems();

      result.IsSuccess.Should().BeTrue();

      //ProductRecord
    }
  }
}