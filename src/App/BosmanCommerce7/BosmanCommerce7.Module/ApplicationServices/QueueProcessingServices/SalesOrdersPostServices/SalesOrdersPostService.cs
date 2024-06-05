/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models;
using BosmanCommerce7.Module.BusinessObjects.SalesOrders;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices {

  public class SalesOrdersPostService : SalesOrderSubTransactionPostServiceBase, ISalesOrdersPostService {
    private readonly ILogger<SalesOrdersPostService> _logger;
    private readonly IPostToEvolutionSalesOrderService _postSalesOrderService;

    public SalesOrdersPostService(ILogger<SalesOrdersPostService> logger,
      IPostToEvolutionSalesOrderService postToEvolutionSalesOrderService) {
      _logger = logger;
      _postSalesOrderService = postToEvolutionSalesOrderService;
    }

    public Result<OnlineSalesOrder> Post(PostToEvolutionSalesOrderContext context, OnlineSalesOrder onlineSalesOrder) {
      try {
        if (onlineSalesOrder.IsRefund && onlineSalesOrder.IsClubOrder) {
          _logger.LogInformation("Skipping - club order refunds are not posted. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
          onlineSalesOrder.PostLog("Skipping - club order refunds are not posted.");
          onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Skipped;
          return Result.Success(onlineSalesOrder);
        }

        if (!onlineSalesOrder.UseCashCustomer && string.IsNullOrWhiteSpace(onlineSalesOrder.EmailAddress)) {
          _logger.LogError("Email Address is empty. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
          onlineSalesOrder.PostLog("Email Address is empty. Posting aborted.");
          onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Failed;
          return Result.Success(onlineSalesOrder);
        }

        return _postSalesOrderService.Post(context, onlineSalesOrder)
              .Bind(onlineSalesOrder => {
                onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Posting;
                onlineSalesOrder.UpdatePostingWorkflowState(SalesOrderPostingWorkflowState.OrderPosted);
                onlineSalesOrder.PostLog("Customer payment posted to Evolution");
                return Result.Success(onlineSalesOrder);
              });
      }
      catch (Exception ex) {
        _logger.LogError(ex, "Error posting SALES ORDER Online Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
        onlineSalesOrder.PostLog(ex.Message, ex);
        return Result.Failure<OnlineSalesOrder>(ex.Message);
      }
    }
  }
}