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
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public class SalesOrdersSyncService : SyncServiceBase, ISalesOrdersSyncService {
    private readonly IValueStoreRepository _valueStoreRepository;

    public SalesOrdersSyncService(ILogger<SalesOrdersSyncService> logger, IValueStoreRepository valueStoreRepository) : base(logger) {
      _valueStoreRepository = valueStoreRepository;
    }

    public Result<SalesOrdersSyncResult> Execute(SalesOrdersSyncContext context) {
      // TODO: Implement this

      // TODO: Fetch orders from C7

      return Result.Success(BuildResult());

      SalesOrdersSyncResult BuildResult() {
        return new SalesOrdersSyncResult { };
      }

    }

  }

}

