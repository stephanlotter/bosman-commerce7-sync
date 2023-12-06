/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models;
using BosmanCommerce7.Module.BusinessObjects.SalesOrders;
using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices {

  public class SalesOrdersPostService : SyncServiceBase, ISalesOrdersPostService {
    private readonly IPostToEvolutionSalesOrderService _postToEvolutionSalesOrderService;

    public SalesOrdersPostService(ILogger<SalesOrdersPostService> logger,
      ILocalObjectSpaceProvider localObjectSpaceProvider,
      IPostToEvolutionSalesOrderService postToEvolutionSalesOrderService) : base(logger, localObjectSpaceProvider) {
      _postToEvolutionSalesOrderService = postToEvolutionSalesOrderService;
    }

    public Result<SalesOrdersPostResult> Execute(SalesOrdersPostContext context) {
      _errorCount = 0;

      LocalObjectSpaceProvider.WrapInObjectSpaceTransaction(objectSpace => {
        CriteriaOperator? criteria = context.Criteria;

        if (criteria is null) {
          var criteriaPostingStatus = "PostingStatus".InCriteriaOperator(SalesOrderPostingStatus.New, SalesOrderPostingStatus.Retrying);
          var criteriaRetryAfter = CriteriaOperator.Or("RetryAfter".IsNullOperator(), "RetryAfter".PropertyLessThan(DateTime.Now));
          criteria = CriteriaOperator.And(criteriaPostingStatus, criteriaRetryAfter);
        }

        var onlineSalesOrders = objectSpace.GetObjects<OnlineSalesOrder>(criteria).ToList();

        if (!onlineSalesOrders.Any()) {
          Logger.LogDebug("No sales orders to post");
          return;
        }

        var postToEvolutionSalesOrderContext = new PostToEvolutionSalesOrderContext {
          ObjectSpace = objectSpace
        };

        foreach (var onlineSalesOrder in onlineSalesOrders) {
          try {
            Logger.LogInformation("Start posting online sales order. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);

            if (onlineSalesOrder.IsRefund && onlineSalesOrder.IsClubOrder) {
              Logger.LogInformation("Skipping club order refund. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
              onlineSalesOrder.PostLog("Skipping club order refund.");
              onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Skipped;
              continue;
            }

            if (string.IsNullOrWhiteSpace(onlineSalesOrder.CustomerOnlineId)) {
              Logger.LogError("Customer Online Id is empty. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
              onlineSalesOrder.PostLog("Customer Online Id is empty.");
              onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Failed;
              continue;
            }

            if (string.IsNullOrWhiteSpace(onlineSalesOrder.EmailAddress)) {
              Logger.LogError("Email Address is empty. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
              onlineSalesOrder.PostLog("Email Address is empty.");
              onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Failed;
              continue;
            }

            _postToEvolutionSalesOrderService
              .Post(postToEvolutionSalesOrderContext, onlineSalesOrder)
              .OnFailureCompensate(err => {
                _errorCount++;
                Logger.LogError("Error posting sales order Online Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
                Logger.LogError("{error}", err);

                onlineSalesOrder.PostLog(err);

                if (onlineSalesOrder.RetryCount < MaxRetryCount) {
                  onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Retrying;
                  onlineSalesOrder.RetryCount++;
                  onlineSalesOrder.RetryAfter = GetRetryAfter(onlineSalesOrder.RetryCount);
                }
                else {
                  onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Failed;
                }

                return Result.Failure<OnlineSalesOrder>(err);
              });
          }
          catch (Exception ex) {
            Logger.LogError(ex, "Error posting online sales order number {OrderNumber}", onlineSalesOrder.OrderNumber);
            onlineSalesOrder.PostLog(ex.Message, ex);
          }
          finally {
            onlineSalesOrder.Save();
            Logger.LogInformation("End posting online sales order. Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
          }
        }
      });

      return _errorCount == 0 ? Result.Success(BuildResult()) : Result.Failure<SalesOrdersPostResult>($"Completed with {_errorCount} errors.");

      SalesOrdersPostResult BuildResult() {
        return new SalesOrdersPostResult { };
      }
    }
  }
}