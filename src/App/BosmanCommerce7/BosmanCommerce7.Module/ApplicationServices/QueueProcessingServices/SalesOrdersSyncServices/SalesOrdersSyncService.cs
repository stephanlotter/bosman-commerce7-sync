/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public class SalesOrdersSyncService : SyncServiceBase, ISalesOrdersSyncService {

    public SalesOrdersSyncService(ILogger<SalesOrdersSyncService> logger) : base(logger) {
    }

    public Result<SalesOrdersSyncResult> Execute(SalesOrdersSyncContext context) {

      return Result.Success(BuildResult());

      SalesOrdersSyncResult BuildResult() {
        return new SalesOrdersSyncResult { };
      }

    }

  }

}

