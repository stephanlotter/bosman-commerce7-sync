﻿/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.Extensions.EvolutionSdk;
using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.EvolutionSdk;
using BosmanCommerce7.Module.Models.LocalDatabase;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class PostToEvolutionSalesOrderService : IPostToEvolutionSalesOrderService {
    private readonly ILogger<PostToEvolutionSalesOrderService> _logger;
    private readonly IEvolutionSdk _evolutionSdk;
    private readonly ISalesOrdersPostValueStoreService _salesOrdersPostValueStoreService;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IEvolutionCustomerRepository _evolutionCustomerRepository;
    private readonly IEvolutionProjectRepository _evolutionProjectRepository;
    private readonly IEvolutionDeliveryMethodRepository _evolutionDeliveryMethodRepository;
    private readonly IEvolutionSalesRepresentativeRepository _evolutionSalesRepresentativeRepository;
    private readonly IEvolutionInventoryItemRepository _evolutionInventoryItemRepository;
    private readonly IEvolutionWarehouseRepository _evolutionWarehouseRepository;
    private readonly IEvolutionPriceListRepository _evolutionPriceListRepository;
    private readonly IEvolutionGeneralLedgerAccountRepository _evolutionGeneralLedgerAccountRepository;

    public PostToEvolutionSalesOrderService(ILogger<PostToEvolutionSalesOrderService> logger,
      IEvolutionSdk evolutionSdk,
      ISalesOrdersPostValueStoreService salesOrdersPostValueStoreService,
      IWarehouseRepository warehouseRepository,
      IEvolutionCustomerRepository evolutionCustomerRepository,
      IEvolutionProjectRepository evolutionProjectRepository,
      IEvolutionDeliveryMethodRepository evolutionDeliveryMethodRepository,
      IEvolutionSalesRepresentativeRepository evolutionSalesRepresentativeRepository,
      IEvolutionInventoryItemRepository evolutionInventoryItemRepository,
      IEvolutionWarehouseRepository evolutionWarehouseRepository,
      IEvolutionPriceListRepository evolutionPriceListRepository,
      IEvolutionGeneralLedgerAccountRepository evolutionGeneralLedgerAccountRepository) {
      _logger = logger;
      _evolutionSdk = evolutionSdk;
      _salesOrdersPostValueStoreService = salesOrdersPostValueStoreService;
      _warehouseRepository = warehouseRepository;
      _evolutionCustomerRepository = evolutionCustomerRepository;
      _evolutionProjectRepository = evolutionProjectRepository;
      _evolutionDeliveryMethodRepository = evolutionDeliveryMethodRepository;
      _evolutionSalesRepresentativeRepository = evolutionSalesRepresentativeRepository;
      _evolutionInventoryItemRepository = evolutionInventoryItemRepository;
      _evolutionWarehouseRepository = evolutionWarehouseRepository;
      _evolutionPriceListRepository = evolutionPriceListRepository;
      _evolutionGeneralLedgerAccountRepository = evolutionGeneralLedgerAccountRepository;
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
        });

      Result<SalesOrder> NewSalesOrder() {
        return Result.Success(new SalesOrder {
          ExternalOrderNo = $"{onlineSalesOrder.OrderNumber}",
          OrderDate = onlineSalesOrder.OrderDate,
          TaxMode = TaxMode.Inclusive
        });
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
      try {
        if (string.IsNullOrWhiteSpace(onlineSalesOrderLine.Sku)) {
          return Result.Failure<SalesOrder>($"Sku on line with Oid {onlineSalesOrderLine.Oid} is blank");
        }

        return NewSalesOrderLine(salesOrder)

          .Bind(salesOrderLine => RepositoryGetFromCode(salesOrderLine, onlineSalesOrderLine.Sku, _evolutionInventoryItemRepository.Get, inventoryItem => salesOrderLine.InventoryItem = inventoryItem))

          .Bind(salesOrderLine => {
            return _warehouseRepository.FindWarehouseCode(new FindWarehouseCodeDescriptor {
              IsStoreOrder = onlineSalesOrder.IsStoreOrder,
              PostalCode = onlineSalesOrder.ShipToAddressPostalCode,
              ObjectSpace = context.ObjectSpace
            })
            .Bind(warehouseCode => RepositoryGetFromCode(salesOrderLine, warehouseCode, _evolutionWarehouseRepository.Get, warehouse => salesOrderLine.Warehouse = warehouse));
          })

          .Bind(salesOrderLine => {

            int? customerPriceListId = salesOrder.Customer.DefaultPriceListID > 0 ? salesOrder.Customer.DefaultPriceListID : null;

            return _evolutionPriceListRepository
              .Get(salesOrderLine.InventoryItem.ID, customerPriceListId)
              .Bind(evolutionPrice => {
                salesOrderLine.PriceListNameID = evolutionPrice.UsedPriceListId;
                salesOrderLine.UnitSellingPrice = evolutionPrice.UnitPriceInVat;
                return Result.Success(salesOrderLine);
              });

          })

          .Bind(salesOrderLine => {
            salesOrderLine.Quantity = onlineSalesOrderLine.Quantity;
            salesOrderLine.Reserved = Math.Min(salesOrderLine.WarehouseContext.QtyFree, onlineSalesOrderLine.Quantity);

            if (!string.IsNullOrWhiteSpace(onlineSalesOrderLine.LineNotes)) {
              salesOrderLine.Note = onlineSalesOrderLine.LineNotes;
            }

            return Result.Success(salesOrderLine);
          })

          .Map(_ => salesOrder);
      }
      catch (Exception ex) {
        var message = $"Error adding inventory line with code '{onlineSalesOrderLine.Sku}' (Oid:{onlineSalesOrderLine.Oid}): {ex.Message}";
        _logger.LogError("{error}", message);
        _logger.LogError(ex, "Error adding inventory line");
        return Result.Failure<SalesOrder>(message);
      }

    }

    private Result<SalesOrder> AddSalesOrderGeneralLedgerLine(PostToEvolutionSalesOrderContext context, SalesOrder salesOrder, OnlineSalesOrder onlineSalesOrder, OnlineSalesOrderLine onlineSalesOrderLine) {
      try {
        return NewSalesOrderLine(salesOrder)

          .Bind(salesOrderLine => RepositoryGetFromCode(salesOrderLine, onlineSalesOrderLine.Sku, _evolutionGeneralLedgerAccountRepository.Get, account => salesOrderLine.Account = account))

          .Bind(salesOrderLine => {

            salesOrderLine.Quantity = onlineSalesOrderLine.Quantity;
            salesOrderLine.UnitSellingPrice = (double)onlineSalesOrderLine.UnitPriceInVat;

            if (!string.IsNullOrWhiteSpace(onlineSalesOrderLine.LineDescription)) {
              salesOrderLine.Description = onlineSalesOrderLine.LineDescription;
            }

            if (!string.IsNullOrWhiteSpace(onlineSalesOrderLine.LineNotes)) {
              salesOrderLine.Note = onlineSalesOrderLine.LineNotes;
            }

            return Result.Success(salesOrderLine);
          })

          .Map(_ => salesOrder);
      }
      catch (Exception ex) {
        var message = $"Error adding general ledger line with code '{onlineSalesOrderLine.Sku}' (Oid:{onlineSalesOrderLine.Oid}): {ex.Message}";
        _logger.LogError("{error}", message);
        _logger.LogError(ex, "Error adding general ledger line");
        return Result.Failure<SalesOrder>(message);
      }

    }

    private Result<OrderDetail> NewSalesOrderLine(SalesOrder salesOrder) {
      return Result.Success(new OrderDetail {
        TaxMode = salesOrder.TaxMode
      })
      .Bind(salesOrderLine => {
        salesOrder.Detail.Add(salesOrderLine);
        return Result.Success(salesOrderLine);
      });
    }

    private Result<SalesOrder> RepositoryGet<T>(SalesOrder salesOrder, object parameters, Func<object, Result<T>> get, Action<T> onSuccess) {
      return get(parameters).Bind(result => {
        onSuccess(result);
        return Result.Success(salesOrder);
      });
    }

    private Result<SalesOrder> RepositoryGetFromCode<T>(SalesOrder salesOrder, string? parameters, Func<string, Result<T>> get, Action<T> onSuccess) {
      return parameters != null ? RepositoryGet(salesOrder, parameters, p => get((string)p), onSuccess) : Result.Failure<SalesOrder>("Parameters may not be null");
    }

    private Result<OrderDetail> RepositoryGetFromCode<T>(OrderDetail salesOrderLine, string? parameters, Func<string, Result<T>> get, Action<T> onSuccess) {
      return parameters != null ? get(parameters).Bind(result => {
        onSuccess(result);
        return Result.Success(salesOrderLine);
      }) : Result.Failure<OrderDetail>("Parameters may not be null");
    }

  }

}