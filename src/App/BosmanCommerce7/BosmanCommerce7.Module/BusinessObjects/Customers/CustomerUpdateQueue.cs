/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.BusinessObjects.Customers {

  [DefaultClassOptions]
  [NavigationItem(true)]
  public class CustomerUpdateQueue : UpdateQueueBase {
    private int _customerId;

    [ModelDefault("AllowEdit", "false")]
    public int CustomerId {
      get => _customerId;
      set => SetPropertyValue(nameof(CustomerId), ref _customerId, value);
    }

    [Association("CustomerUpdateQueue-CustomerUpdateQueueLog")]
    [Aggregated]
    public XPCollection<CustomerUpdateQueueLog> CustomerUpdateQueueLogs => GetCollection<CustomerUpdateQueueLog>(nameof(CustomerUpdateQueueLogs));

    public CustomerUpdateQueue(Session session) : base(session) {
    }

    protected override UpdateQueueLogBase CreateLogEntry() {
      var log = new CustomerUpdateQueueLog(Session);
      CustomerUpdateQueueLogs.Add(log);
      return log;
    }
  }
}