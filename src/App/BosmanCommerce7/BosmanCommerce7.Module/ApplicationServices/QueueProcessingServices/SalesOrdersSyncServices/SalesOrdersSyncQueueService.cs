/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public class SalesOrdersSyncQueueService : SyncQueueServiceBase, ISalesOrdersSyncQueueService {
    private readonly ISalesOrdersSyncService _salesOrdersSyncService;

    public SalesOrdersSyncQueueService(ILogger<SalesOrdersSyncQueueService> logger, ISalesOrdersSyncService salesOrdersSyncService) : base(logger) {
      _salesOrdersSyncService = salesOrdersSyncService;
    }

    protected override void ProcessQueue() {
      var context = new SalesOrdersSyncContext();
      _salesOrdersSyncService.Execute(context);
    }

  }

}
