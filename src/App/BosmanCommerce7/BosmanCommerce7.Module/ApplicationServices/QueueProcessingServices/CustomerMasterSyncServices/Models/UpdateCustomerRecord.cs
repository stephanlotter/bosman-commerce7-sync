/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-08
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using Newtonsoft.Json;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models {
  public abstract record UpdateCustomerRecord : CustomerRecordBase {
    [JsonIgnore]
    public required Commerce7CustomerId Id { get; init; }
  }
}