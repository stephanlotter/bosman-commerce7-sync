/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models {
  public record SalesOrdersDescriptor {

    public string SimpleCode { get; init; } = default!;

    public int MaterialId { get; init; }

    public int ItemMasterId { get; init; }

  }

}
