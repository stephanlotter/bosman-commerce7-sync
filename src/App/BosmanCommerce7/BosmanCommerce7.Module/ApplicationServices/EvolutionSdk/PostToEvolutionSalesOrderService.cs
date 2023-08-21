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
using Microsoft.Extensions.Logging;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class PostToEvolutionSalesOrderService : IPostToEvolutionSalesOrderService {
    private readonly ILogger<PostToEvolutionSalesOrderService> _logger;
    private readonly IEvolutionSdk _evolutionSdk;
    private readonly IEvolutionCustomerRepository _evolutionCustomerRepository;
    private readonly IEvolutionProjectRepository _evolutionProjectRepository;

    public PostToEvolutionSalesOrderService(ILogger<PostToEvolutionSalesOrderService> logger,
      IEvolutionSdk evolutionSdk,
      IEvolutionCustomerRepository evolutionCustomerRepository,
      IEvolutionProjectRepository evolutionProjectRepository) {
      _logger = logger;
      _evolutionSdk = evolutionSdk;
      _evolutionCustomerRepository = evolutionCustomerRepository;
      _evolutionProjectRepository = evolutionProjectRepository;
    }

    public Result<OnlineSalesOrder> Post(PostToEvolutionSalesOrderContext context, OnlineSalesOrder onlineSalesOrder) {
      return _evolutionSdk.WrapInSdkTransaction(connection => {
        try {
          return CreateSalesOrderHeader(context, onlineSalesOrder)

          .Bind(salesOrder => {
            // Add lines
            // Add line note if not null

            return Result.Success(salesOrder);
          })

          .Bind(salesOrder => {
            salesOrder.Save();
            onlineSalesOrder.EvolutionSalesOrderNumber = salesOrder.OrderNo;
            onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Posted;
            onlineSalesOrder.DatePosted = DateTime.Now;
            return Result.Success(onlineSalesOrder);
          });
        }
        catch (Exception ex) {
          _logger.LogError(ex, "Error posting sales order Online Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
          return Result.Failure<OnlineSalesOrder>(ex.Message);
        }
      });
    }

    private Result<SalesOrder> CreateSalesOrderHeader(PostToEvolutionSalesOrderContext context, OnlineSalesOrder onlineSalesOrder) {
      //salesOrder.DeliveryMethod = ;
      //salesOrder.Representative = ;

      /*
      var deliveryAddress = GetDeliveryAddress(genericSalesOrderDto, evolutionSalesOrder);
      if (genericSalesOrderDto.BlackboxxTransactionType == BlackboxxTransactionType.StoreOrder) {
        deliveryAddress = AddCustomerNameToTopOfDeliveryAddress(deliveryAddress, genericSalesOrderDto);
        deliveryAddress = AddPhoneNumberToEndOfDeliveryAddress(deliveryAddress, genericSalesOrderDto);
      }
      evolutionSalesOrder.DeliverTo = deliveryAddress;
       
       */

      // set other header details

      return NewSalesOrder()

        .Bind(salesOrder => {
          return _evolutionCustomerRepository
            .GetCustomer(new Models.EvolutionSdk.CustomerDescriptor { EmailAddress = onlineSalesOrder.EmailAddress })
            .Bind(customer => {
              salesOrder.Customer = customer;
              return Result.Success(salesOrder);
            });
        })

        .Bind(salesOrder => {
          return _evolutionProjectRepository
            .GetProject(onlineSalesOrder.ProjectCode)
            .Bind(project => {
              salesOrder.Project = project;
              return Result.Success(salesOrder);
            });
        })

        .Bind(salesOrder => {
          salesOrder.DiscountPercent = onlineSalesOrder.IsStoreOrder ? salesOrder.Customer.AutomaticDiscount : 0d;

          return Result.Success(salesOrder);
        })

        .Bind(salesOrder => {

          return Result.Success(salesOrder);
        })
        ;

      Result<SalesOrder> NewSalesOrder() => Result.Success(new SalesOrder {
        ExternalOrderNo = $"{onlineSalesOrder.OrderNumber}",
        OrderDate = onlineSalesOrder.OrderDate,
        TaxMode = TaxMode.Inclusive
      });

    }

  }

}
