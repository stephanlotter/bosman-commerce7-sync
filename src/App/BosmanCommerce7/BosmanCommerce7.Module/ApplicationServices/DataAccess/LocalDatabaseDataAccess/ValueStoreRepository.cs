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

    public Result DeleteKey(string keyName) {
      const string sql = @"
if exists(select 1 from ValueStore where lower(KeyName) = lower(@keyName))
    delete from ValueStore where lower(KeyName) = lower(@keyName);
";
      try {
        ConnectionStringProvider.LocalDatabase.WrapInTransaction((connection, transaction) => {
          connection.Execute(sql, new { keyName }, transaction: transaction);
        });

        return Result.Success();
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While DeleteKey");
        return Result.Failure(ex.Message);
      }
    }

    public Result<DateTime?> GetDateTimeValue(string keyName, DateTime? defaultValue = null) {
      return GetValue(keyName, $"{defaultValue:yyyy-MM-dd HH:mm:ss}")
        .Bind(v => Result.Success<DateTime?>(DateTime.TryParse(v, out DateTime r) ? r : null));
    }

    public Result<int?> GetIntValue(string keyName, int? defaultValue = null) {
      return GetValue(keyName, $"{defaultValue}")
        .Bind(v => Result.Success<int?>(int.TryParse(v, out int r) ? r : null));
    }

    public Result<string?> GetValue(string keyName, string? defaultValue = null) {
      const string sql = @"
if exists(select 1 from ValueStore where lower(KeyName) = lower(@keyName))
  select KeyValue from ValueStore where lower(KeyName) = lower(@keyName)
else
  select @defaultValue;
";
      try {
        var data = ConnectionStringProvider.LocalDatabase.WrapInOpenConnection(connection => {
          return connection.QueryFirst<string?>(sql, new { keyName, defaultValue });
        });

        return Result.Success(data);
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While GetValue");
        return Result.Failure<string?>(ex.Message);
      }
    }

    public Result<bool> KeyExists(string keyName) {
      const string sql = @"
if exists(select 1 from ValueStore where lower(KeyName) = lower(@keyName))
    select convert(Bit, 1);
else
    select convert(Bit, 0);
";
      try {
        var data = ConnectionStringProvider.LocalDatabase.WrapInOpenConnection(connection => {
          return connection.QueryFirst<bool>(sql, new { keyName });
        });

        return Result.Success(data);
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While KeyExists");
        return Result.Failure<bool>(ex.Message);
      }
    }

    public Result SetDateTimeValue(string keyName, DateTime? keyValue) {
      return SetValue(keyName, $"{keyValue:yyyy-MM-dd HH:mm:ss.fffffff}");
    }

    public Result SetValue(string keyName, string? keyValue) {
      const string sql = @"
if exists(select 1 from ValueStore where lower(KeyName) = lower(@keyName))
    update ValueStore set KeyValue=@keyValue where lower(KeyName) = lower(@keyName);
else 
    insert into ValueStore (KeyName, KeyValue, OptimisticLockField) values (lower(@keyName), @keyValue, 0);
";
      try {
        ConnectionStringProvider.LocalDatabase.WrapInTransaction((connection, transaction) => {
          connection.Execute(sql, new { keyName, keyValue }, transaction: transaction);
        });

        return Result.Success();
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While SetValue");
        return Result.Failure(ex.Message);
      }
    }

  }

}
