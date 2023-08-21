/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.LocalDatabase;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public class WarehouseRepository : LocalDatabaseRepositoryBase, IWarehouseRepository {
    private readonly IValueStoreRepository _valueStoreRepository;

    public WarehouseRepository(ILogger<WarehouseRepository> logger, ILocalDatabaseConnectionStringProvider connectionStringProvider, IValueStoreRepository valueStoreRepository) : base(logger, connectionStringProvider) {
      _valueStoreRepository = valueStoreRepository;
    }

    public Result<string?> FindWarehouseCode(FindWarehouseCodeDescriptor findWarehouseCodeDescriptor) {

      if (!findWarehouseCodeDescriptor.IsStoreOrder) {
        return _valueStoreRepository.GetValue("default-ship-from-warehouse-code");
      }

      // TODO: Add BO for mapping postal codes to warehouse codes

      // TODO: Implement logic to find the warehouse code for findWarehouseCodeDescriptor.PostalCode



      return Result.Success<string?>(null);
    }

  }

}
