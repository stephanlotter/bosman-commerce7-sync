﻿/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients.SalesOrders {

  public interface ICustomerMasterApiClient {

    Result<CustomerMasterResponse> ListCustomerMaster(string emailAddress);

    // TODO: Add Create method
    // TODO: Add Update method
  }
}