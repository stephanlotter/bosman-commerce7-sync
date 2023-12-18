/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Models.RestApi;
using RestSharp;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.RestApi {
  public record CustomerAddressGetApiRequest : ApiRequestBase {
    public CustomerAddressGetApiRequest(Commerce7CustomerId customerId) {
      Resource = $"/customer/{customerId}/address";
      Method = Method.Get;
      IsPagedResponse = false;
      CustomerId = customerId;
    }

    public Commerce7InventoryId CustomerId { get; }
  }
}