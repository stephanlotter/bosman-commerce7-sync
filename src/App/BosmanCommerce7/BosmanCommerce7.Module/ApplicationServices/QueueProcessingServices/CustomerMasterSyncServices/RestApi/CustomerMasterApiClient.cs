/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.RestApiClients;
using BosmanCommerce7.Module.Models.RestApi;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.RestApi
{

    public class CustomerMasterApiClient : ApiClientBase, ICustomerMasterApiClient {

    public CustomerMasterApiClient(ILogger<CustomerMasterApiClient> logger, IApiClientService apiClientService) : base(logger, apiClientService) {
    }

    public Result<CustomerMasterResponse> GetCustomerMasterByEmail(string emailAddress) {
      CustomerMasterResponse apiResponse = new();
      var list = new List<dynamic>();

      return SendRequest(new CustomerMasterGetApiRequest(emailAddress), data => {
        if (data == null) {
          return (Result.Failure<CustomerMasterResponse>("Response body not valid JSON."), ApiRequestPaginationStatus.Completed);
        }

        var totalRecords = (int)data!.total;

        foreach (var customer in data!.customers) { list.Add(customer); }

        apiResponse = apiResponse with { CustomerMasters = list.ToArray() };

        return (Result.Success(apiResponse), list.Count < totalRecords ? ApiRequestPaginationStatus.MorePages : ApiRequestPaginationStatus.Completed);
      });
    }

    public Result<CustomerMasterResponse> GetCustomerMasterById(Commerce7CustomerId commerce7CustomerId) {
      //CustomerMasterResponse apiResponse = new();
      //var list = new List<dynamic>();

      //return SendRequest(new CustomerMasterGetApiRequest(commerce7CustomerId), data => {
      //  if (data == null) {
      //    return (Result.Failure<CustomerMasterResponse>("Response body not valid JSON."), ApiRequestPaginationStatus.Completed);
      //  }

      //  var totalRecords = (int)data!.total;

      //  list.Add(data);

      //  apiResponse = apiResponse with { CustomerMasters = list.ToArray() };

      //  return (Result.Success(apiResponse), ApiRequestPaginationStatus.Completed);
      //});

      return SendSingleResultRequest(new CustomerMasterGetApiRequest(commerce7CustomerId));
    }

    public Result<CustomerMasterResponse> CreateCustomerWithAddress(CreateCustomerRecord customerRecord) {
      return SendSingleResultRequest(new CustomerMasterCreateApiRequest(customerRecord));
    }

    public Result<CustomerMasterResponse> UpdateCustomerWithAddress(UpdateCustomerRecord customerRecord) {
      return SendSingleResultRequest(new CustomerMasterUpdateApiRequest(customerRecord));
    }

    public Result<CustomerMasterResponse> SendSingleResultRequest(ApiRequestBase apiRequest) {
      CustomerMasterResponse apiResponse = new();
      var list = new List<dynamic>();

      return SendRequest(apiRequest, data => {
        if (data == null) {
          return (Result.Failure<CustomerMasterResponse>("Response body not valid JSON."), ApiRequestPaginationStatus.Completed);
        }

        var totalRecords = (int)data!.total;

        list.Add(data);

        apiResponse = apiResponse with { CustomerMasters = list.ToArray() };

        return (Result.Success(apiResponse), ApiRequestPaginationStatus.Completed);
      });
    }
  }
}