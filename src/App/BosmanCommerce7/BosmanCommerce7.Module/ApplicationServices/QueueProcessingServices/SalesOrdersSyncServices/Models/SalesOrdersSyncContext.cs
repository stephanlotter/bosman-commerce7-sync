/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models.LocalDatabase;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models {
  public record SalesOrdersSyncContext {

    public SalesOrdersQueueItemDto SalesOrdersQueueItem { get; init; } = default!;

    //public EvolutionInventoryDto EvolutionInventory { get; init; } = default!;

  }

}
