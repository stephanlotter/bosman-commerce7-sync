/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.RestApi;

namespace BosmanCommerce7.UnitTests.Commerce7Tests {

  public class SalesOrdersApiClientApiClientTests : Commerce7ClientApiTestsBase {

    public SalesOrdersApiClientApiClientTests() {
    }

    [Fact]
    public void Fetch_sales_orders() {
      var sut = new SalesOrdersApiClient(A.Fake<ILogger<SalesOrdersApiClient>>(), ApiClientService);

      var result = sut.GetSalesOrders(new DateTime(2023, 8, 1));

      result.IsSuccess.Should().BeTrue();
    }
  }
}