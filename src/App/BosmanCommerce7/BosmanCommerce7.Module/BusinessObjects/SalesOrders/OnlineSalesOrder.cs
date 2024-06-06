/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using System.ComponentModel;
using BosmanCommerce7.Module.ApplicationServices.OnlineSalesOrderServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models;
using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.BusinessObjects.SalesOrders {

  [DefaultClassOptions]
  [NavigationItem(true)]
  [DefaultProperty(nameof(OrderNumber))]
  public class OnlineSalesOrder : XPObject, IOnlineSalesOrder {
    private string? _customerId;
    private string? _emailAddress;
    private string? _onlineId;
    private string? _linkedOnlineId;
    private string? _channel;
    private string? _purchaseType;
    private DateTime _orderDate;
    private int? _orderNumber;
    private int? _linkedOrderNumber;
    private string? _evolutionSalesOrderNumber;
    private string? _evolutionInvoiceNumber;
    private double _orderValueInVat;
    private string? _projectCode;
    private string? _shipToName;
    private string? _shipToPhoneNumber;
    private string? _shipToAddress1;
    private string? _shipToAddress2;
    private string? _shipToAddressCity;
    private string? _shipToAddressProvince;
    private string? _shipToAddressPostalCode;
    private string? _shipToAddressCountryCode;
    private string? _lastErrorMessage;
    private string? _orderJson;
    private SalesOrderPostingStatus _postingStatus;
    private SalesOrderPostingWorkflowState _postingWorkflowState;
    private int _retryCount;
    private DateTime? _retryAfter;
    private DateTime? _datePosted;

    [Size(40)]
    public string? CustomerOnlineId {
      get => _customerId;
      set => SetPropertyValue(nameof(CustomerOnlineId), ref _customerId, value);
    }

    public string? EmailAddress {
      get => _emailAddress;
      set => SetPropertyValue(nameof(EmailAddress), ref _emailAddress, value);
    }

    [Size(40)]
    [ModelDefault("AllowEdit", "false")]
    [Indexed(Unique = true)]
    public string? OnlineId {
      get => _onlineId;
      set => SetPropertyValue(nameof(OnlineId), ref _onlineId, value);
    }

    [Size(40)]
    [ModelDefault("AllowEdit", "false")]
    public string? LinkedOnlineId {
      get => _linkedOnlineId;
      set => SetPropertyValue(nameof(LinkedOnlineId), ref _linkedOnlineId, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public string? Channel {
      get => _channel;
      set => SetPropertyValue(nameof(Channel), ref _channel, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public string? PurchaseType {
      get => _purchaseType;
      set => SetPropertyValue(nameof(PurchaseType), ref _purchaseType, value);
    }

    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public bool IsRefund => PurchaseType?.Equals("refund", StringComparison.InvariantCultureIgnoreCase) ?? false;

    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public bool IsClubOrder => Channel?.Equals("club", StringComparison.InvariantCultureIgnoreCase) ?? false;

    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public bool IsPosOrder => Channel?.Equals("pos", StringComparison.InvariantCultureIgnoreCase) ?? false;

    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public bool UseAccountCustomer => !UseCashCustomer;

    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public bool UseCashCustomer => string.IsNullOrWhiteSpace(CustomerOnlineId);

    [ModelDefault("AllowEdit", "false")]
    public string? EvolutionSalesOrderNumber {
      get => _evolutionSalesOrderNumber;
      set => SetPropertyValue(nameof(EvolutionSalesOrderNumber), ref _evolutionSalesOrderNumber, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public string? EvolutionInvoiceNumber {
      get => _evolutionInvoiceNumber;
      set => SetPropertyValue(nameof(EvolutionInvoiceNumber), ref _evolutionInvoiceNumber, value);
    }

    [ModelDefault("DisplayFormat", "{0:yyyy/MM/dd HH:mm:ss}")]
    [ModelDefault("EditMask", "yyyy/MM/dd HH:mm:ss")]
    [ModelDefault("AllowEdit", "false")]
    public DateTime OrderDate {
      get => _orderDate;
      set => SetPropertyValue(nameof(OrderDate), ref _orderDate, value);
    }

    [ModelDefault("AllowEdit", "false")]
    [Indexed(Unique = true)]
    public int? OrderNumber {
      get => _orderNumber;
      set => SetPropertyValue(nameof(OrderNumber), ref _orderNumber, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public int? LinkedOrderNumber {
      get => _linkedOrderNumber;
      set => SetPropertyValue(nameof(LinkedOrderNumber), ref _linkedOrderNumber, value);
    }

    public string? ShipToName {
      get => _shipToName;
      set => SetPropertyValue(nameof(ShipToName), ref _shipToName, value);
    }

    public string? ShipToPhoneNumber {
      get => _shipToPhoneNumber;
      set => SetPropertyValue(nameof(ShipToPhoneNumber), ref _shipToPhoneNumber, value);
    }

    [Size(200)]
    public string? ShipToAddress1 {
      get => _shipToAddress1;
      set => SetPropertyValue(nameof(ShipToAddress1), ref _shipToAddress1, value);
    }

    [Size(200)]
    public string? ShipToAddress2 {
      get => _shipToAddress2;
      set => SetPropertyValue(nameof(ShipToAddress2), ref _shipToAddress2, value);
    }

    public string? ShipToAddressCity {
      get => _shipToAddressCity;
      set => SetPropertyValue(nameof(ShipToAddressCity), ref _shipToAddressCity, value);
    }

    public string? ShipToAddressProvince {
      get => _shipToAddressProvince;
      set => SetPropertyValue(nameof(ShipToAddressProvince), ref _shipToAddressProvince, value);
    }

    public string? ShipToAddressPostalCode {
      get => _shipToAddressPostalCode;
      set => SetPropertyValue(nameof(ShipToAddressPostalCode), ref _shipToAddressPostalCode, value);
    }

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

    [ModelDefault("AllowEdit", "false")]
    public string? ProjectCode {
      get => _projectCode;
      set => SetPropertyValue(nameof(ProjectCode), ref _projectCode, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public SalesOrderPostingStatus PostingStatus {
      get => _postingStatus;
      set => SetPropertyValue(nameof(PostingStatus), ref _postingStatus, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public SalesOrderPostingWorkflowState PostingWorkflowState {
      get => _postingWorkflowState;
      set => SetPropertyValue(nameof(PostingWorkflowState), ref _postingWorkflowState, value);
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
    public DateTime? DatePosted {
      get => _datePosted;
      set => SetPropertyValue(nameof(DatePosted), ref _datePosted, value);
    }

    [ModelDefault("AllowEdit", "false")]
    public string? LastErrorMessage {
      get => _lastErrorMessage;
      set => SetPropertyValue(nameof(LastErrorMessage), ref _lastErrorMessage, value);
    }

    [Size(-1)]
    [ModelDefault("AllowEdit", "false")]
    public string? OrderJson {
      get => _orderJson;
      set => SetPropertyValue(nameof(OrderJson), ref _orderJson, value);
    }

    [Browsable(false)]
    public OnlineSalesOrderJsonProperties JsonProperties => new(OrderJson ?? "");

    [Browsable(false)]
    public bool IsStoreOrder => Channel?.Equals("web", StringComparison.InvariantCultureIgnoreCase) ?? false;

    [VisibleInListView(false)]
    public double PaymentAmount => JsonProperties.PaymentAmount();

    [VisibleInListView(false)]
    public double TipAmount => JsonProperties.TipAmount();

    [Association("OnlineSalesOrder-OnlineSalesOrderLine")]
    [Aggregated]
    [ModelDefault("AllowEdit", "false")]
    public XPCollection<OnlineSalesOrderLine> SalesOrderLines => GetCollection<OnlineSalesOrderLine>(nameof(SalesOrderLines));

    [Association("OnlineSalesOrder-OnlineSalesOrderProcessingLog")]
    [Aggregated]
    [ModelDefault("AllowEdit", "false")]
    public XPCollection<OnlineSalesOrderProcessingLog> SalesOrderProcessingLogs => GetCollection<OnlineSalesOrderProcessingLog>(nameof(SalesOrderProcessingLogs));

    public OnlineSalesOrder(Session session) : base(session) {
    }

    public override void AfterConstruction() {
      base.AfterConstruction();
      PostingStatus = SalesOrderPostingStatus.New;
      PostingWorkflowState = SalesOrderPostingWorkflowState.New;
      RetryAfter = DateTime.Now;
    }

    public DateTime TransactionDate() {
      return OrderDate;
    }

    public DateTime TransactionOrderDate() {
      return OrderDate;
    }

    public string TransactionLinkedOrderNumnber() {
      return IsRefund ? $" ON: {LinkedOrderNumber}" : "";
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

      var log = new OnlineSalesOrderProcessingLog(Session) {
        ShortDescription = shortDescription,
        Details = details
      };

      log.Save();
      SalesOrderProcessingLogs.Add(log);
    }

    public void ResetPostingStatus() {
      RetryAfter = DateTime.Now;
      RetryCount = 0;
      PostingStatus = SalesOrderPostingStatus.New;

      // IMPORTANT NOTE:
      // PostingWorkflowState must not be reset.
      // The retry is for the current workflow step only.
      // PostingWorkflowState = SalesOrderPostingWorkflowState.New;

      LastErrorMessage = null;
    }

    public Address ShipToAddress() {
      var list = new List<string>();

      string Normalize(string? value) {
        value = value?.Trim() ?? "";
        var x = Math.Min(value.Length, 40);
        return value[..x];
      }

      void Add(string? value) {
        if (string.IsNullOrWhiteSpace(value)) { return; }

        if (!value.Contains(',')) {
          list.Add(Normalize(value));
          return;
        }

        value.Split(',').ToList().ForEach(x => list.Add(Normalize(x)));
      }

      if (IsStoreOrder) { Add(ShipToName); }

      Add(ShipToAddress1);
      Add(ShipToAddress2);
      Add(ShipToAddressCity);

      if (IsStoreOrder) { Add(ShipToPhoneNumber); }

      Add(ShipToAddressProvince);
      Add(ShipToAddressCountryCode);

      list = list.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

      string GetLine(int i) => list!.Count > i ? list![i] : "";

      return new Address(
          GetLine(0),
          GetLine(1),
          GetLine(2),
          GetLine(3),
          GetLine(4),
          Normalize(ShipToAddressPostalCode));
    }

    public void UpdatePostingWorkflowState(SalesOrderPostingWorkflowState workflowState) {
      PostingWorkflowState = workflowState;
      LastErrorMessage = null;
      RetryCount = 0;
    }

    public void SetPostingStatus(SalesOrderPostingStatus postingStatus) {
      PostingStatus = postingStatus;

      if (postingStatus == SalesOrderPostingStatus.Retrying) {
        RetryCount++;
        RetryAfter = RetryPolicy.GetRetryAfter(RetryCount);
      }
    }

    public void SetAsPosted() {
      SetPostingStatus(SalesOrderPostingStatus.Posted);
      DatePosted = DateTime.Now;
      UpdatePostingWorkflowState(SalesOrderPostingWorkflowState.Completed);
      PostLog("Posting complete");
    }

    public void SetAsCancelled() {
      SetPostingStatus(SalesOrderPostingStatus.Cancelled);
      UpdatePostingWorkflowState(SalesOrderPostingWorkflowState.Completed);
      PostLog("Posting cancelled");
    }

    public void SetEvolutionInvoiceNumber(string value) {
      EvolutionInvoiceNumber = value;
    }

    public void SetEvolutionSalesOrderNumber(string value) {
      EvolutionSalesOrderNumber = value;
    }
  }
}