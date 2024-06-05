/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-06-05
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models;
using CSharpFunctionalExtensions;
using Dapper;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales {

  public abstract class SalesOrderSubTransactionPostServiceBase {

    protected Result<IEvolutionCustomerDocument> LoadSalesOrder(IOnlineSalesOrder onlineSalesOrder) {
      const string sql = @"
select top 1 h.AutoIndex DocumentId,
			 c.DCLink CustomerId,
			 wm.Code WarehouseCode,
			 h.InvNumber Reference,
			 h.OrderNum OrderNumber,
			 h.ProjectID ProjectId,
			 h.OrderDate
	from _btblInvoiceLines il
	  join InvNum h
		  on h.AutoIndex = il.iInvoiceID
	  join Client c
		  on c.DCLink = h.AccountID
	  join WhseMst wm
		  on wm.WhseLink = il.iWarehouseID
	where h.InvNumber = @documentNumber
		and h.DocType = @documentType
		and h.DocState = @documentState
";

      var documentType = 4;
      var documentState = 4;

      if (onlineSalesOrder.IsRefund) {
        documentType = 1;
      }

      var data = DatabaseContext.DBConnection.Query<EvolutionCustomerDocument>(sql, new {
        DocumentNumber = onlineSalesOrder.EvolutionInvoiceNumber,
        DocumentType = documentType,
        DocumentState = documentState
      }, transaction: DatabaseContext.DBTransaction);

      if (!data.Any()) {
        return Result.Failure<IEvolutionCustomerDocument>("Sales order/Credit note not found");
      }

      return Result.Success((IEvolutionCustomerDocument)data.First());
    }
  }
}