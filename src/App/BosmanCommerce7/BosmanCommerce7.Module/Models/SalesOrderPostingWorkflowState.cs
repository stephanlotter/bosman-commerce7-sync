/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-06-05
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.Models {

  public enum SalesOrderPostingWorkflowState {
    New = 0,
    OrderPosted = 10,
    PaymentPosted = 20,
    TipPosted = 30
  }
}