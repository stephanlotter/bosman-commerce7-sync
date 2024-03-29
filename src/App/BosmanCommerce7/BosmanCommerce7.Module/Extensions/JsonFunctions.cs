﻿/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BosmanCommerce7.Module.Extensions {

  public static class JsonFunctions {

    public static string? Serialize(object? data) {
      if (data is null) { return null; }
      if (data is string) { return $"{data ?? ""}"; }

      var settings = new JsonSerializerSettings {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
      };
      return data != null ? JsonConvert.SerializeObject(data, settings) : null;
    }

    public static T? Deserialize<T>(string json) {
      return JsonConvert.DeserializeObject<T>(json);
    }
  }
}