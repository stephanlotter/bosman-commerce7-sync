/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.Models {

  public enum QueueProcessingStatus {
    New = 0,
    Retrying = 10,
    Processed = 100,
    Skipped = 110,
    Failed = 200
  }
}