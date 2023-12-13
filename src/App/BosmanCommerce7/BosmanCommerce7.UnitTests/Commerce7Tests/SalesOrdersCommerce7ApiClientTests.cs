/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.RestApiClients;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.RestApi;
using BosmanCommerce7.Module.Models;

namespace BosmanCommerce7.UnitTests.Commerce7Tests
{
    public class SalesOrdersApiClientApiClientTests {

    public SalesOrdersApiClientApiClientTests() {

    }

    [Fact]
    public void Fetch_sales_orders() {

      var apiOptions = new ApiOptions {
        AppId = "evolution-integration",
        AppSecretKey = Environment.GetEnvironmentVariable("ApplicationOptions__ApiOptions__AppSecretKey", EnvironmentVariableTarget.Machine),
        Endpoint = "https://api.commerce7.com/v1",
        TenantId = "bosman-family-vineyards"
      };

      var restClientFactory = new RestClientFactory(apiOptions);
      var apiClientService = new ApiClientService(A.Fake<ILogger<ApiClientService>>(), restClientFactory);

      var sut = new SalesOrdersApiClient(A.Fake<ILogger<SalesOrdersApiClient>>(), apiClientService);

      var result = sut.GetSalesOrders(new DateTime(2023, 8, 1));

      result.IsSuccess.Should().BeTrue();
    }

  }
}
