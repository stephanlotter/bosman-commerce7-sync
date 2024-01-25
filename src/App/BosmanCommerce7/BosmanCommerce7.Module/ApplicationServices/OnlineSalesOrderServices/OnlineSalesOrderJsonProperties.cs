/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-01-25
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using Newtonsoft.Json;

namespace BosmanCommerce7.Module.ApplicationServices.OnlineSalesOrderServices {

  public class OnlineSalesOrderJsonProperties {
    private dynamic? _data;

    public OnlineSalesOrderJsonProperties(string json) {
      if (string.IsNullOrEmpty(json)) { throw new ArgumentNullException(nameof(json)); }
      _data = JsonConvert.DeserializeObject<dynamic>(json);
    }

    public string? SalesAssociateName() => _data?.salesAssociate?.Name;
  }
}