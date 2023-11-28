/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices {

  public class CustomerMasterSyncQueueService : SyncQueueServiceBase, ICustomerMasterSyncQueueService {
    private readonly ICustomerMasterSyncService _customerMasterSyncService;

    public CustomerMasterSyncQueueService(ILogger<CustomerMasterSyncQueueService> logger, ICustomerMasterSyncService customerMasterSyncService) : base(logger) {
      _customerMasterSyncService = customerMasterSyncService;
    }

    protected override void ProcessQueue() {
      var context = new CustomerMasterSyncContext();
      _customerMasterSyncService.Execute(context);
    }
  }
}