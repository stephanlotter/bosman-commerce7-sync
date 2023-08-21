/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.Extensions.EvolutionSdk {
  public class PostToEvolutionSalesOrderService : IPostToEvolutionSalesOrderService {
    private readonly IEvolutionSdk _evolutionSdk;

    public PostToEvolutionSalesOrderService(IEvolutionSdk evolutionSdk) {
      _evolutionSdk = evolutionSdk;
    }

    public Result<OnlineSalesOrder> Post(PostToEvolutionSalesOrderContext context, OnlineSalesOrder onlineSalesOrder) {
      // TODO: Implement this


      // create new Pastel.Evolution.SalesOrder
      var salesOrder = new SalesOrder {
        
        };

      // Set customer auto discount
      // Set sales rep
      // Set project
      // Set delivery method
      // Set delivery address

      // set other header details
      


      // Add lines
      // Add line note if not null

      // TODO: Set onlineSalesOrder.EvolutionSalesOrderNumber
      //onlineSalesOrder.EvolutionSalesOrderNumber = "";
      onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Posted;
      onlineSalesOrder.DatePosted = DateTime.Now;

      return Result.Success(onlineSalesOrder);

    }

  }

}
