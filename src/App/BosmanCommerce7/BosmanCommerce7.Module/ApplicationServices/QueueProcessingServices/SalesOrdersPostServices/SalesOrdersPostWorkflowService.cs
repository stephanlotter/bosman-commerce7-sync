/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-06-05
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models;
using BosmanCommerce7.Module.BusinessObjects.SalesOrders;
using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices {

  public class SalesOrdersPostWorkflowService : SyncServiceBase, ISalesOrdersPostWorkflowService {
    private readonly IPostToEvolutionSalesOrderService _postSalesOrderService;
    private readonly ISalesOrderCustomerPaymentPostService _postSalesOrderCustomerPaymentService;
    private readonly ISalesOrderTipPostService _postSalesOrderTipService;

    public SalesOrdersPostWorkflowService(ILogger<SalesOrdersPostService> logger,
      ILocalObjectSpaceEvolutionSdkProvider localObjectSpaceEvolutionSdkProvider,
      IPostToEvolutionSalesOrderService postToEvolutionSalesOrderService,
      ISalesOrderCustomerPaymentPostService postToEvolutionSalesOrderCustomerPaymentService,
      ISalesOrderTipPostService postToEvolutionSalesOrderTipService)
      : base(logger, localObjectSpaceEvolutionSdkProvider) {
      _postSalesOrderService = postToEvolutionSalesOrderService;
      _postSalesOrderCustomerPaymentService = postToEvolutionSalesOrderCustomerPaymentService;
      _postSalesOrderTipService = postToEvolutionSalesOrderTipService;
    }

    public Result<SalesOrdersPostResult> Execute(SalesOrdersPostContext context) {
      _errorCount = 0;

      /*
      This needs to be broken up into a workflow:
        - Post the sales order/invoice/credit note
        - Post the payment
        - Post the tip
      Each step in the workflow should be a separate process that can be retried.
      Each step must be in its own transaction.

      When the final workflow step is done set: 
      - onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Posted;
      - onlineSalesOrder.DatePosted = DateTime.Now;
       */

      var onlineSalesOrdersResult = LocalObjectSpaceEvolutionSdkProvider.WrapInObjectSpaceTransaction<List<OnlineSalesOrder>>(objectSpace => {
        return GetOnlineSalesOrders(objectSpace, context);
      });

      if (onlineSalesOrdersResult.IsFailure) {
        return Result.Failure<SalesOrdersPostResult>(onlineSalesOrdersResult.Error);
      }

      var onlineSalesOrders = onlineSalesOrdersResult.Value;
      if (!onlineSalesOrders.Any()) {
        Logger.LogDebug("No sales orders to post");
        return Result.Success(BuildResult());
      }

      var salesOrderPostingResult = LocalObjectSpaceEvolutionSdkProvider.WrapInObjectSpaceEvolutionSdkTransaction((objectSpace, _) => {
        var postToEvolutionSalesOrderContext = new PostToEvolutionSalesOrderContext {
          ObjectSpace = objectSpace
        };

        foreach (var onlineSalesOrder in onlineSalesOrders) {
          PostOnlineSalesOrder(LoadOnlineSalesOrder(objectSpace, onlineSalesOrder.Oid), postToEvolutionSalesOrderContext);
        }

        return _errorCount == 0 ? Result.Success(BuildResult()) : Result.Failure<SalesOrdersPostResult>($"Completed with {_errorCount} errors.");
      });

      return salesOrderPostingResult;

      SalesOrdersPostResult BuildResult() {
        return new SalesOrdersPostResult { };
      }
    }

    private void PostOnlineSalesOrder(OnlineSalesOrder onlineSalesOrder, PostToEvolutionSalesOrderContext postToEvolutionSalesOrderContext) {
      try {
        Logger.LogInformation("Start posting online sales order. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);

        if (onlineSalesOrder.IsRefund && onlineSalesOrder.IsClubOrder) {
          Logger.LogInformation("Skipping club order refund. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
          onlineSalesOrder.PostLog("Skipping club order refund.");
          onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Skipped;
          return;
        }

        if (!onlineSalesOrder.UseCashCustomer && string.IsNullOrWhiteSpace(onlineSalesOrder.EmailAddress)) {
          Logger.LogError("Email Address is empty. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
          onlineSalesOrder.PostLog("Email Address is empty.");
          onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Failed;
          return;
        }

        _postSalesOrderService.Post(postToEvolutionSalesOrderContext, onlineSalesOrder)
              .OnFailureCompensate(err => {
                RecordPostingError(onlineSalesOrder, err);
                return Result.Failure<OnlineSalesOrder>(err);
              });
      }
      catch (Exception ex) {
        onlineSalesOrder.PostLog(ex.Message, ex);
        RecordPostingError(onlineSalesOrder, ex.Message);
      }
      finally {
        onlineSalesOrder.Save();
        Logger.LogInformation("End posting online sales order. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
      }
    }

    private List<OnlineSalesOrder> GetOnlineSalesOrders(IObjectSpace objectSpace, SalesOrdersPostContext context) {
      CriteriaOperator? criteria = BuildCriteria(context);

      var sort = new List<SortProperty> {
        new SortProperty(nameof(OnlineSalesOrder.OrderNumber), DevExpress.Xpo.DB.SortingDirection.Ascending)
      };

      return objectSpace.GetObjects<OnlineSalesOrder>(criteria, (IList<SortProperty>)sort, false).ToList();
    }

    private OnlineSalesOrder LoadOnlineSalesOrder(IObjectSpace objectSpace, int oid) {
      return objectSpace.GetObjectByKey<OnlineSalesOrder>(oid);
    }

    private CriteriaOperator BuildCriteria(SalesOrdersPostContext context) {
      CriteriaOperator? criteria = context.Criteria;
      if (criteria is not null) { return criteria; }
      var criteriaPostingStatus = "PostingStatus".InCriteriaOperator(
        SalesOrderPostingStatus.New,
        SalesOrderPostingStatus.Posting,
        SalesOrderPostingStatus.Retrying
      );
      var criteriaRetryAfter = CriteriaOperator.Or("RetryAfter".IsNullOperator(), "RetryAfter".PropertyLessThan(DateTime.Now));
      return CriteriaOperator.And(criteriaPostingStatus, criteriaRetryAfter);
    }

    private void RecordPostingError(OnlineSalesOrder onlineSalesOrder, string error) {
      _errorCount++;
      Logger.LogError("Error posting sales order Online Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
      Logger.LogError("{error}", error);

      onlineSalesOrder.PostLog(error);

      if (onlineSalesOrder.RetryCount < MaxRetryCount) {
        onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Retrying;
        onlineSalesOrder.RetryCount++;
        onlineSalesOrder.RetryAfter = GetRetryAfter(onlineSalesOrder.RetryCount);
      }
      else {
        onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Failed;
      }
    }
  }
}