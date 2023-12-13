/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-06
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices {

  public abstract class SyncMasterDataServiceBase : SyncServiceBase {
    private readonly IEvolutionSdk _evolutionSdk;

    public SyncMasterDataServiceBase(ILogger logger, ILocalObjectSpaceProvider localObjectSpaceProvider, IEvolutionSdk evolutionSdk) : base(logger, localObjectSpaceProvider) {
      _evolutionSdk = evolutionSdk;
    }

    protected Result ProcessQueueItems<T>(SyncContextBase context) where T : UpdateQueueBase {
      _errorCount = 0;
      CriteriaOperator? criteria = context.Criteria;

      try {
        return LocalObjectSpaceProvider.WrapInObjectSpaceTransaction(objectSpace => {
          return _evolutionSdk.WrapInSdkTransaction(connection => {
            CriteriaOperator? criteria = context.Criteria;
            var now = DateTime.Now;

            if (criteria is null) {
              var criteriaPostingStatus = "Status".InCriteriaOperator(QueueProcessingStatus.New, QueueProcessingStatus.Retrying);
              var criteriaRetryAfter = CriteriaOperator.Or("RetryAfter".IsNullOperator(), "RetryAfter".PropertyLessThan(now));
              criteria = CriteriaOperator.And(criteriaPostingStatus, criteriaRetryAfter);
            }

            var queueItems = objectSpace.GetObjects<T>(criteria).ToList<UpdateQueueBase>();

            if (!queueItems.Any()) {
              Logger.LogDebug("No records to sync.");
              return Result.Success() ;
            }

            Logger.LogDebug("{count} records to sync.", queueItems.Count);

            foreach (var queueItem in queueItems) {
              var result = ProcessQueueItem(queueItem);
              SetStatus(result, queueItem);
            }
            return _errorCount == 0 ? Result.Success() : Result.Failure($"Completed with {_errorCount} errors.");
          });
        });
      }
      catch (Exception ex) {
        Logger.LogError("Unable to process queue items: {error}", ex);
        return Result.Failure(ex.Message);
      }
    }

    protected abstract Result ProcessQueueItem(UpdateQueueBase updateQueueItem);

    protected void SetStatus(Result result, UpdateQueueBase queueItem) {
      if (result.IsFailure) {
        _errorCount++;
        queueItem.LastErrorMessage = result.Error;

        queueItem.RetryCount++;
        queueItem.Status = queueItem.RetryCount < MaxRetryCount ? QueueProcessingStatus.Retrying : QueueProcessingStatus.Failed;

        if (queueItem.Status != QueueProcessingStatus.Failed) {
          queueItem.RetryAfter = GetRetryAfter(queueItem.RetryCount);
        }
      }
      else {
        queueItem.ResetPostingStatus();
        queueItem.Status = QueueProcessingStatus.Processed;
      }
    }
  }
}