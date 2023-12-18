/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.RestApi {
  public record CustomerAddressResponse : ResponseBase {
    public bool HasData => Data?.Length > 0;

    public dynamic? DefaultAddress {
      get {
        return HasData ? Data?.FirstOrDefault(a => a.isDefault == true) ?? Data?[0] : null;
      }
    }
  }
}