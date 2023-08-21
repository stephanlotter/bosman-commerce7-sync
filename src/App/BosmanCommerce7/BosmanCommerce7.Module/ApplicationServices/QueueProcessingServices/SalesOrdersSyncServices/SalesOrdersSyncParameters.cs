/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public record SalesOrdersSyncParameters {

    public double FetchNumberOfDaysBack { get; init; }

    public string? ShippingGeneralLedgerAccountCode { get; init; }

    public string? ShippingTaxType { get; init; }

  }
}

