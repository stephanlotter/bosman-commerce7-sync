/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using System.Text;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.Extensions.DataAccess;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using Dapper;
using DevExpress.ExpressApp.Core;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public class SalesOrdersLocalRepository : LocalDatabaseRepositoryBase, ISalesOrdersLocalRepository {
    private readonly IObjectSpaceFactory _objectSpaceFactory;

    public SalesOrdersLocalRepository(ILogger<SalesOrdersLocalRepository> logger, ILocalDatabaseConnectionStringProvider connectionStringProvider, IObjectSpaceFactory objectSpaceFactory)
      : base(logger, connectionStringProvider) {
      _objectSpaceFactory = objectSpaceFactory;
    }

    public Result<SalesOrder[]> LoadPendingSalesOrders() {
      var sql = $@"
select rmqi.OID,
       rmqi.SimpleCode,
       rmqi.QueueItemStatus,
       rmqi.RetryCount
    from SalesOrdersQueueItem rmqi
    where rmqi.QueueItemStatus in ({(int)QueueItemStatus.New}, {(int)QueueItemStatus.Retry})
        and IsNull(rmqi.SendAfter, rmqi.CreatedOn) <= getdate()
        and rmqi.RetryCount < 6
    order by rmqi.SendAfter

      ";
      try {
        var data = ConnectionStringProvider.LocalDatabase.WrapInOpenConnection(connection => {
          return connection.Query<SalesOrder>(sql).ToArray();
        });

        return Result.Success(data);
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While LoadPendingQueueItems");
        return Result.Failure<SalesOrder[]>(ex.Message);
      }

    }

    public Result UpdateStatus(int oid, QueueItemStatus queueItemStatus, string? syncStatusDescription = null) {
      var sb = new StringBuilder();
      sb.AppendLine("update SalesOrdersQueueItem ");
      sb.AppendLine("    set QueueItemStatus = @queueItemStatus,");
      sb.AppendLine("    ProcessedOn = getdate(),");
      sb.AppendLine("    SyncStatusDescription = @syncStatusDescription");

      if (queueItemStatus == QueueItemStatus.Retry) {
        sb.AppendLine("    ,SendAfter = dateadd(minute, RetryCount + 1, getdate())");
        sb.AppendLine("    ,RetryCount = RetryCount + 1");
      }
      else {
        sb.AppendLine("    ,SendAfter = getdate()");
        sb.AppendLine("    ,RetryCount = 0");
      }

      sb.AppendLine(" where OID=@oid;");

      try {
        ConnectionStringProvider.LocalDatabase.WrapInTransaction((connection, transaction) => {
          connection.Execute(sb.ToString(), new {
            queueItemStatus = (int)queueItemStatus,
            syncStatusDescription,
            oid
          }, transaction);
        });

        return Result.Success();
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While LoadPendingQueueItems");
        return Result.Failure(ex.Message);
      }
    }

  }

}
