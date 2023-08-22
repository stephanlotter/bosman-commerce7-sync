/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-22
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.BusinessObjects {
  [DefaultClassOptions]
  [NavigationItem(true)]
  public class WarehousePostalCodeMapping : XPObject {
    private string? _postalCode;
    private string? _warehouseCode;

    [Size(4)]
    [Indexed(Unique = true)]
    public string? PostalCode {
      get => _postalCode;
      set => SetPropertyValue(nameof(PostalCode), ref _postalCode, value);
    }

    [Size(10)]
    public string? WarehouseCode {
      get => _warehouseCode;
      set => SetPropertyValue(nameof(WarehouseCode), ref _warehouseCode, value);
    }

    public WarehousePostalCodeMapping(Session session) : base(session) { }

  }
}
