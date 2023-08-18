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

namespace BosmanCommerce7.Module.BusinessObjects {
  [DefaultClassOptions]
  [NavigationItem(true)]
  public class SalesOrderProcessingLog : XPObject {
    private SalesOrder? _SalesOrder;
    private DateTime _entryDate;
    private string? _shortDescription;
    private string? _details;

    [Association("SalesOrder-SalesOrderProcessingLog")]
    public SalesOrder? SalesOrder {
      get => _SalesOrder;
      set => SetPropertyValue(nameof(SalesOrder), ref _SalesOrder, value);
    }

    [ModelDefault("DisplayFormat", "{0:yyyy/MM/dd HH:mm:ss}")]
    [ModelDefault("EditMask", "yyyy/MM/dd HH:mm:ss")]
    [ModelDefault("AllowEdit", "false")]
    public DateTime EntryDate {
      get => _entryDate;
      set => SetPropertyValue(nameof(EntryDate), ref _entryDate, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public string? ShortDescription {
      get => _shortDescription;
      set => SetPropertyValue(nameof(ShortDescription), ref _shortDescription, value);
    }

    [Size(-1)]
    [ModelDefault("AllowEdit", "false")]
    public string? Details {
      get => _details;
      set => SetPropertyValue(nameof(Details), ref _details, value);
    }

    public SalesOrderProcessingLog(Session session) : base(session) { }

    public override void AfterConstruction() {
      base.AfterConstruction();
      EntryDate = DateTime.Now;
    }

  }

}
