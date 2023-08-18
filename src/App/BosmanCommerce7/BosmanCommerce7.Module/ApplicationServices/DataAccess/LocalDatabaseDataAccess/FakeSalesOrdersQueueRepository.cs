/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using Bogus;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public class FakeSalesOrdersQueueRepository : ISalesOrdersLocalRepository {

    public Result<SalesOrder[]> LoadPendingSalesOrders() {
      var result = new Faker<SalesOrder>()
        .RuleFor(a => a.Oid, f => ++f.IndexVariable)
        .RuleFor(a => a.OrderNumber, f => f.IndexFaker)
        .RuleFor(a => a.PostingStatus, f => SalesOrderPostingStatus.New)
        .RuleFor(a => a.RetryCount, 0);

      return Result.Success(result.Generate(2).ToArray());
    }

    public Result UpdateStatus(int oid, QueueItemStatus queueItemStatus, string? syncStatusDescription = null) {
      return Result.Success();
    }
  }

}
