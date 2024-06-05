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

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales {

  public interface IPostToEvolutionCustomerPaymentService {

    Result<(IOnlineSalesOrder onlineSalesOrder, IEvolutionCustomerDocument customerDocument)> Post(PostToEvolutionSalesOrderContext context, (IOnlineSalesOrder onlineSalesOrder, IEvolutionCustomerDocument customerDocument) orderDetails);
  }
}