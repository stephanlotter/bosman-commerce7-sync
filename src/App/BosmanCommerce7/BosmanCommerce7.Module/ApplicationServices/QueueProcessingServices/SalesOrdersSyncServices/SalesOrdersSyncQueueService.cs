/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public class SalesOrdersSyncQueueService : SyncQueueServiceBase, ISalesOrdersSyncQueueService {
    private readonly ISalesOrdersQueueRepository _salesOrdersQueueRepository;
    private readonly ISalesOrdersSyncService _salesOrdersSyncService;

    public SalesOrdersSyncQueueService(ILogger<SalesOrdersSyncQueueService> logger,
      IOptions<SalesOrdersSyncJobOptions> options,
      ISalesOrdersQueueRepository salesOrdersQueueRepository,
      ISalesOrdersSyncService salesOrdersSyncService) : base(logger) {
      _salesOrdersQueueRepository = salesOrdersQueueRepository;
      _salesOrdersSyncService = salesOrdersSyncService;
    }

    protected override void ProcessQueue() {
      // TODO: Implement this
    }

  }

}
