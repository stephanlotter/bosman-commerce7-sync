﻿/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.RestApiClients;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.RestApi
{

    public class SalesOrdersApiClient : ApiClientBase, ISalesOrdersApiClient {

    public SalesOrdersApiClient(ILogger<SalesOrdersApiClient> logger, IApiClientService apiClientService) : base(logger, apiClientService) {
    }

    public Result<SalesOrdersSyncResponse> GetSalesOrders(DateTime orderSubmittedDate) {
      SalesOrdersSyncResponse apiResponse = new();
      var list = new List<dynamic>();

      return SendRequest(new SalesOrdersSyncApiRequest(orderSubmittedDate), data => {
        if (data == null) {
          return (Result.Failure<SalesOrdersSyncResponse>("Response body not valid JSON."), ApiRequestPaginationStatus.Completed);
        }

        var totalRecords = (int)data!.total;

        foreach (var order in data!.orders) { list.Add(order); }

        apiResponse = apiResponse with { Data = list.ToArray() };

        return (Result.Success(apiResponse), list.Count < totalRecords ? ApiRequestPaginationStatus.MorePages : ApiRequestPaginationStatus.Completed);
      });
    }
  }
}