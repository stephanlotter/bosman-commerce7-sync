/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-01-25
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
  [RuleCombinationOfPropertiesIsUnique("SalesPersonMapping_Unique", DefaultContexts.Save, "EvolutionSalesRepCode,Commerce7SalesAssociateName", "Combination of Evolution Sales Rep Code and Commerce7 Sales Associate Name must be unique.")]
  public class SalesPersonMapping : XPObject {
    private string _commerce7SalesAssociateName;
    private string _evolutionSalesRepCode;

    [RuleRequiredField]
    public string EvolutionSalesRepCode {
      get => _evolutionSalesRepCode;
      set => SetPropertyValue(nameof(EvolutionSalesRepCode), ref _evolutionSalesRepCode, value);
    }

    [RuleRequiredField]
    public string Commerce7SalesAssociateName {
      get => _commerce7SalesAssociateName;
      set => SetPropertyValue(nameof(Commerce7SalesAssociateName), ref _commerce7SalesAssociateName, value);
    }

    public SalesPersonMapping(Session session) : base(session) {
    }
  }
}