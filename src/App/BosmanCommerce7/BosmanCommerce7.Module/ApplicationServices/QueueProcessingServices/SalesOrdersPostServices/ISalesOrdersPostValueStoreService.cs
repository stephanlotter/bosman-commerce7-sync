/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices {
  public interface ISalesOrdersPostValueStoreService {

    Result<string?> GetDefaultSalesRepresentativeCode();

    Result<string?> GetDefaultDeliveryMethodCode();

  }
}