/*
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

namespace BosmanCommerce7.Module.BusinessObjects.Settings {

  [DefaultClassOptions]
  [NavigationItem("System")]
  public class BundleMapping : XPObject {
    private string _bundleSku = default!;
    private string _evolutionCode = default!;
    private string? _externalReferenceCode;

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

    public string? ExternalReferenceCode {
      get => _externalReferenceCode;
      set => SetPropertyValue(nameof(ExternalReferenceCode), ref _externalReferenceCode, value);
    }

    public BundleMapping(Session session) : base(session) {
    }
  }
}