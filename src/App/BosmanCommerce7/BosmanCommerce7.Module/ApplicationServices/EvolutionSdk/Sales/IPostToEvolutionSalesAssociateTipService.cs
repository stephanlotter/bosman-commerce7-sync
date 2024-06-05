/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-01-25
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models;
using CSharpFunctionalExtensions;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales {

  public interface IPostToEvolutionSalesAssociateTipService {

    Result<(IOnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder)> Post(PostToEvolutionSalesOrderContext context, (IOnlineSalesOrder onlineSalesOrder, SalesDocumentBase salesOrder) orderDetails);
  }
}