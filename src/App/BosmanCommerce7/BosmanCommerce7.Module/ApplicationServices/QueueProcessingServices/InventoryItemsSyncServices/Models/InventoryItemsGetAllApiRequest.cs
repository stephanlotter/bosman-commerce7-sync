/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-26
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Models.RestApi;
using RestSharp;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models {
  public record InventoryItemsGetAllApiRequest : ApiRequestBase {
    public InventoryItemsGetAllApiRequest() {
      Resource = $"/product";
      Method = Method.Get;
      IsPagedResponse = false;
    }
  }
}