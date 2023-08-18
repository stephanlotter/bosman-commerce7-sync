/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Extensions.DataAccess;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using Dapper;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public class ValueStoreRepository : LocalDatabaseRepositoryBase, IValueStoreRepository {
    public ValueStoreRepository(ILogger<ValueStoreRepository> logger, ILocalDatabaseConnectionStringProvider connectionStringProvider) : base(logger, connectionStringProvider) {
    }

    public Result<string?> GetValue(string keyName) {
      const string sql = @"select KeyValue from ValueStore where lower(KeyName) = lower(@keyName)";
      try {
        var data = ConnectionStringProvider.LocalDatabase.WrapInOpenConnection(connection => {
          return connection.QueryFirst<string?>(sql, new { keyName });
        });

        return Result.Success(data);
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While GetValue");
        return Result.Failure<string?>(ex.Message);
      }
    }

    public Result SetValue(string keyName, string? keyValue) {
      const string sql = @"update ValueStore set KeyValue=@keyValue where lower(KeyName) = lower(@keyName)";
      try {
        ConnectionStringProvider.LocalDatabase.WrapInTransaction((connection, transaction) => {
          connection.Execute(sql, new { keyName, keyValue }, transaction: transaction);
        });

        return Result.Success();
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While GetValue");
        return Result.Failure(ex.Message);
      }
    }


  }

}
