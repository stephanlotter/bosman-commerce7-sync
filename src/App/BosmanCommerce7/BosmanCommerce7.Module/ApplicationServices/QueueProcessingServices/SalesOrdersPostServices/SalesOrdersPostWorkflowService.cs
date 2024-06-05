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
    private readonly ISalesOrdersPostService _salesOrdersPostService;
    private readonly ISalesOrderCustomerPaymentPostService _salesOrderCustomerPaymentPostService;
    private readonly ISalesOrderTipPostService _salesOrderTipPostService;

    public SalesOrdersPostWorkflowService(ILogger<SalesOrdersPostService> logger,
      ILocalObjectSpaceEvolutionSdkProvider localObjectSpaceEvolutionSdkProvider,
      ISalesOrdersPostService salesOrdersPostService,
      ISalesOrderCustomerPaymentPostService salesOrderCustomerPaymentPostService,
      ISalesOrderTipPostService salesOrderTipPostService)
      : base(logger, localObjectSpaceEvolutionSdkProvider) {
      _salesOrdersPostService = salesOrdersPostService;
      _salesOrderCustomerPaymentPostService = salesOrderCustomerPaymentPostService;
      _salesOrderTipPostService = salesOrderTipPostService;
    }

    public Result Execute(SalesOrdersPostContext context) {
      _errorCount = 0;

      var onlineSalesOrdersResult = LocalObjectSpaceEvolutionSdkProvider.WrapInObjectSpaceTransaction<List<OnlineSalesOrder>>(objectSpace => {
        return GetOnlineSalesOrders(objectSpace, context);
      });

      if (onlineSalesOrdersResult.IsFailure) {
        return Result.Failure(onlineSalesOrdersResult.Error);
      }

      var onlineSalesOrders = onlineSalesOrdersResult.Value;
      if (!onlineSalesOrders.Any()) {
        Logger.LogDebug("No sales orders to post");
        return Result.Success();
      }

      var results = new List<Result>();

      foreach (var onlineSalesOrder in onlineSalesOrders) {
        var processingResult = LocalObjectSpaceEvolutionSdkProvider.WrapInObjectSpaceEvolutionSdkTransaction((objectSpace, _) => {
          var postToEvolutionSalesOrderContext = new PostToEvolutionSalesOrderContext {
            ObjectSpace = objectSpace
          };

          return ProcessOnlineSalesOrder(LoadOnlineSalesOrder(objectSpace, onlineSalesOrder.Oid), postToEvolutionSalesOrderContext);
        });
        results.Add(processingResult);
      }

      return _errorCount == 0 ? Result.Success() : Result.Failure($"Completed with {_errorCount} errors.");
    }

    private Result ProcessOnlineSalesOrder(OnlineSalesOrder onlineSalesOrder, PostToEvolutionSalesOrderContext postToEvolutionSalesOrderContext) {
      try {
        Logger.LogInformation("Start online sales order workflow. Order Number {OrderNumber}: Current state: {onlineSalesOrder.PostingWorkflowState}", onlineSalesOrder.OrderNumber, onlineSalesOrder.PostingWorkflowState);

        switch (onlineSalesOrder.PostingWorkflowState) {
          case SalesOrderPostingWorkflowState.New:
            return _salesOrdersPostService.Post(postToEvolutionSalesOrderContext, onlineSalesOrder)
                  .OnFailureCompensate(err => {
                    RecordPostingError(onlineSalesOrder, err);
                    return Result.Failure<IOnlineSalesOrder>(err);
                  });

          case SalesOrderPostingWorkflowState.OrderPosted:
            return _salesOrderCustomerPaymentPostService.Post(postToEvolutionSalesOrderContext, onlineSalesOrder)
                  .OnFailureCompensate(err => {
                    RecordPostingError(onlineSalesOrder, err);
                    return Result.Failure<IOnlineSalesOrder>(err);
                  });

          case SalesOrderPostingWorkflowState.PaymentPosted:
            return _salesOrderTipPostService.Post(postToEvolutionSalesOrderContext, onlineSalesOrder)
                  .OnFailureCompensate(err => {
                    RecordPostingError(onlineSalesOrder, err);
                    return Result.Failure<IOnlineSalesOrder>(err);
                  });

          case SalesOrderPostingWorkflowState.TipPosted:
            onlineSalesOrder.SetAsPosted();
            return Result.Success();

          default:
            throw new ArgumentOutOfRangeException();
        }
      }
      catch (Exception ex) {
        onlineSalesOrder.PostLog(ex.Message, ex);
        RecordPostingError(onlineSalesOrder, ex.Message);
        return Result.Failure(ex.Message);
      }
      finally {
        onlineSalesOrder.Save();
        Logger.LogInformation("End online sales order workflow. Order Number {OrderNumber}: New state: {onlineSalesOrder.PostingWorkflowState}", onlineSalesOrder.OrderNumber, onlineSalesOrder.PostingWorkflowState);
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