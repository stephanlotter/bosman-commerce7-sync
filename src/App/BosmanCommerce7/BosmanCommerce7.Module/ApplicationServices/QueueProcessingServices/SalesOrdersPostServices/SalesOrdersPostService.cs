/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
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

  public class SalesOrdersPostService : SyncServiceBase, ISalesOrdersPostService {
    private readonly IPostToEvolutionSalesOrderService _postToEvolutionSalesOrderService;

    public SalesOrdersPostService(ILogger<SalesOrdersPostService> logger,
      ILocalObjectSpaceEvolutionSdkProvider localObjectSpaceEvolutionSdkProvider,
      IPostToEvolutionSalesOrderService postToEvolutionSalesOrderService) : base(logger, localObjectSpaceEvolutionSdkProvider) {
      _postToEvolutionSalesOrderService = postToEvolutionSalesOrderService;
    }

    public Result<SalesOrdersPostResult> Execute(SalesOrdersPostContext context) {
      _errorCount = 0;

      return LocalObjectSpaceEvolutionSdkProvider.WrapInObjectSpaceEvolutionSdkTransaction((objectSpace, _) => {
        var onlineSalesOrders = GetOnlineSalesOrders(objectSpace, context);

        if (!onlineSalesOrders.Any()) {
          Logger.LogDebug("No sales orders to post");
          return Result.Success(BuildResult());
        }

        var postToEvolutionSalesOrderContext = new PostToEvolutionSalesOrderContext {
          ObjectSpace = objectSpace
        };

        foreach (var onlineSalesOrder in onlineSalesOrders) {
          PostOnlineSalesOrder(onlineSalesOrder, postToEvolutionSalesOrderContext);
        }

        return _errorCount == 0 ? Result.Success(BuildResult()) : Result.Failure<SalesOrdersPostResult>($"Completed with {_errorCount} errors.");
      });

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

        _postToEvolutionSalesOrderService
              .Post(postToEvolutionSalesOrderContext, onlineSalesOrder)
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

    private CriteriaOperator BuildCriteria(SalesOrdersPostContext context) {
      CriteriaOperator? criteria = context.Criteria;
      if (criteria is not null) { return criteria; }
      var criteriaPostingStatus = "PostingStatus".InCriteriaOperator(SalesOrderPostingStatus.New, SalesOrderPostingStatus.Retrying);
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