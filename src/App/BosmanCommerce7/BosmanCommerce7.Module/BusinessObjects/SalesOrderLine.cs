/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.BusinessObjects {
  [DefaultClassOptions]
  [NavigationItem(false)]
  public class SalesOrderLine : XPObject {

    private SalesOrder? _salesOrder;
    private string? _onlineId;
    private SalesOrderLineType _lineType;
    private string? _sku;
    private string? _lineDescription;
    private double _quantity;
    private double _taxAmount;
    private string? _onlineTaxType;
    private double _unitPriceInVat;
    private string? _lineNotes;

    [Association("SalesOrder-SalesOrderLine")]
    public SalesOrder? SalesOrder {
      get => _salesOrder;
      set => SetPropertyValue(nameof(SalesOrder), ref _salesOrder, value);
    }

    [Size(40)]
    [ModelDefault("AllowEdit", "false")]
    public string? OnlineId {
      get => _onlineId;
      set => SetPropertyValue(nameof(OnlineId), ref _onlineId, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public SalesOrderLineType LineType {
      get => _lineType;
      set => SetPropertyValue(nameof(LineType), ref _lineType, value);
    }

    [Size(20)]
    [ModelDefault("AllowEdit", "false")]
    public string? Sku {
      get => _sku;
      set => SetPropertyValue(nameof(Sku), ref _sku, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public string? LineDescription {
      get => _lineDescription;
      set => SetPropertyValue(nameof(LineDescription), ref _lineDescription, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "false")]
    public double Quantity {
      get => _quantity;
      set => SetPropertyValue(nameof(Quantity), ref _quantity, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "false")]
    public double TaxAmount {
      get => _taxAmount;
      set => SetPropertyValue(nameof(TaxAmount), ref _taxAmount, value);
    }

    [ModelDefault("AllowEdit", "false")]
    [Size(20)]
    public string? OnlineTaxType {
      get => _onlineTaxType;
      set => SetPropertyValue(nameof(OnlineTaxType), ref _onlineTaxType, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "false")]
    // price
    public double UnitPriceInVat {
      get => _unitPriceInVat;
      set => SetPropertyValue(nameof(UnitPriceInVat), ref _unitPriceInVat, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    public double LineValueInVat => Quantity * UnitPriceInVat;
    
    [Size(-1)]
    [ModelDefault("AllowEdit", "false")]
    public string? LineNotes {
      get => _lineNotes;
      set => SetPropertyValue(nameof(LineNotes), ref _lineNotes, value);
    }

    public SalesOrderLine(Session session) : base(session) { }

  }

}
