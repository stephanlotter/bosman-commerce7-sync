/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-06
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using DevExpress.Data.Filtering;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices {
  public abstract record SyncContextBase(CriteriaOperator? Criteria = null);
}