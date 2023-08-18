/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models;
using DevExpress.ExpressApp.Core;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public class SalesOrdersSyncQueueService : SyncQueueServiceBase, ISalesOrdersSyncQueueService {
    private readonly ISalesOrdersSyncService _salesOrdersSyncService;
    private readonly IObjectSpaceFactory _objectSpaceFactory;

    public SalesOrdersSyncQueueService(ILogger<SalesOrdersSyncQueueService> logger,
      ISalesOrdersSyncService salesOrdersSyncService,
      IObjectSpaceFactory objectSpaceFactory) : base(logger) {
      _salesOrdersSyncService = salesOrdersSyncService;
      _objectSpaceFactory = objectSpaceFactory;
    }

    protected override void ProcessQueue() {
      var context = new SalesOrdersSyncContext();
      _salesOrdersSyncService.Execute(context);
    }

  }

}
