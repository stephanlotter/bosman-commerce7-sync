﻿/* 
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

namespace BosmanCommerce7.Module.BusinessObjects {

  [DefaultClassOptions]
  [NavigationItem(true)]
  public class SalesOrder : XPObject {

    private string? _customerId;
    private string? _onlineId;
    private DateTime _orderDate;
    private int? _orderNumber;
    private double _orderValueInVat;
    private string? _shipToAddress1;
    private string? _shipToAddress2;
    private string? _shipToAddressCity;
    private string? _shipToAddressProvince;
    private string? _shipToAddressPostalCode;
    private string? _shipToAddressCountryCode;
    private string? _lineNotes;
    private string? _orderJson;

    [Size(40)]
    [ModelDefault("AllowEdit", "false")]
    public string? CustomerId {
      get => _customerId;
      set => SetPropertyValue(nameof(CustomerId), ref _customerId, value);
    }

    [Size(40)]
    [ModelDefault("AllowEdit", "false")]
    public string? OnlineId {
      get => _onlineId;
      set => SetPropertyValue(nameof(OnlineId), ref _onlineId, value);
    }

    [ModelDefault("DisplayFormat", "{0:yyyy/MM/dd HH:mm:ss}")]
    [ModelDefault("EditMask", "yyyy/MM/dd HH:mm:ss")]
    [ModelDefault("AllowEdit", "false")]
    public DateTime OrderDate {
      get => _orderDate;
      set => SetPropertyValue(nameof(OrderDate), ref _orderDate, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public int? OrderNumber {
      get => _orderNumber;
      set => SetPropertyValue(nameof(OrderNumber), ref _orderNumber, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public string? ShipToAddress1 {
      get => _shipToAddress1;
      set => SetPropertyValue(nameof(ShipToAddress1), ref _shipToAddress1, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public string? ShipToAddress2 {
      get => _shipToAddress2;
      set => SetPropertyValue(nameof(ShipToAddress2), ref _shipToAddress2, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public string? ShipToAddressCity {
      get => _shipToAddressCity;
      set => SetPropertyValue(nameof(ShipToAddressCity), ref _shipToAddressCity, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public string? ShipToAddressProvince {
      get => _shipToAddressProvince;
      set => SetPropertyValue(nameof(ShipToAddressProvince), ref _shipToAddressProvince, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public string? ShipToAddressPostalCode {
      get => _shipToAddressPostalCode;
      set => SetPropertyValue(nameof(ShipToAddressPostalCode), ref _shipToAddressPostalCode, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public string? ShipToAddressCountryCode {
      get => _shipToAddressCountryCode;
      set => SetPropertyValue(nameof(ShipToAddressCountryCode), ref _shipToAddressCountryCode, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "false")]
    public double OrderValueInVat {
      get => _orderValueInVat;
      set => SetPropertyValue(nameof(OrderValueInVat), ref _orderValueInVat, value);
    }

    [Size(-1)]
    [ModelDefault("AllowEdit", "false")]
    public string? LineNotes {
      get => _lineNotes;
      set => SetPropertyValue(nameof(LineNotes), ref _lineNotes, value);
    }

    [Size(-1)]
    [ModelDefault("AllowEdit", "false")]
    public string? OrderJson {
      get => _orderJson;
      set => SetPropertyValue(nameof(OrderJson), ref _orderJson, value);
    }

    [Association("SalesOrder-SalesOrderLine")]
    [Aggregated]
    [ModelDefault("AllowEdit", "false")]
    public XPCollection<SalesOrderLine> SalesOrderLines => GetCollection<SalesOrderLine>(nameof(SalesOrderLines));

    public SalesOrder(Session session) : base(session) { }

  }

}
