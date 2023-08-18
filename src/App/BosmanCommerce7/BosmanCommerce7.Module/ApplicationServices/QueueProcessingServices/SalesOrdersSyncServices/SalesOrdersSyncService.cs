/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public class SalesOrdersSyncService : SyncServiceBase, ISalesOrdersSyncService {
    private const string ValueStoreKey = "sales-orders-sync-last-synced";
    private readonly SalesOrdersSyncJobOptions _salesOrdersSyncJobOptions;
    private readonly IValueStoreRepository _valueStoreRepository;

    public SalesOrdersSyncService(ILogger<SalesOrdersSyncService> logger, SalesOrdersSyncJobOptions salesOrdersSyncJobOptions, IValueStoreRepository valueStoreRepository) : base(logger) {
      _salesOrdersSyncJobOptions = salesOrdersSyncJobOptions;
      _valueStoreRepository = valueStoreRepository;
    }

    public Result<SalesOrdersSyncResult> Execute(SalesOrdersSyncContext context) {
      var lastSynced = _valueStoreRepository.GetDateTimeValue(ValueStoreKey, _salesOrdersSyncJobOptions.StartDate);

      if (lastSynced.IsFailure) {
        Logger.LogError("Unable to execute SalesOrdersSyncService: {error}", lastSynced.Error);
        return Result.Failure<SalesOrdersSyncResult>(lastSynced.Error);
      }

      var orderSubmittedDate = (lastSynced.Value.HasValue ? lastSynced.Value.Value : DateTime.Now).AddDays(-_salesOrdersSyncJobOptions.FetchNumberOfDaysBack).Date;

      Logger.LogInformation("Fetching sales orders since: {orderSubmittedDate}", $"{orderSubmittedDate:yyyy-MM-dd HH:mm:ss}");

      // TODO: Implement this

      // TODO: Fetch orders from C7
      // /order?orderSubmittedDate=gt:2023-08-01

      // TODO: if order retrieval and storage is successful, update the last synced date

      return Result.Success(BuildResult());

      SalesOrdersSyncResult BuildResult() {
        return new SalesOrdersSyncResult { };
      }

    }

  }

}

