/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-06
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using System.Text;
using System.Text.RegularExpressions;
using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices {

  public abstract class SyncMasterDataServiceBase : SyncServiceBase {

    //private Regex regex = new Regex(@"(?<=Response body:\W){.*}");
    private Regex regexJson = new Regex(@"(?<=Response body:\W){.*}", RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private Regex regexErrorMessage = new Regex(@"(.*?)(?=Response body:)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

    protected IObjectSpace? ObjectSpace { get; private set; }

    public SyncMasterDataServiceBase(ILogger logger, ILocalObjectSpaceEvolutionSdkProvider localObjectSpaceEvolutionSdkProvider) : base(logger, localObjectSpaceEvolutionSdkProvider) {
    }

    protected Result ProcessQueueItems<T>(SyncContextBase context) where T : UpdateQueueBase {
      _errorCount = 0;
      CriteriaOperator? criteria = context.Criteria;

      try {
        return LocalObjectSpaceEvolutionSdkProvider.WrapInObjectSpaceEvolutionSdkTransaction((objectSpace, connection) => {
          CriteriaOperator? criteria = context.Criteria;
          ObjectSpace = objectSpace;
          var now = DateTime.Now;

          if (criteria is null) {
            var criteriaPostingStatus = "Status".InCriteriaOperator(QueueProcessingStatus.New, QueueProcessingStatus.Retrying);
            var criteriaRetryAfter = CriteriaOperator.Or("RetryAfter".IsNullOperator(), "RetryAfter".PropertyLessThan(now));
            criteria = CriteriaOperator.And(criteriaPostingStatus, criteriaRetryAfter);
          }

          var queueItems = objectSpace.GetObjects<T>(criteria).ToList<UpdateQueueBase>();

          if (!queueItems.Any()) {
            Logger.LogDebug("No records to sync.");
            return Result.Success();
          }

          Logger.LogDebug("{count} records to sync.", queueItems.Count);

          foreach (var queueItem in queueItems) {
            var result = ProcessQueueItem(queueItem);
            SetStatus(result, queueItem);
          }

          return _errorCount == 0 ? Result.Success() : Result.Failure($"Completed with {_errorCount} errors.");
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

        var isUnprocessableEntity = result.Error.Contains("UnprocessableEntity");

        if (regexJson.IsMatch(result.Error)) {
          var errorMessage = regexErrorMessage.Match(result.Error);
          var json = regexJson.Match(result.Error);

          try {
            var errorResults = JsonConvert.DeserializeObject<Commerce7ErrorResponse>(json.Value);
            if (errorResults != null) {
              var sb = new StringBuilder();

              sb.AppendLine($"{errorMessage}");
              sb.AppendLine($"{errorResults.Message}");
              sb.AppendLine($"Errors:");

              foreach (var e in errorResults.Errors) {
                sb.Append($"  * {e.Field} ");
                sb.Append($"({e.TranslatedField}): ");
                sb.AppendLine($"{e.Message}");
              }

              queueItem.LastErrorMessage = sb.ToString();
            }
          }
          catch {
          }
        }

        if (isUnprocessableEntity) {
          queueItem.Status = QueueProcessingStatus.Failed;
        }
        else {
          queueItem.RetryCount++;
          queueItem.Status = queueItem.RetryCount < MaxRetryCount ? QueueProcessingStatus.Retrying : QueueProcessingStatus.Failed;
        }

        if (queueItem.Status != QueueProcessingStatus.Failed) {
          queueItem.RetryAfter = GetRetryAfter(queueItem.RetryCount);
        }
      }
      else {
        var s = queueItem.Status;
        queueItem.ResetPostingStatus();
        if (s != QueueProcessingStatus.Skipped) {
          queueItem.Status = QueueProcessingStatus.Processed;
        }
        else {
          queueItem.Status = s;
        }
      }
    }

    public class Commerce7ErrorResponse {

      public int StatusCode { get; set; }

      public string Type { get; set; }

      public string Message { get; set; }

      public Commerce7Error[] Errors { get; set; }
    }

    public class Commerce7Error {

      public string Field { get; set; }

      public string TranslatedField { get; set; }

      public string Message { get; set; }
    }
  }
}