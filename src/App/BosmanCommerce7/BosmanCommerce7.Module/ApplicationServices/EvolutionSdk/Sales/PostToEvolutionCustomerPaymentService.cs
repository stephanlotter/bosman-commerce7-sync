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

      var logTransactionType = onlineSalesOrder.IsRefund ? "customer refund" : "customer receipt";
      var logTransactionIdentifier = onlineSalesOrder.IsRefund
        ? $"[Evolution Reference:{customerDocument.OrderNo}][POS Reference:{onlineSalesOrder.OrderNumber}][POS Refunded Order Reference:{onlineSalesOrder.LinkedOrderNumber}]"
        : $"[Evolution Reference:{customerDocument.OrderNo}][POS Reference:{onlineSalesOrder.OrderNumber}]";

      _logger.LogInformation("START: Posting {logTransactionType} {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);

      OrderDetail? GetFirstLineWithWarehouse() {
        foreach (OrderDetail line in customerDocument!.Detail) {
          var warehouse = line.Warehouse;
          if (warehouse != null) { return line; }
        }

        return null;
      }

      Result<string> WarehouseCode() {
        var salesOrderLine = GetFirstLineWithWarehouse();

        if (salesOrderLine == null) {
          _logger.LogError("Cannot post customer receipt. No warehouse linked line found on the sales order. {logTransactionType} {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);
          return Result.Failure<string>("Cannot post customer receipt.");
        }

        return salesOrderLine.Warehouse.Code;
      }

      Result<string> GetTransactionCode(string warehouseCode) {
        var transactionCodeKey = $"sales-orders-post-pos-payment-{(onlineSalesOrder!.IsRefund ? "refund" : "receipt")}-transaction-code-{warehouseCode}";
        var transactionCodeResult = _valueStoreRepository.GetValue(transactionCodeKey);

        if (transactionCodeResult.IsFailure) {
          return Result.Failure<string>(transactionCodeResult.Error);
        }

        if (string.IsNullOrWhiteSpace(transactionCodeResult.Value)) {
          return Result.Failure<string>($"Could not find a transaction code entry in ValueStore for {transactionCodeKey}");
        }

        return transactionCodeResult.Value.Trim().ToUpper();
      }

      try {
        var warehouseCodeResult = WarehouseCode();

        if (warehouseCodeResult.IsFailure) {
          return Result.Failure<(OnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder)>(warehouseCodeResult.Error);
        }

        var warehouseCode = warehouseCodeResult.Value;
        var transactionCodeResult = GetTransactionCode(warehouseCode);

        if (transactionCodeResult.IsFailure) {
          return Result.Failure<(OnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder)>(transactionCodeResult.Error);
        }

        var transactionCode = $"{transactionCodeResult.Value}";
        var transactionAmountInVat = customerDocument.TotalIncl;
        var transactionDate = customerDocument.OrderDate;

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
        _logger.LogError(ex, "While posting {logTransactionType} {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);
        return Result.Failure<(OnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder)>(ex.Message);
      }
      finally {
        _logger.LogInformation("END: Posting {logTransactionType} {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);
      }
    }
  }
}