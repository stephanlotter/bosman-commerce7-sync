/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-01-25
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models;
using BosmanCommerce7.Module.BusinessObjects.SalesOrders;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales {

  public class PostToEvolutionSalesAssociateTipService : IPostToEvolutionSalesAssociateTipService {

    public PostToEvolutionSalesAssociateTipService(ILogger<PostToEvolutionSalesAssociateTipService> logger) {
    }

    public Result<(OnlineSalesOrder onlineSalesOrder, SalesOrder salesOrder)> Post(PostToEvolutionSalesOrderContext context, (OnlineSalesOrder onlineSalesOrder, SalesOrder salesOrder) orderDetails) {
      // TODO: Add logic to post tip amounts. For POS orders
      // TODO: Post GL journal using transaction type for POS tips
      // TODO: Journal GL accounts for tips defined in ValueStore
      throw new NotImplementedException();
    }
  }
}