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

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.RestApi {
  public record CustomerMasterUpdateApiRequest : ApiRequestBase {
    private readonly UpdateCustomerRecord _updateCustomerRecord;

    override public object? Data => _updateCustomerRecord;

    public CustomerMasterUpdateApiRequest(UpdateCustomerRecord updateCustomerRecord) {
      Resource = $"/customer/{updateCustomerRecord.Id}";
      Method = Method.Put;
      IsPagedResponse = false;
      _updateCustomerRecord = updateCustomerRecord;
    }
  }
}