﻿/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.RestApiClients;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.RestApi {

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

        apiResponse = apiResponse with { Data = list.ToArray() };

        return (Result.Success(apiResponse), list.Count < totalRecords ? ApiRequestPaginationStatus.MorePages : ApiRequestPaginationStatus.Completed);
      });
    }

    private ResponseBase NewCustomerMasterResponse() => new CustomerMasterResponse();

    private ResponseBase NewCustomerAddressResponse() => new CustomerAddressResponse();

    public Result<CustomerMasterResponse> GetCustomerMasterById(Commerce7CustomerId commerce7CustomerId) {
      return SendSingleResultRequest(new CustomerMasterGetApiRequest(commerce7CustomerId), NewCustomerMasterResponse).Map(r => (CustomerMasterResponse)r);
    }

    public Result<CustomerMasterResponse> CreateCustomerWithAddress(CreateCustomerRecord customerRecord) {
      return SendSingleResultRequest(new CustomerMasterCreateApiRequest(customerRecord), NewCustomerMasterResponse).Map(r => (CustomerMasterResponse)r);
    }

    public Result<CustomerMasterResponse> UpdateCustomer(UpdateCustomerRecord customerRecord) {
      return SendSingleResultRequest(new CustomerMasterUpdateApiRequest(customerRecord), NewCustomerMasterResponse).Map(r => (CustomerMasterResponse)r);
    }

    public Result<CustomerAddressResponse> GetCustomerDefaultAddress(Commerce7CustomerId commerce7CustomerId) {
      return SendListResultRequest(new CustomerAddressGetApiRequest(commerce7CustomerId), NewCustomerAddressResponse, "customerAddresses").Map(r => (CustomerAddressResponse)r);
    }

    public Result<CustomerAddressResponse> UpdateCustomerAddress(UpdateCustomerAddressRecord customerAddressRecord) {
      return SendSingleResultRequest(new CustomerAddressUpdateApiRequest(customerAddressRecord), NewCustomerAddressResponse).Map(r => (CustomerAddressResponse)r);
    }
  }
}