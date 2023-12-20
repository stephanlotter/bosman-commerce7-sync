/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-20
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models {
  public record InventoryLocationRecord {
    public Commerce7LocationId Id { get; init; }

    public required Commerce7LocationTitle Title { get; init; }
  }
}