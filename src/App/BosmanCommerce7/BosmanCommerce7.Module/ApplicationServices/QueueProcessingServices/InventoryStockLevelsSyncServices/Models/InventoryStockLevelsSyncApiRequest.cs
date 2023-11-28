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

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices.Models {
  public record InventoryStockLevelsSyncApiRequest : ApiRequestBase {
    public InventoryStockLevelsSyncApiRequest(DateTime inventoryCheckDate) {
      Resource = $"/inventory?inventoryCheckDate=gt:{inventoryCheckDate:yyyy-MM-dd}";
      Method = Method.Get;
      IsPagedResponse = true;
    }
  }
}