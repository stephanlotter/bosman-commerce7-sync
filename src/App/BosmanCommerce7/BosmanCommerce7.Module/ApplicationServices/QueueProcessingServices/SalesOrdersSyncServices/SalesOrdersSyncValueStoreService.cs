/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {

  public class SalesOrdersSyncValueStoreService : ISalesOrdersSyncValueStoreService {
    private readonly IValueStoreRepository _valueStoreRepository;

    public SalesOrdersSyncValueStoreService(IValueStoreRepository valueStoreRepository) {
      _valueStoreRepository = valueStoreRepository;
    }

    private Result<DateTime?> GetSalesOrdersSyncStartDate() => _valueStoreRepository.GetDateTimeValue("sales-orders-sync-start-date", new DateTime(2023, 1, 1));

    public Result UpdateSalesOrdersSyncLastSynced(DateTime? dateTime) => _valueStoreRepository.SetDateTimeValue("sales-orders-sync-last-synced", dateTime);

    public Result<DateTime?> GetSalesOrdersSyncLastSynced() {
      return GetSalesOrdersSyncStartDate().Bind(startDate => {
        return _valueStoreRepository.GetDateTimeValue("sales-orders-sync-last-synced", startDate);
      });
    }

    public Result<int?> GetFetchNumberOfDaysBack() {
      return _valueStoreRepository.GetIntValue("sales-orders-sync-fetch-number-of-days-back", 3);
    }

    public Result<string?> GetShippingGeneralLedgerAccountCode() {
      return _valueStoreRepository.GetValue("sales-orders-sync-shipping-general-ledger-account-code");
    }

    public Result<string?> GetShippingTaxType() {
      return _valueStoreRepository.GetValue("sales-orders-sync-shipping-tax-type");
    }

    public Result<string?> GetChannelProjectCode(string? channel, string? posProfile) {
      if (string.IsNullOrWhiteSpace(channel)) { return Result.Failure<string?>("Channel provided is empty."); }

      string NormaliseSegment(string? segment) => string.IsNullOrWhiteSpace(segment) ? "" : segment.Trim().ToLower();

      string NormalisePosProfile(string? segment) {
        var v = NormaliseSegment(segment);
        return string.IsNullOrWhiteSpace(v) ? "" : $"-{segment}";
      }

      var keyName = $"sales-orders-sync-channel-{NormaliseSegment(channel)}{NormalisePosProfile(posProfile)}-project-code";
      return _valueStoreRepository
        .GetValue(keyName)
        .Bind(p => string.IsNullOrWhiteSpace(p)
            ? Result.Failure<string?>($"No project code mapping found for channel: '{channel}'. Please add a record to ValueStore for key: {keyName}")
            : Result.Success<string?>(p));
    }
  }
}