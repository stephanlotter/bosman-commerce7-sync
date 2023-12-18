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

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices.Models {
  public record InventoryLevelsResetApiRequest : ApiRequestBase {
    private readonly ResetInventoryLevelsRecord _resetInventoryLevelsRecord;

    public override object? Data => _resetInventoryLevelsRecord;

    public InventoryLevelsResetApiRequest(ResetInventoryLevelsRecord resetInventoryLevelsRecord) {
      Resource = $"/inventory-transaction";
      Method = Method.Post;
      IsPagedResponse = false;
      _resetInventoryLevelsRecord = resetInventoryLevelsRecord;
    }
  }
}