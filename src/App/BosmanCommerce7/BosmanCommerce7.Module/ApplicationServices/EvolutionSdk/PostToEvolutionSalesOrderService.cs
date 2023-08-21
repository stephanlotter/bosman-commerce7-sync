/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.Extensions.EvolutionSdk;
using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.EvolutionSdk;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class PostToEvolutionSalesOrderService : IPostToEvolutionSalesOrderService {
    private readonly ILogger<PostToEvolutionSalesOrderService> _logger;
    private readonly IEvolutionSdk _evolutionSdk;
    private readonly ISalesOrdersPostValueStoreService _salesOrdersPostValueStoreService;
    private readonly IEvolutionCustomerRepository _evolutionCustomerRepository;
    private readonly IEvolutionProjectRepository _evolutionProjectRepository;
    private readonly IEvolutionDeliveryMethodRepository _evolutionDeliveryMethodRepository;
    private readonly IEvolutionSalesRepresentativeRepository _evolutionSalesRepresentativeRepository;

    public PostToEvolutionSalesOrderService(ILogger<PostToEvolutionSalesOrderService> logger,
      IEvolutionSdk evolutionSdk,
      ISalesOrdersPostValueStoreService salesOrdersPostValueStoreService,
      IEvolutionCustomerRepository evolutionCustomerRepository,
      IEvolutionProjectRepository evolutionProjectRepository,
      IEvolutionDeliveryMethodRepository evolutionDeliveryMethodRepository,
      IEvolutionSalesRepresentativeRepository evolutionSalesRepresentativeRepository) {
      _logger = logger;
      _evolutionSdk = evolutionSdk;
      _salesOrdersPostValueStoreService = salesOrdersPostValueStoreService;
      _evolutionCustomerRepository = evolutionCustomerRepository;
      _evolutionProjectRepository = evolutionProjectRepository;
      _evolutionDeliveryMethodRepository = evolutionDeliveryMethodRepository;
      _evolutionSalesRepresentativeRepository = evolutionSalesRepresentativeRepository;
    }

    public Result<OnlineSalesOrder> Post(PostToEvolutionSalesOrderContext context, OnlineSalesOrder onlineSalesOrder) {
      return _evolutionSdk.WrapInSdkTransaction(connection => {
        try {
          return CreateSalesOrderHeader(context, onlineSalesOrder)

          .Bind(salesOrder => AddSalesOrderLines(context, salesOrder, onlineSalesOrder))

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
      return NewSalesOrder()

        .Bind(salesOrder => {
          var parameters = new CustomerDescriptor { EmailAddress = onlineSalesOrder.EmailAddress };
          return RepositoryGet(salesOrder, parameters, p => _evolutionCustomerRepository.Get((CustomerDescriptor)p), customer => salesOrder.Customer = customer);
        })

        .Bind(salesOrder => RepositoryGetFromCode(salesOrder, onlineSalesOrder.ProjectCode, _evolutionProjectRepository.Get, project => salesOrder.Project = project))

        .Bind(salesOrder => _salesOrdersPostValueStoreService
          .GetDefaultSalesRepresentativeCode()
          .Bind(code => RepositoryGetFromCode(salesOrder, code, _evolutionSalesRepresentativeRepository.Get, representative => salesOrder.Representative = representative))
        )

        .Bind(salesOrder => _salesOrdersPostValueStoreService
          .GetDefaultDeliveryMethodCode()
          .Bind(code => RepositoryGetFromCode(salesOrder, code, _evolutionDeliveryMethodRepository.Get, deliveryMethod => salesOrder.DeliveryMethod = deliveryMethod))
        )

        .Bind(salesOrder => {
          var deliveryAddress = onlineSalesOrder.ShipToAddress();
          deliveryAddress = AddCustomerNameToAddress(deliveryAddress);
          deliveryAddress = AddPhoneNumberToAddress(deliveryAddress);
          salesOrder.DeliverTo = deliveryAddress;
          return Result.Success(salesOrder);
        })

        .Bind(salesOrder => {
          salesOrder.DiscountPercent = onlineSalesOrder.IsStoreOrder ? salesOrder.Customer.AutomaticDiscount : 0d;
          return Result.Success(salesOrder);
        })
        ;

      Result<SalesOrder> NewSalesOrder() => Result.Success(new SalesOrder {
        ExternalOrderNo = $"{onlineSalesOrder.OrderNumber}",
        OrderDate = onlineSalesOrder.OrderDate,
        TaxMode = TaxMode.Inclusive
      });

      Result<SalesOrder> RepositoryGet<T>(SalesOrder salesOrder, object parameters, Func<object, Result<T>> get, Action<T> onSuccess) {
        return get(parameters).Bind(result => {
          onSuccess(result);
          return Result.Success(salesOrder);
        });
      }

      Result<SalesOrder> RepositoryGetFromCode<T>(SalesOrder salesOrder, string? parameters, Func<string, Result<T>> get, Action<T> onSuccess) {
        return parameters != null ? RepositoryGet(salesOrder, parameters, p => get((string)p), onSuccess) : Result.Failure<SalesOrder>("Parameters may not be null");
      }

      Address AddCustomerNameToAddress(Address deliveryAddress) {
        if (!onlineSalesOrder.IsStoreOrder || string.IsNullOrWhiteSpace(onlineSalesOrder.ShipToName)) { return deliveryAddress; }

        deliveryAddress = deliveryAddress
          .MoveEmptyLineToBottom()
          .TryAddValueToTop(onlineSalesOrder.ShipToName);

        return deliveryAddress;
      }

      Address AddPhoneNumberToAddress(Address deliveryAddress) {
        if (!onlineSalesOrder.IsStoreOrder || string.IsNullOrWhiteSpace(onlineSalesOrder.ShipToPhoneNumber)) { return deliveryAddress; }

        deliveryAddress = deliveryAddress
          .MoveEmptyLineToBottom()
          .WriteToFirstEmptySpace(onlineSalesOrder.ShipToPhoneNumber);

        return deliveryAddress;
      }

    }

    private Result<SalesOrder> AddSalesOrderLines(PostToEvolutionSalesOrderContext context, SalesOrder salesOrder, OnlineSalesOrder onlineSalesOrder) {

      foreach (var onlineSalesOrderLine in onlineSalesOrder.SalesOrderLines.OrderBy(a => a.LineType)) {

        var result = onlineSalesOrderLine.LineType switch {
          SalesOrderLineType.Inventory => AddSalesOrderInventoryLine(context, salesOrder, onlineSalesOrder, onlineSalesOrderLine),
          SalesOrderLineType.GeneralLedger => AddSalesOrderGeneralLedgerLine(context, salesOrder, onlineSalesOrder, onlineSalesOrderLine),
          _ => throw new NotImplementedException()
        };

        if (result.IsFailure) { return result; }

      }

      return Result.Success(salesOrder);
    }

    private Result<SalesOrder> AddSalesOrderInventoryLine(PostToEvolutionSalesOrderContext context, SalesOrder salesOrder, OnlineSalesOrder onlineSalesOrder, OnlineSalesOrderLine onlineSalesOrderLine) {
      // Add line note if not null

      try {
        if (string.IsNullOrWhiteSpace(onlineSalesOrderLine.Sku)) {
          return Result.Failure<SalesOrder>($"Sku on line with Oid {onlineSalesOrderLine.Oid} is blank");
        }

        //var findInventoryItem = genericSalesOrderLineDto.ItemCode.FindInventoryItem();
        //if (findInventoryItem.IsFailure) {
        //  return Result.Fail<OrderDetail>(findInventoryItem.Error);
        //}

        //var line = evolutionSalesOrder.NewSalesOrderLine();

        //line.InventoryItem = findInventoryItem.Value;
        //line.Warehouse = new Warehouse(genericSalesOrderLineDto.WarehouseCode);
        //line.Quantity = genericSalesOrderLineDto.Quantity;
        //line.Reserved = line.WarehouseContext.QtyFree >= genericSalesOrderLineDto.Quantity
        //  ? genericSalesOrderLineDto.Quantity
        //  : line.WarehouseContext.QtyFree;

        //var sellingPriceResult = line.InventoryItem.GetSellingPrice(priceListId);
        //if (sellingPriceResult.IsFailure) {
        //  return Result.Fail<OrderDetail>(sellingPriceResult.Error);
        //}

        //line.PriceListNameID = sellingPriceResult.Value.Item1;
        //line.UnitSellingPrice = sellingPriceResult.Value.Item2.Value;

        return Result.Success(salesOrder);
      }
      catch (Exception ex) {
        var message = $"Error adding inventory line with code '{onlineSalesOrderLine.Sku}' (Oid:{onlineSalesOrderLine.Oid}): {ex.Message}";
        _logger.LogError("{error}", message);
        _logger.LogError(ex, "Error adding inventory line");
        return Result.Failure<SalesOrder>(message);
      }

    }

    private Result<SalesOrder> AddSalesOrderGeneralLedgerLine(PostToEvolutionSalesOrderContext context, SalesOrder salesOrder, OnlineSalesOrder onlineSalesOrder, OnlineSalesOrderLine onlineSalesOrderLine) {
      // Add line note if not null
      return Result.Success(salesOrder);
    }

  }

}
