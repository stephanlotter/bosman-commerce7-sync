/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-06-05
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models;
using BosmanCommerce7.Module.BusinessObjects.SalesOrders;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales {

  public class SalesOrderTipPostService : SalesOrderSubTransactionPostServiceBase, ISalesOrderTipPostService {
    private readonly ILogger<PostToEvolutionSalesOrderService> _logger;
    private readonly IPostToEvolutionSalesAssociateTipService _postToEvolutionSalesAssociateTipService;

    public SalesOrderTipPostService(ILogger<PostToEvolutionSalesOrderService> logger,
      IPostToEvolutionSalesAssociateTipService postToEvolutionSalesAssociateTipService) {
      _logger = logger;
      _postToEvolutionSalesAssociateTipService = postToEvolutionSalesAssociateTipService;
    }

    public Result<OnlineSalesOrder> Post(PostToEvolutionSalesOrderContext context, OnlineSalesOrder onlineSalesOrder) {
      try {
        if (!onlineSalesOrder.IsPosOrder) {
          onlineSalesOrder.PostingWorkflowState = SalesOrderPostingWorkflowState.PaymentPosted;
          return Result.Success(onlineSalesOrder);
        }

        return LoadSalesOrder(onlineSalesOrder)
          .Bind(salesOrder => Result.Success((onlineSalesOrder, salesOrder)))

          .Bind(orderDetails => {
            var (onlineSalesOrder, salesOrder) = orderDetails;

            return _postToEvolutionSalesAssociateTipService.Post(context, orderDetails)
              .Bind(a => {
                onlineSalesOrder.UpdatePostingWorkflowState(SalesOrderPostingWorkflowState.TipPosted);
                onlineSalesOrder.PostLog("Tip posted to Evolution");
                return Result.Success(a.onlineSalesOrder);
              });
          });
      }
      catch (Exception ex) {
        _logger.LogError(ex, "Error posting TIP Online Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
        onlineSalesOrder.PostLog(ex.Message, ex);
        return Result.Failure<OnlineSalesOrder>(ex.Message);
      }
    }
  }
}