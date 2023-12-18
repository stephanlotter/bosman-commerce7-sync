/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using Newtonsoft.Json;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models {
  public record UpdateCustomerAddressRecord : CustomerRecordBase {
    [JsonIgnore]
    public required Commerce7CustomerId Id { get; init; }

    [JsonIgnore]
    public required Commerce7CustomerAddressId AddressId { get; init; }
  }
}