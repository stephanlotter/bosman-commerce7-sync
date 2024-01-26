/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-01-25
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models;
using BosmanCommerce7.Module.BusinessObjects.SalesOrders;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales {

  public class PostToEvolutionCustomerPaymentService : IPostToEvolutionCustomerPaymentService {
    private readonly ILogger<PostToEvolutionCustomerPaymentService> _logger;
    private readonly IValueStoreRepository _valueStoreRepository;

    public PostToEvolutionCustomerPaymentService(ILogger<PostToEvolutionCustomerPaymentService> logger, IValueStoreRepository valueStoreRepository) {
      _logger = logger;
      _valueStoreRepository = valueStoreRepository;
    }

    public Result<(OnlineSalesOrder onlineSalesOrder, SalesDocumentBase customerDocument)> Post(PostToEvolutionSalesOrderContext context, (OnlineSalesOrder onlineSalesOrder, SalesDocumentBase customerDocument) orderDetails) {
      // TODO: Add logic to post payment amounts. For POS orders
      // TODO: Post customer journal using transaction type for POS payments

      // TODO:  Reverse transaction for refunds

      var (onlineSalesOrder, customerDocument) = orderDetails;

      var logMessage = onlineSalesOrder.IsRefund ? "customer refund" : "customer receipt";
      var receiptId = onlineSalesOrder.IsRefund
        ? $"[Evolution Reference:{customerDocument.OrderNo}][POS Reference:{onlineSalesOrder.OrderNumber}][POS Refunded Order Reference:{onlineSalesOrder.LinkedOrderNumber}]"
        : $"[Evolution Reference:{customerDocument.OrderNo}][POS Reference:{onlineSalesOrder.OrderNumber}]";

      _logger.LogInformation("START: Posting {logMessage} {receiptId}", logMessage, receiptId);

      OrderDetail? GetFirstLineWithWarehouse() {
        foreach (OrderDetail line in customerDocument!.Detail) {
          var warehouse = line.Warehouse;
          if (warehouse != null) { return line; }
        }

        return null;
      }

      try {
        var salesOrderLine = GetFirstLineWithWarehouse();

        if (salesOrderLine == null) {
          _logger.LogError("Cannot post customer receipt. No warehouse linked line found on the sales order. {logMessage} {receiptId}", logMessage, receiptId);
          return Result.Failure<(OnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder)>("Cannot post customer receipt.");
        }

        var warehouseCode = salesOrderLine.Warehouse.Code;
        var transactionCodeKey = $"sales-orders-post-pos-payment-{(onlineSalesOrder.IsRefund ? "refund" : "receipt")}-transaction-code-{warehouseCode}";
        var transactionCodeResult = _valueStoreRepository.GetValue(transactionCodeKey);

        if (transactionCodeResult.IsFailure) {
          return Result.Failure<(OnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder)>(transactionCodeResult.Error);
        }

        if (string.IsNullOrWhiteSpace(transactionCodeResult.Value)) {
          return Result.Failure<(OnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder)>($"Could not find a transaction code entry in ValueStore for {transactionCodeKey}");
        }

        var transactionAmountInVat = customerDocument.TotalIncl;
        var transactionDate = customerDocument.OrderDate;
        var transactionCode = $"{transactionCodeResult.Value}".Trim().ToUpper();

        var receipt = new CustomerTransaction {
          Customer = new Customer(customerDocument.Customer.ID),
          Amount = transactionAmountInVat,
          Date = transactionDate,
          Description = $"POS Order {onlineSalesOrder.OrderNumber} {customerDocument.OrderNo} {warehouseCode}",
          ExtOrderNo = $"{onlineSalesOrder.OrderNumber}",
          OrderNo = customerDocument.OrderNo,
          Reference2 = $"{onlineSalesOrder.LinkedOrderNumber}",
          Reference = customerDocument.OrderNo,
          TransactionCode = new TransactionCode(Pastel.Evolution.Module.AR, transactionCode)
        };

        receipt.Post();

        return Result.Success((onlineSalesOrder, customerDocument));
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While posting {logMessage} {receiptId}", logMessage, receiptId);
        return Result.Failure<(OnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder)>(ex.Message);
      }
      finally {
        _logger.LogInformation("END: Posting {logMessage} {receiptId}", logMessage, receiptId);
      }
    }
  }
}