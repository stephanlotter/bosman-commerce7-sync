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
  public class SalesOrdersPostService : SyncServiceBase, ISalesOrdersPostService {

    public SalesOrdersPostService(ILogger<SalesOrdersPostService> logger) : base(logger) {
    }

    public Result<SalesOrdersPostResult> Execute(SalesOrdersPostContext context) {
      // TODO: Implement this

      return Result.Success(BuildResult());

      SalesOrdersPostResult BuildResult() {
        return new SalesOrdersPostResult { };
      }

    }

  }

}

