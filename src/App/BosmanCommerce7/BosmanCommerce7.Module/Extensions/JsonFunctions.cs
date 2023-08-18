/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using Newtonsoft.Json;

namespace BosmanCommerce7.Module.Extensions {
  public static class JsonFunctions {

    public static string? Serialize(object? data) {
      if (data is string) { return $"{data ?? ""}"; }

      var settings = new JsonSerializerSettings {
        NullValueHandling = NullValueHandling.Ignore
      };
      return data != null ? JsonConvert.SerializeObject(data, settings) : null;
    }

    public static T? Deserialize<T>(string json) {
      return JsonConvert.DeserializeObject<T>(json);
    }

  }
}
