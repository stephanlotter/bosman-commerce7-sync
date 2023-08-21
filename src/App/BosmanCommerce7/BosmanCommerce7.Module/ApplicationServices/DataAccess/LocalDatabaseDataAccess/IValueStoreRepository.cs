/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {
  public interface IValueStoreRepository {

    Result DeleteKey(string keyName);

    Result<string?> GetValue(string keyName, string? defaultValue = null);

    Result<DateTime?> GetDateTimeValue(string keyName, DateTime? defaultValue = null);

    Result<int?> GetIntValue(string keyName, int? defaultValue = null);

    Result<bool> KeyExists(string keyName);

    Result SetValue(string keyName, string? keyValue);

    Result SetDateTimeValue(string keyName, DateTime? keyValue);


  }
}