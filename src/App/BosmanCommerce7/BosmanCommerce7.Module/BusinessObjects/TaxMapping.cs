/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.BusinessObjects {
  [DefaultClassOptions]
  [NavigationItem(true)]
  public class TaxMapping : XPObject {

    private string? _onlineTaxType;
    private string? _evolutionTaxType;

    public string? OnlineTaxType {
      get => _onlineTaxType;
      set => SetPropertyValue(nameof(OnlineTaxType), ref _onlineTaxType, value);
    }

    public string? EvolutionTaxType {
      get => _evolutionTaxType;
      set => SetPropertyValue(nameof(EvolutionTaxType), ref _evolutionTaxType, value);
    }

    public TaxMapping(Session session) : base(session) { }

  }
}
