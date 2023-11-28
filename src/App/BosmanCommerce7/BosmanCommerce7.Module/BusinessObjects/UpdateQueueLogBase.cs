/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using DevExpress.ExpressApp.Model;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.BusinessObjects {

  [NonPersistent]
  public abstract class UpdateQueueLogBase : XPObject {
    private DateTime _entryDate;
    private string? _shortDescription;
    private string? _details;

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

    public UpdateQueueLogBase(Session session) : base(session) {
    }

    public override void AfterConstruction() {
      base.AfterConstruction();
      EntryDate = DateTime.Now;
    }
  }
}