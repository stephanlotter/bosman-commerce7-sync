﻿/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales {

  public interface IPostToEvolutionSalesOrderService {

    Result<IOnlineSalesOrder> Post(PostToEvolutionSalesOrderContext context, IOnlineSalesOrder onlineSalesOrder);
  }
}