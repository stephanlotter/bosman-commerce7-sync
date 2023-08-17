/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

namespace BosmanCommerce7.Module.Models {
  public enum QueueItemStatus {
    New = 10,
    Processed = 20,
    Retry = 30,
    Failed = 100
  }

}
