/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
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
  public class TaxMapping : XPObject {

    private string? _onlineTaxType;
    private string? _evolutionTaxType;

    [RuleRequiredField]
    [RuleUniqueValue]
    public string? OnlineTaxType {
      get => _onlineTaxType;
      set => SetPropertyValue(nameof(OnlineTaxType), ref _onlineTaxType, value);
    }

    [RuleRequiredField]
    public string? EvolutionTaxType {
      get => _evolutionTaxType;
      set => SetPropertyValue(nameof(EvolutionTaxType), ref _evolutionTaxType, value);
    }

    public TaxMapping(Session session) : base(session) { }

  }
}
