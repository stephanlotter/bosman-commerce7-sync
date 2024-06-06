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
    Posting = 1,
    Retrying = 10,
    Posted = 100,
    Skipped = 110,
    Cancelled = 190,
    Failed = 200
  }
}