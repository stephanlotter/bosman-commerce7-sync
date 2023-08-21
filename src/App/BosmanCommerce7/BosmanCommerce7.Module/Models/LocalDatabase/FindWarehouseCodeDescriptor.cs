/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

namespace BosmanCommerce7.Module.Models.LocalDatabase {
  public record FindWarehouseCodeDescriptor {

    public bool IsStoreOrder { get; init; }

    public string? PostalCode { get; init; }

  }

}
