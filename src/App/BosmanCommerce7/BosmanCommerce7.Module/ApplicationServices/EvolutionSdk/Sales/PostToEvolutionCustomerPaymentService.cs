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
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales {

  public class PostToEvolutionCustomerPaymentService : PostToEvolutionJournalTransactionServiceBase, IPostToEvolutionCustomerPaymentService {

    public PostToEvolutionCustomerPaymentService(ILogger<PostToEvolutionCustomerPaymentService> logger, IValueStoreRepository valueStoreRepository) : base(logger, valueStoreRepository) {
    }

    public Result<(IOnlineSalesOrder onlineSalesOrder, IEvolutionCustomerDocument customerDocument)> Post(PostToEvolutionSalesOrderContext context, (IOnlineSalesOrder onlineSalesOrder, IEvolutionCustomerDocument customerDocument) orderDetails) {
      var (onlineSalesOrder, customerDocument) = orderDetails;

      var logTransactionType = onlineSalesOrder.IsRefund ? "customer refund" : "customer receipt";
      var logTransactionIdentifier = onlineSalesOrder.IsRefund
        ? $"[Evolution Reference:{customerDocument.Reference}][POS Reference:{onlineSalesOrder.OrderNumber}][POS Refunded Order Reference:{onlineSalesOrder.LinkedOrderNumber}]"
        : $"[Evolution Reference:{customerDocument.Reference}][POS Reference:{onlineSalesOrder.OrderNumber}]";

      _logger.LogInformation("START: Posting {logTransactionType} {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);

      Result<string> GetPaymentTransactionCode(string warehouseCode) {
        var codeKey = $"sales-orders-post-pos-payment-{(onlineSalesOrder!.IsRefund ? "refund" : "receipt")}-transaction-code-{warehouseCode}";
        return GetCodeFromValueStore(codeKey);
      }

      try {
        var warehouseCode = customerDocument.WarehouseCode;
        if (string.IsNullOrWhiteSpace(warehouseCode)) {
          _logger.LogError("Cannot post {logTransactionType}. No warehouse linked line found on the sales order. {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);
          return Result.Failure<(IOnlineSalesOrder onlineSalesOrder, IEvolutionCustomerDocument salesOrder)>("No warehouse linked line found on the sales order");
        }

        var transactionCodeResult = GetPaymentTransactionCode(warehouseCode);

        if (transactionCodeResult.IsFailure) {
          return Result.Failure<(IOnlineSalesOrder onlineSalesOrder, IEvolutionCustomerDocument salesOrder)>(transactionCodeResult.Error);
        }

        var transactionCode = $"{transactionCodeResult.Value}";
        var transactionAmountInVat = Math.Abs(onlineSalesOrder.JsonProperties.PaymentAmount()) - (onlineSalesOrder.IsRefund ? Math.Abs(onlineSalesOrder.JsonProperties.TipAmount()) : 0);
        var transactionDate = onlineSalesOrder.TransactionDate();

        var evolutionReference = onlineSalesOrder.IsRefund ? customerDocument.Reference : customerDocument.OrderNumber;
        var linkedOnlineOrderNumber = onlineSalesOrder.TransactionLinkedOrderNumnber();

        var receipt = new CustomerTransaction {
          Customer = new Customer(customerDocument.CustomerId),
          Amount = transactionAmountInVat,
          Date = transactionDate,
          Description = $"POS Order payment {onlineSalesOrder.OrderNumber} {evolutionReference} {warehouseCode}{linkedOnlineOrderNumber}",
          ExtOrderNo = $"{onlineSalesOrder.OrderNumber}",
          OrderNo = evolutionReference,
          Reference2 = $"{onlineSalesOrder.JsonProperties.SalesAssociateName()}",
          Reference = evolutionReference,
          TransactionCode = new TransactionCode(Pastel.Evolution.Module.AR, transactionCode),
        };

        if (orderDetails.customerDocument.ProjectId.HasValue) {
          receipt.Project = new Project(orderDetails.customerDocument.ProjectId!.Value);
        }

        receipt.Post();

        return Result.Success((onlineSalesOrder, customerDocument));
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While posting {logTransactionType} {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);
        return Result.Failure<(IOnlineSalesOrder onlineSalesOrder, IEvolutionCustomerDocument salesOrder)>(ex.Message);
      }
      finally {
        _logger.LogInformation("END: Posting {logTransactionType} {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);
      }
    }
  }
}