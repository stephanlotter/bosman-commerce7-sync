/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.AppDataServices {
  public interface IAppDataFileManager {
    Result<T?> LoadJson<T>(string subfolderName, string filename);

    Result<string> LoadText(string subfolderName, string filename);

    Result RemoveFile(string subfolderName, string filename);

    Result<string> StoreJson(string subfolderName, string filename, object? data);

    Result<string> StoreText(string subfolderName, string filename, string data);

  }
}