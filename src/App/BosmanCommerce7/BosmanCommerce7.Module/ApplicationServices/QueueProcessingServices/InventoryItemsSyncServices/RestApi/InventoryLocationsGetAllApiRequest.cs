/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-01-25
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Models.RestApi;
using RestSharp;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi {
  public record InventoryLocationsGetAllApiRequest : ApiRequestBase {
    public InventoryLocationsGetAllApiRequest() {
      Resource = $"/inventory-location";
      Method = Method.Get;
      IsPagedResponse = false;
    }
  }
}