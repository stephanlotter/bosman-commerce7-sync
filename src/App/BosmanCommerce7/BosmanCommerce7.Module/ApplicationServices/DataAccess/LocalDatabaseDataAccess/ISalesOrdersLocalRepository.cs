/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public interface ISalesOrdersLocalRepository {

    Result<SalesOrder[]> LoadPendingSalesOrders();

    Result UpdateStatus(int oid, QueueItemStatus queueItemStatus, string? syncStatusDescription = null);

  }
}