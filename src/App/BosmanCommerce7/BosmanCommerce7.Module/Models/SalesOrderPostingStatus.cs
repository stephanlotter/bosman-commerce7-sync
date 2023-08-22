/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

namespace BosmanCommerce7.Module.Models {
  public enum SalesOrderPostingStatus {
    New = 0,
    Retrying = 10,
    Posted = 100,
    Failed = 200
  }

}
