/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-19
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices {

  public interface IInventoryItemsLocalCache {

    Result UpdateLocalCache();

    Result<ProductRecord> GetProduct(Commerce7Sku sku);
  }
}