/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public interface ISalesOrdersSyncValueStoreService {

    Result<DateTime?> GetSalesOrdersSyncLastSynced();

    Result UpdateSalesOrdersSyncLastSynced(DateTime? dateTime);

    Result<int?> GetFetchNumberOfDaysBack();

    Result<string?> GetShippingGeneralLedgerAccountCode();

    Result<string?> GetShippingTaxType();

    Result<string?> GetChannelProjectCode(string? channel);

  }
}