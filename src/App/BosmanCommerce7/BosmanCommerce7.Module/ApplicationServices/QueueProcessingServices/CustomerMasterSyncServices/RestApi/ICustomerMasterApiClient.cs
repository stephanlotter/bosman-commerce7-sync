/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.RestApi
{

    public interface ICustomerMasterApiClient {

    Result<CustomerMasterResponse> GetCustomerMasterByEmail(string emailAddress);

    Result<CustomerMasterResponse> GetCustomerMasterById(Commerce7CustomerId commerce7CustomerId);

    Result<CustomerMasterResponse> CreateCustomerWithAddress(CreateCustomerRecord customerRecord);

    Result<CustomerMasterResponse> UpdateCustomerWithAddress(UpdateCustomerRecord customerRecord);
  }
}