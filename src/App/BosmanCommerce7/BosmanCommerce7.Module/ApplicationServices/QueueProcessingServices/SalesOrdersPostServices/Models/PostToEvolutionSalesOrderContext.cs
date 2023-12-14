/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using DevExpress.ExpressApp;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models {
  public record PostToEvolutionSalesOrderContext {
    public required IObjectSpace ObjectSpace { get; init; }
  }
}