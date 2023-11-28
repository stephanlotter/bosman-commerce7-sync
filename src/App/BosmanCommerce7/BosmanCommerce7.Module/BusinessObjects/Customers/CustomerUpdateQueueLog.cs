/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.BusinessObjects.Customers {
  [DefaultClassOptions]
  public class CustomerUpdateQueueLog : UpdateQueueLogBase {
    private CustomerUpdateQueue _customerUpdateQueue = default!;

    [Association("CustomerUpdateQueue-CustomerUpdateQueueLog")]
    public CustomerUpdateQueue CustomerUpdateQueue {
      get => _customerUpdateQueue;
      set => SetPropertyValue(nameof(CustomerUpdateQueue), ref _customerUpdateQueue, value);
    }

    public CustomerUpdateQueueLog(Session session) : base(session) {
    }
  }
}