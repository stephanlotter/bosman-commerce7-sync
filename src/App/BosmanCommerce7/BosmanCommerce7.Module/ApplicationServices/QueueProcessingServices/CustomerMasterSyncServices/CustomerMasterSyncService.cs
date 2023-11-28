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
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.RestApiClients.SalesOrders;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
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
      // TODO:  Add sync logic here

      // TODO: Store the JSON request in the AppData folder (_appDataFileManager)


      try {

      return Result.Success(new CustomerMasterSyncResult());
      }
      catch (Exception ex) {
                Logger.LogError("Unable to execute SalesOrdersSyncService: {error}", ex);
        return Result.Failure<CustomerMasterSyncResult>(ex.Message);

        
      }

    }
  }
}