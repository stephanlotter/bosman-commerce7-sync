/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using DevExpress.ExpressApp.Model;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.BusinessObjects {

  [NonPersistent]
  public abstract class UpdateQueueBase : XPObject {
    private int _retryCount;
    private DateTime? _retryAfter;
    private string? _lastErrorMessage;
    private DateTime _dateTimeAdded;
    private DateTime _dateTimeProcessed;
    private QueueProcessingStatus _status;

    public QueueProcessingStatus Status {
      get => _status;
      set => SetPropertyValue(nameof(Status), ref _status, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public int RetryCount {
      get => _retryCount;
      set => SetPropertyValue(nameof(RetryCount), ref _retryCount, value);
    }

    [ModelDefault("DisplayFormat", "{0:yyyy/MM/dd HH:mm:ss}")]
    [ModelDefault("EditMask", "yyyy/MM/dd HH:mm:ss")]
    [ModelDefault("AllowEdit", "false")]
    public DateTime? RetryAfter {
      get => _retryAfter;
      set => SetPropertyValue(nameof(RetryAfter), ref _retryAfter, value);
    }

    [ModelDefault("DisplayFormat", "{0:yyyy/MM/dd HH:mm:ss}")]
    [ModelDefault("EditMask", "yyyy/MM/dd HH:mm:ss")]
    [ModelDefault("AllowEdit", "false")]
    public DateTime DateTimeAdded {
      get => _dateTimeAdded;
      set => SetPropertyValue(nameof(DateTimeAdded), ref _dateTimeAdded, value);
    }

    [ModelDefault("DisplayFormat", "{0:yyyy/MM/dd HH:mm:ss}")]
    [ModelDefault("EditMask", "yyyy/MM/dd HH:mm:ss")]
    [ModelDefault("AllowEdit", "false")]
    public DateTime DateTimeProcessed {
      get => _dateTimeProcessed;
      set => SetPropertyValue(nameof(DateTimeProcessed), ref _dateTimeProcessed, value);
    }

    [ModelDefault("AllowEdit", "false")]
    [Size(-1)]
    public string? LastErrorMessage {
      get => _lastErrorMessage;
      set => SetPropertyValue(nameof(LastErrorMessage), ref _lastErrorMessage, value);
    }

    public UpdateQueueBase(Session session) : base(session) {
    }

    public override void AfterConstruction() {
      base.AfterConstruction();
      Status = QueueProcessingStatus.New;
      DateTimeAdded = DateTime.Now;
      RetryAfter = DateTime.Now;
    }

    public void PostLog(string shortDescription, Exception ex) {
      PostLog(shortDescription, ExceptionFunctions.GetMessages(ex));
    }

    public void PostLog(string shortDescription, string? details = null) {
      if (shortDescription.Length > 100) {
        details = shortDescription + "\r\n" + (details ?? "");
        shortDescription = $"{shortDescription[..97]}...";
      }

      LastErrorMessage = shortDescription;

      var log = CreateLogEntry();

      log.ShortDescription = shortDescription;
      log.Details = details;

      log.Save();
    }

    protected abstract UpdateQueueLogBase CreateLogEntry();

    public void ResetPostingStatus() {
      RetryAfter = DateTime.Now;
      RetryCount = 0;
      Status = QueueProcessingStatus.New;
      LastErrorMessage = null;
    }
  }
}