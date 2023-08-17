/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.LocalDatabase;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public interface ISalesOrdersQueueRepository {

    Result<SalesOrdersQueueItemDto[]> LoadPendingQueueItems();

    Result UpdateStatus(int oid, QueueItemStatus queueItemStatus, string? syncStatusDescription = null);

  }
}