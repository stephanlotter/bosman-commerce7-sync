/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.AppDataServices;
using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.RestApiClients.SalesOrders;
using BosmanCommerce7.Module.BusinessObjects.Customers;
using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices {

  public class CustomerMasterSyncService : SyncServiceBase, ICustomerMasterSyncService {
    private readonly CustomerMasterSyncJobOptions _customerMasterSyncJobOptions;
    private readonly ICustomerMasterApiClient _apiClient;
    private readonly ILocalObjectSpaceProvider _localObjectSpaceProvider;
    private readonly IAppDataFileManager _appDataFileManager;

    public CustomerMasterSyncService(ILogger<CustomerMasterSyncService> logger,
      CustomerMasterSyncJobOptions customerMasterSyncJobOptions,
      ICustomerMasterApiClient apiClient,
      ILocalObjectSpaceProvider localObjectSpaceProvider,
      IAppDataFileManager appDataFileManager
      ) : base(logger) {
      _customerMasterSyncJobOptions = customerMasterSyncJobOptions;
      _apiClient = apiClient;
      _localObjectSpaceProvider = localObjectSpaceProvider;
      _appDataFileManager = appDataFileManager;
    }

    public Result<CustomerMasterSyncResult> Execute(CustomerMasterSyncContext context) {
      var errorCount = 0;

      _localObjectSpaceProvider.WrapInObjectSpaceTransaction(objectSpace => {
        CriteriaOperator? criteria = context.Criteria;

        if (criteria is null) {
          var criteriaPostingStatus = "Status".InCriteriaOperator(QueueProcessingStatus.New, QueueProcessingStatus.Retrying);
          var criteriaRetryAfter = CriteriaOperator.Or("RetryAfter".IsNullOperator(), "RetryAfter".PropertyLessThan(DateTime.Now));
          criteria = CriteriaOperator.And(criteriaPostingStatus, criteriaRetryAfter);
        }

        var queueItems = objectSpace.GetObjects<CustomerUpdateQueue>(criteria).ToList();

        if (!queueItems.Any()) {
          Logger.LogDebug("No sales orders to post");
          return;
        }

        foreach (var queueItem in queueItems) {
          // TODO:  Add sync logic here

          // TODO: Store the JSON request in the AppData folder (_appDataFileManager)
        }
      });

      return errorCount == 0 ? Result.Success(BuildResult()) : Result.Failure<CustomerMasterSyncResult>($"Completed with {errorCount} errors.");

      CustomerMasterSyncResult BuildResult() {
        return new CustomerMasterSyncResult { };
      }
    }
  }
}