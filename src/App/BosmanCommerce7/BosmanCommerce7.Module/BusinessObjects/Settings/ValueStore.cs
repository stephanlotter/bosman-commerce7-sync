/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.BusinessObjects.Settings {

  [DefaultClassOptions]
  [NavigationItem("System")]
  public class ValueStore : XPObject {
    private string? _keyName;
    private string? _keyValue;

    [ModelDefault("AllowEdit", "false")]
    public string? KeyName {
      get => _keyName;
      set => SetPropertyValue(nameof(KeyName), ref _keyName, value);
    }

    [Size(-1)]
    public string? KeyValue {
      get => _keyValue;
      set => SetPropertyValue(nameof(KeyValue), ref _keyValue, value);
    }

    public ValueStore(Session session) : base(session) {
    }
  }
}