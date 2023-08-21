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
    private readonly ISalesOrdersPostService _salesOrdersPostService;

    public SalesOrdersPostQueueService(ILogger<SalesOrdersPostQueueService> logger, ISalesOrdersPostService salesOrdersPostService) : base(logger) {
      _salesOrdersPostService = salesOrdersPostService;
    }

    protected override void ProcessQueue() {
      var context = new SalesOrdersPostContext();
      var result = _salesOrdersPostService.Execute(context);
    }
  }

}
