/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using Bogus;
using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.LocalDatabase;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public class FakeSalesOrdersQueueRepository : ISalesOrdersQueueRepository {

    public Result<SalesOrdersQueueItemDto[]> LoadPendingQueueItems() {
      var result = new Faker<SalesOrdersQueueItemDto>()
        .RuleFor(a => a.OID, f => ++f.IndexVariable)
        .RuleFor(a => a.SimpleCode, f => $"I{f.IndexFaker:0000}")
        .RuleFor(a => a.QueueItemStatus, f => QueueItemStatus.New)
        .RuleFor(a => a.RetryCount, 0);

      return Result.Success(result.Generate(2).ToArray());
    }

    public Result UpdateStatus(int oid, QueueItemStatus queueItemStatus, string? syncStatusDescription = null) {
      return Result.Success();
    }
  }

}
