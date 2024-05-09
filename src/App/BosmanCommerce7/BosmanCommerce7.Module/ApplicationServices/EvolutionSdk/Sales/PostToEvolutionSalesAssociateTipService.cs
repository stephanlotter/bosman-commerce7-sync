﻿/*
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

  public class PostToEvolutionSalesAssociateTipService : PostToEvolutionJournalTransactionServiceBase, IPostToEvolutionSalesAssociateTipService {

    public PostToEvolutionSalesAssociateTipService(ILogger<PostToEvolutionSalesAssociateTipService> logger, IValueStoreRepository valueStoreRepository) : base(logger, valueStoreRepository) {
    }

    public Result<(OnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder)> Post(PostToEvolutionSalesOrderContext context, (OnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder) orderDetails) {
      var (onlineSalesOrder, customerDocument) = orderDetails;

      var logTransactionType = onlineSalesOrder.IsRefund ? "tip refund" : "tip receipt";
      var logTransactionIdentifier = onlineSalesOrder.IsRefund
        ? $"[Evolution Reference:{customerDocument.Reference}][POS Reference:{onlineSalesOrder.OrderNumber}][POS Refunded Order Reference:{onlineSalesOrder.LinkedOrderNumber}]"
        : $"[Evolution Reference:{customerDocument.Reference}][POS Reference:{onlineSalesOrder.OrderNumber}]";

      _logger.LogInformation("START: Posting {logTransactionType} {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);

      try {
        var warehouseCodeResult = WarehouseCode(customerDocument);

        if (warehouseCodeResult.IsFailure) {
          _logger.LogError("Cannot post {logTransactionType}. No warehouse linked line found on the sales order. {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);
          return Result.Failure<(OnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder)>(warehouseCodeResult.Error);
        }

        var warehouseCode = warehouseCodeResult.Value;
        var transactionCodeResult = GetTipTransactionCode(warehouseCode);

        if (transactionCodeResult.IsFailure) {
          return Result.Failure<(OnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder)>(transactionCodeResult.Error);
        }

        var transactionCode = $"{transactionCodeResult.Value}";
        var tipAmount = onlineSalesOrder.JsonProperties.TipAmount();
        var transactionAmountInVat = Math.Abs(tipAmount);

        if (transactionAmountInVat == 0) {
          _logger.LogInformation("Tip amount zero. No {logTransactionType} transaction posted. {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);
          return Result.Success((onlineSalesOrder, customerDocument));
        }

        var transactionDate = customerDocument.OrderDate;
        var evolutionReference = onlineSalesOrder.IsRefund ? customerDocument.Reference : customerDocument.OrderNo;

        Result AddTransaction(GLBatch batch, Result<string> accountResult, double debitAmount, double creditAmount) {
          if (accountResult.IsFailure) { return accountResult; }

          _logger.LogInformation("Post TIP transaction for: {orderNumber} | Account: {account} | Debit: {debit} | Credit: {credit} ", onlineSalesOrder.OrderNumber, accountResult.Value, debitAmount, creditAmount);

          var transaction = new GLTransaction {
            Account = new GLAccount(accountResult.Value),
            Date = transactionDate,
            Description = $"POS Tip {onlineSalesOrder.OrderNumber} {evolutionReference} {warehouseCode}",
            ExtOrderNo = $"{onlineSalesOrder.OrderNumber}",
            OrderNo = evolutionReference,
            Reference2 = $"{onlineSalesOrder.JsonProperties.SalesAssociateName}",
            Reference = evolutionReference,
            TransactionCode = new TransactionCode(Pastel.Evolution.Module.GL, transactionCode)
          };

          if (Math.Abs(debitAmount) >= 0.01) {
            transaction.Debit = debitAmount;
          }
          if (Math.Abs(creditAmount) >= 0.01) {
            transaction.Credit = creditAmount;
          }

          batch.Add(transaction);

          return Result.Success();
        }

        Result PostBatch(GLBatch batch) {
          batch.Post();
          return Result.Success();
        }

        var batch = new GLBatch();

        var debitAccount = onlineSalesOrder.IsRefund ? GetCreditAccountCode(warehouseCode) : GetDebitAccountCode(warehouseCode);
        var creditAccount = onlineSalesOrder.IsRefund ? GetDebitAccountCode(warehouseCode) : GetCreditAccountCode(warehouseCode);

        return AddTransaction(batch, debitAccount, transactionAmountInVat, 0)
          .Bind(() => AddTransaction(batch, creditAccount, 0, transactionAmountInVat))
          .Bind(() => PostBatch(batch))
          .Map(() => (onlineSalesOrder, customerDocument));
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While posting {logTransactionType} {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);
        return Result.Failure<(OnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder)>(ex.Message);
      }
      finally {
        _logger.LogInformation("END: Posting {logTransactionType} {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);
      }

      Result<string> GetTipTransactionCode(string warehouseCode) {
        var codeKey = $"sales-orders-post-pos-tip-{(onlineSalesOrder!.IsRefund ? "refund" : "receipt")}-transaction-code-{warehouseCode}";
        return GetCodeFromValueStore(codeKey);
      }

      Result<string> GetDebitAccountCode(string warehouseCode) {
        var codeKey = $"sales-orders-post-pos-tip-debit-account-code-{warehouseCode}";
        return GetCodeFromValueStore(codeKey);
      }

      Result<string> GetCreditAccountCode(string warehouseCode) {
        var codeKey = $"sales-orders-post-pos-tip-credit-account-code-{warehouseCode}";
        return GetCodeFromValueStore(codeKey);
      }
    }
  }
}