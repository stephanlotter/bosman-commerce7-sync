/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-06-05
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.OnlineSalesOrderServices;
using BosmanCommerce7.Module.BusinessObjects.SalesOrders;
using BosmanCommerce7.Module.Models;
using DevExpress.Xpo;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models {

  public interface IOnlineSalesOrder {

    bool IsRefund { get; }

    int? OrderNumber { get; }

    OnlineSalesOrderJsonProperties JsonProperties { get; }

    int? LinkedOrderNumber { get; }

    bool IsPosOrder { get; }

    string? EvolutionInvoiceNumber { get; }

    string? EvolutionSalesOrderNumber { get; }

    bool UseAccountCustomer { get; }

    DateTime OrderDate { get; }

    string? ProjectCode { get; }

    bool IsStoreOrder { get; }

    XPCollection<OnlineSalesOrderLine> SalesOrderLines { get; }

    string? EmailAddress { get; }

    string? ShipToAddressPostalCode { get; }

    bool IsClubOrder { get; }

    bool UseCashCustomer { get; }

    void PostLog(string shortDescription, string? details = null);

    void PostLog(string shortDescription, Exception ex);

    void SetPostingStatus(SalesOrderPostingStatus postingStatus);

    Address ShipToAddress();

    void UpdatePostingWorkflowState(SalesOrderPostingWorkflowState workflowState);

    void SetEvolutionInvoiceNumber(string value);

    void SetEvolutionSalesOrderNumber(string value);
  }
}