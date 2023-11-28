/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Models.RestApi;
using RestSharp;

namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients.SalesOrders {
  public record CustomerMasterGetApiRequest : ApiRequestBase {
    public CustomerMasterGetApiRequest(string emailAddress) {
      Resource = $"/customer?q={emailAddress}";
      Method = Method.Get;
      IsPagedResponse = true;
    }
  }
}