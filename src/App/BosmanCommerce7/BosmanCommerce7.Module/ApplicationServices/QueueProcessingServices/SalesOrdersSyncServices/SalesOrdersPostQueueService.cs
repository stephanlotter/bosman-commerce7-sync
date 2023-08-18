/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public class SalesOrdersPostQueueService : SyncQueueServiceBase, ISalesOrdersPostQueueService {
    private readonly IOptions<SalesOrdersPostJobOptions> _options;

    public SalesOrdersPostQueueService(ILogger<SalesOrdersPostQueueService> logger,
      IOptions<SalesOrdersPostJobOptions> options) : base(logger) {
      _options = options;
    }

    protected override void ProcessQueue() {
      // TODO: Implement this
    }
  }

}
