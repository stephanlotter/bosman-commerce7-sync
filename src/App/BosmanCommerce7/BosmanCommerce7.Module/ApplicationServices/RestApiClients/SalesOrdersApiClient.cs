/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models;
using BosmanCommerce7.Module.Models.RestApi;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients {
  public class SalesOrdersApiClient : ApiClientBase, ISalesOrdersApiClient {

    public SalesOrdersApiClient(ILogger<SalesOrdersApiClient> logger, IApiClientService apiClientService) : base(logger, apiClientService) {

    }

    public Result<SalesOrdersSyncResponse> GetSalesOrders(DateTime orderSubmittedDate) {
      return SendRequest(new SalesOrdersSyncApiRequest(orderSubmittedDate), data => {
        if (data == null) {
          return Result.Failure<SalesOrdersSyncResponse>("Response body not valid JSON.");
        }

        var apiResponse = new SalesOrdersSyncResponse {
          SalesOrders = data!.orders == null ? null : data!.orders
        };

        return Result.Success(apiResponse);

      });
    }

  }

}
