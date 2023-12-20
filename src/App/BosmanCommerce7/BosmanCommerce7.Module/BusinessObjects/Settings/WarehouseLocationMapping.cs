/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-20
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
  [RuleCombinationOfPropertiesIsUnique("WarehouseLocationMapping_Unique", DefaultContexts.Save, "LocationTitle,WarehouseCode", "Combination of location and warehouse must be unique.")]
  public class WarehouseLocationMapping : XPObject {
    private string? _locationTitle;
    private string? _warehouseCode;

    [RuleRequiredField]
    public string? LocationTitle {
      get => _locationTitle;
      set => SetPropertyValue(nameof(LocationTitle), ref _locationTitle, value);
    }

    [RuleRequiredField]
    public string? WarehouseCode {
      get => _warehouseCode;
      set => SetPropertyValue(nameof(WarehouseCode), ref _warehouseCode, value);
    }

    public WarehouseLocationMapping(Session session) : base(session) {
    }
  }
}