/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-08
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models;
using BosmanCommerce7.Module.Models.RestApi;
using RestSharp;

namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients.SalesOrders {
  public record CustomerMasterCreateApiRequest : ApiRequestBase {
    private readonly CreateCustomerRecord _createCustomerRecord;

    override public object? Data => _createCustomerRecord;

    public CustomerMasterCreateApiRequest(CreateCustomerRecord createCustomerRecord) {
      Resource = $"/customer-address";
      Method = Method.Post;
      IsPagedResponse = false;
      _createCustomerRecord = createCustomerRecord;
    }
  }
}