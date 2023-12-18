/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models;
using BosmanCommerce7.Module.Models.RestApi;
using RestSharp;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.RestApi {
  public record CustomerAddressUpdateApiRequest : ApiRequestBase {
    private readonly UpdateCustomerAddressRecord _updateCustomerAddressRecord;

    override public object? Data => _updateCustomerAddressRecord;

    public CustomerAddressUpdateApiRequest(UpdateCustomerAddressRecord updateCustomerAddressRecord) {
      Resource = $"/customer/{updateCustomerAddressRecord.Id}/address/{updateCustomerAddressRecord.AddressId}";
      Method = Method.Put;
      IsPagedResponse = false;
      _updateCustomerAddressRecord = updateCustomerAddressRecord;
    }
  }
}