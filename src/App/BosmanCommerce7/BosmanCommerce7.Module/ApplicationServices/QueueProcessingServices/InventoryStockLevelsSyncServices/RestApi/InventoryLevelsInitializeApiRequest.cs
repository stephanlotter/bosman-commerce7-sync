/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices.Models;
using BosmanCommerce7.Module.Models.RestApi;
using RestSharp;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices.RestApi {
  public record InventoryLevelsInitializeApiRequest : ApiRequestBase {
    private readonly InitializeInventoryLevelsRecord _initializeInventoryLevelsRecord;

    public override object? Data => _initializeInventoryLevelsRecord;

    public InventoryLevelsInitializeApiRequest(InitializeInventoryLevelsRecord initializeInventoryLevelsRecord) {
      Resource = $"/inventory";
      Method = Method.Post;
      IsPagedResponse = false;
      _initializeInventoryLevelsRecord = initializeInventoryLevelsRecord;
    }
  }
}