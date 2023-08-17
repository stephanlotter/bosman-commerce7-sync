/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public abstract class LocalMappingServiceBase {

    protected string JsonFileName(string path) { return $"{SafeFileName(path)}.json"; }

    protected string SafeFileName(string path) {
      if (string.IsNullOrWhiteSpace(path)) { return path; }
      var invalidChars = Path.GetInvalidFileNameChars();
      var output = new string(path.Where(c => !invalidChars.Contains(c)).ToArray());
      return output;
    }

  }

}
