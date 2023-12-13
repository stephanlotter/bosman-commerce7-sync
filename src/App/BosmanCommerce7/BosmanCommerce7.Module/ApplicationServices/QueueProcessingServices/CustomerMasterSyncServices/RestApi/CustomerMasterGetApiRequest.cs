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

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.RestApi {
  public record CustomerMasterGetApiRequest : ApiRequestBase {
    public CustomerMasterGetApiRequest(string emailAddress) {
      if (string.IsNullOrWhiteSpace(emailAddress)) { throw new ArgumentNullException(nameof(emailAddress)); }

      Resource = $"/customer?q={emailAddress}";
      Method = Method.Get;
      IsPagedResponse = true;
    }

    public CustomerMasterGetApiRequest(Commerce7CustomerId commerce7CustomerId) {
      if (commerce7CustomerId == Commerce7CustomerId.Empty) { throw new Exception("commerce7CustomerId may not be empty"); }
      Resource = $"/customer/{commerce7CustomerId}";
      Method = Method.Get;
      IsPagedResponse = false;
    }
  }
}