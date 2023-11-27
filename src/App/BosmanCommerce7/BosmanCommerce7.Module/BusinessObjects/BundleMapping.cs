﻿/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-27
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.BusinessObjects {

  [DefaultClassOptions]
  [NavigationItem("System")]
  public class BundleMapping : XPObject {
    private string _bundleSku = default!;
    private string _evolutionCode = default!;

    [RuleRequiredField]
    public string BundleSku {
      get => _bundleSku;
      set => SetPropertyValue(nameof(BundleSku), ref _bundleSku, value);
    }

    [RuleRequiredField]
    public string EvolutionCode {
      get => _evolutionCode;
      set => SetPropertyValue(nameof(EvolutionCode), ref _evolutionCode, value);
    }

    public BundleMapping(Session session) : base(session) {
    }
  }
}