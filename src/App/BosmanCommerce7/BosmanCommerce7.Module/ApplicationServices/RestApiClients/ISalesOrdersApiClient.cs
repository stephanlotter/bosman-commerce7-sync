/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients {
  public interface ISalesOrdersApiClient {
    Result<SalesOrdersSyncResponse> GetSalesOrders(DateTime orderSubmittedDate);
    
  }
}