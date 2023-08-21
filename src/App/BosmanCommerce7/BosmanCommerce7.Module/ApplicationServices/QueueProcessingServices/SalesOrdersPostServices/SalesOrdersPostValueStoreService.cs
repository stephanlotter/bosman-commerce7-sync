/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices {
  public class SalesOrdersPostValueStoreService : ISalesOrdersPostValueStoreService {

    private readonly IValueStoreRepository _valueStoreRepository;

    public SalesOrdersPostValueStoreService(IValueStoreRepository valueStoreRepository) {
      _valueStoreRepository = valueStoreRepository;
    }

    public Result<string?> GetDefaultSalesRepresentativeCode() {
      return _valueStoreRepository.GetValue("default-sales-rep-code");
    }

    public Result<string?> GetDefaultDeliveryMethodCode() {
      return _valueStoreRepository.GetValue("default-delivery-method-description");
    }

  }

}

