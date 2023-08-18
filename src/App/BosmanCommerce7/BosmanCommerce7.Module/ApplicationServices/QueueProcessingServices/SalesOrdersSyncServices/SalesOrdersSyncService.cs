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
using BosmanCommerce7.Module.ApplicationServices.RestApiClients;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public class SalesOrdersSyncService : SyncServiceBase, ISalesOrdersSyncService {
    private const string ValueStoreKey = "sales-orders-sync-last-synced";

    private readonly SalesOrdersSyncJobOptions _salesOrdersSyncJobOptions;
    private readonly IValueStoreRepository _valueStoreRepository;
    private readonly ISalesOrdersApiClient _apiClient;

    public SalesOrdersSyncService(ILogger<SalesOrdersSyncService> logger, SalesOrdersSyncJobOptions salesOrdersSyncJobOptions, IValueStoreRepository valueStoreRepository, ISalesOrdersApiClient apiClient) : base(logger) {
      _salesOrdersSyncJobOptions = salesOrdersSyncJobOptions;
      _valueStoreRepository = valueStoreRepository;
      _apiClient = apiClient;
    }

    public Result<SalesOrdersSyncResult> Execute(SalesOrdersSyncContext context) {
      var lastSynced = _valueStoreRepository.GetDateTimeValue(ValueStoreKey, _salesOrdersSyncJobOptions.StartDate);

      if (lastSynced.IsFailure) {
        Logger.LogError("Unable to execute SalesOrdersSyncService: {error}", lastSynced.Error);
        return Result.Failure<SalesOrdersSyncResult>(lastSynced.Error);
      }

      var orderSubmittedDate = (lastSynced.Value ?? DateTime.Now).AddDays(-_salesOrdersSyncJobOptions.FetchNumberOfDaysBack).Date;

      Logger.LogInformation("Fetching sales orders since: {orderSubmittedDate}", $"{orderSubmittedDate:yyyy-MM-dd HH:mm:ss}");

      var salesOrdersResult = _apiClient.GetSalesOrders(orderSubmittedDate);

      if (salesOrdersResult.IsFailure) {
        Logger.LogError("Unable to execute SalesOrdersSyncService: {error}", salesOrdersResult.Error);
        return Result.Failure<SalesOrdersSyncResult>(salesOrdersResult.Error);
      }

      var response = salesOrdersResult.Value;

      if (response.SalesOrders == null || response.SalesOrders!.Count == 0) {
        Logger.LogInformation("No sales orders found since: {orderSubmittedDate}", $"{orderSubmittedDate:yyyy-MM-dd HH:mm:ss}");
        return Result.Success(BuildResult());
      }

      foreach (dynamic salesOrder in response.SalesOrders!) {
        Logger.LogInformation("Sales order found: {orderNumber}", $"{salesOrder.orderNumber}");
      }

      // TODO: if order retrieval and storage is successful, update the last synced date

      return Result.Success(BuildResult());

      SalesOrdersSyncResult BuildResult() {
        return new SalesOrdersSyncResult { };
      }

    }

  }

}

