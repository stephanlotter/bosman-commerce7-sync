/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices {

  public class SalesOrdersPostQueueService : SyncQueueServiceBase, ISalesOrdersPostQueueService {
    private readonly ISalesOrdersPostWorkflowService _salesOrdersPostWorkflowService;

    public SalesOrdersPostQueueService(ILogger<SalesOrdersPostQueueService> logger, ISalesOrdersPostWorkflowService salesOrdersPostWorkflowService) : base(logger) {
      _salesOrdersPostWorkflowService = salesOrdersPostWorkflowService;
    }

    protected override void ProcessQueue() {
      var context = new SalesOrdersPostContext();
      var result = _salesOrdersPostWorkflowService.Execute(context);
    }
  }
}