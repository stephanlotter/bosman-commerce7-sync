/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

namespace BosmanCommerce7.Module.Extensions.QuartzTools {
  public static class JobIds {

    public const string JobServiceInstance = nameof(JobServiceInstance);

    public const string JobsGroup = nameof(JobsGroup);

    public const string CustomerMasterSyncJob = nameof(CustomerMasterSyncJob);

    public const string InventoryItemsSyncJob = nameof(InventoryItemsSyncJob);

    public const string InventoryLevelsSyncJob = nameof(InventoryLevelsSyncJob);

    public const string SalesOrdersSyncJob = nameof(SalesOrdersSyncJob);

    public const string SalesOrdersPostJob = nameof(SalesOrdersPostJob);

  }
}
