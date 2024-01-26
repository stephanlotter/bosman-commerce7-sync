/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Customers;
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Inventory;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models;
using BosmanCommerce7.Module.BusinessObjects.SalesOrders;
using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.EvolutionSdk.Customers;
using BosmanCommerce7.Module.Models.LocalDatabase;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales {

  public class PostToEvolutionSalesOrderService : IPostToEvolutionSalesOrderService {
    private readonly ILogger<PostToEvolutionSalesOrderService> _logger;
    private readonly IEvolutionSdk _evolutionSdk;
    private readonly ISalesOrdersPostValueStoreService _salesOrdersPostValueStoreService;
    private readonly IBundleMappingRepository _bundleMappingRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ISalesPersonMappingRepository _salesPersonMappingRepository;
    private readonly IEvolutionCustomerRepository _evolutionCustomerRepository;
    private readonly IEvolutionProjectRepository _evolutionProjectRepository;
    private readonly IEvolutionDeliveryMethodRepository _evolutionDeliveryMethodRepository;
    private readonly IEvolutionSalesRepresentativeRepository _evolutionSalesRepresentativeRepository;
    private readonly IEvolutionInventoryItemRepository _evolutionInventoryItemRepository;
    private readonly IEvolutionWarehouseRepository _evolutionWarehouseRepository;
    private readonly IEvolutionPriceListRepository _evolutionPriceListRepository;
    private readonly IEvolutionGeneralLedgerAccountRepository _evolutionGeneralLedgerAccountRepository;
    private readonly IPostToEvolutionCustomerPaymentService _postToEvolutionCustomerPaymentService;
    private readonly IPostToEvolutionSalesAssociateTipService _postToEvolutionSalesAssociateTipService;

    public PostToEvolutionSalesOrderService(ILogger<PostToEvolutionSalesOrderService> logger,
      IEvolutionSdk evolutionSdk,
      ISalesOrdersPostValueStoreService salesOrdersPostValueStoreService,
      IBundleMappingRepository bundleMappingRepository,
      IWarehouseRepository warehouseRepository,
      ISalesPersonMappingRepository salesPersonMappingRepository,
      IEvolutionCustomerRepository evolutionCustomerRepository,
      IEvolutionProjectRepository evolutionProjectRepository,
      IEvolutionDeliveryMethodRepository evolutionDeliveryMethodRepository,
      IEvolutionSalesRepresentativeRepository evolutionSalesRepresentativeRepository,
      IEvolutionInventoryItemRepository evolutionInventoryItemRepository,
      IEvolutionWarehouseRepository evolutionWarehouseRepository,
      IEvolutionPriceListRepository evolutionPriceListRepository,
      IEvolutionGeneralLedgerAccountRepository evolutionGeneralLedgerAccountRepository,
      IPostToEvolutionCustomerPaymentService postToEvolutionCustomerPaymentService,
      IPostToEvolutionSalesAssociateTipService postToEvolutionSalesAssociateTipService) {
      _logger = logger;
      _evolutionSdk = evolutionSdk;
      _salesOrdersPostValueStoreService = salesOrdersPostValueStoreService;
      _bundleMappingRepository = bundleMappingRepository;
      _warehouseRepository = warehouseRepository;
      _salesPersonMappingRepository = salesPersonMappingRepository;
      _evolutionCustomerRepository = evolutionCustomerRepository;
      _evolutionProjectRepository = evolutionProjectRepository;
      _evolutionDeliveryMethodRepository = evolutionDeliveryMethodRepository;
      _evolutionSalesRepresentativeRepository = evolutionSalesRepresentativeRepository;
      _evolutionInventoryItemRepository = evolutionInventoryItemRepository;
      _evolutionWarehouseRepository = evolutionWarehouseRepository;
      _evolutionPriceListRepository = evolutionPriceListRepository;
      _evolutionGeneralLedgerAccountRepository = evolutionGeneralLedgerAccountRepository;
      _postToEvolutionCustomerPaymentService = postToEvolutionCustomerPaymentService;
      _postToEvolutionSalesAssociateTipService = postToEvolutionSalesAssociateTipService;
    }

    public Result<OnlineSalesOrder> Post(PostToEvolutionSalesOrderContext context, OnlineSalesOrder onlineSalesOrder) {
      return _evolutionSdk.WrapInSdkTransaction(connection => {
        try {

          // TODO: Modify this code to handle refunds as CreditNote

          return CreateSalesOrderHeader(context, onlineSalesOrder)

          .Bind(salesOrder => AddSalesOrderLines(context, salesOrder, onlineSalesOrder))

          .Bind(salesOrder => {
            salesOrder.Save();
            onlineSalesOrder.EvolutionSalesOrderNumber = salesOrder.OrderNo;
            onlineSalesOrder.PostingStatus = SalesOrderPostingStatus.Posted;
            onlineSalesOrder.LastErrorMessage = null;
            onlineSalesOrder.RetryCount = 0;
            onlineSalesOrder.DatePosted = DateTime.Now;
            return Result.Success((onlineSalesOrder, salesOrder));
          })
          .Bind(orderDetails => {
            var (onlineSalesOrder, salesOrder) = orderDetails;
            if (!onlineSalesOrder.IsPosOrder) { return Result.Success(onlineSalesOrder); }

            return _postToEvolutionCustomerPaymentService
              .Post(context, orderDetails)
              .Bind(a => _postToEvolutionSalesAssociateTipService.Post(context, a))
              .Bind(a => Result.Success(a.onlineSalesOrder));
          });
        }
        catch (Exception ex) {
          _logger.LogError(ex, "Error posting sales order Online Order Number {OrderNumber}", onlineSalesOrder.OrderNumber);
          return Result.Failure<OnlineSalesOrder>(ex.Message);
        }
      });
    }

    private Result<SalesOrder> CreateSalesOrderHeader(PostToEvolutionSalesOrderContext context, OnlineSalesOrder onlineSalesOrder) {
      //var c = new CreditNote();

      return NewSalesOrder()

        .Bind(salesOrder => {
          var parameters = new CustomerDescriptor { EmailAddress = onlineSalesOrder.EmailAddress };
          return RepositoryGet(salesOrder, parameters, p => _evolutionCustomerRepository.Get((CustomerDescriptor)p), customer => salesOrder.Customer = customer);
        })

        .Bind(salesOrder => RepositoryGetFromCode(salesOrder, onlineSalesOrder.ProjectCode, _evolutionProjectRepository.Get, project => salesOrder.Project = project))

        .Bind(salesOrder => {
          return (onlineSalesOrder.IsPosOrder
            ? _salesPersonMappingRepository.FindMapping(context.ObjectSpace, onlineSalesOrder.JsonProperties.SalesAssociateName()).Map(a => a?.EvolutionSalesRepCode)
            : _salesOrdersPostValueStoreService.GetDefaultSalesRepresentativeCode())
            .Bind(code => RepositoryGetFromCode(salesOrder, code, _evolutionSalesRepresentativeRepository.Get, representative => salesOrder.Representative = representative));
        })

        .Bind(salesOrder => _salesOrdersPostValueStoreService
          .GetDefaultDeliveryMethodCode()
          .Bind(code => RepositoryGetFromCode(salesOrder, code, _evolutionDeliveryMethodRepository.Get, deliveryMethod => salesOrder.DeliveryMethod = deliveryMethod))
        )

        .Bind(salesOrder => {
          var deliveryAddress = onlineSalesOrder.ShipToAddress();
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

          .Bind(salesOrderLine => {
            var bundleMappingResult = _bundleMappingRepository.FindBundleMapping(context.ObjectSpace, onlineSalesOrderLine.Sku);
            if (bundleMappingResult.IsFailure) { return Result.Failure<OrderDetail>(bundleMappingResult.Error); }

            if (bundleMappingResult.Value != null) {
              salesOrder.ExternalOrderNo = bundleMappingResult.Value.ExternalReferenceCode;
            }

            var sku = bundleMappingResult.Value?.EvolutionCode ?? onlineSalesOrderLine.Sku;
            return RepositoryGetFromCode(salesOrderLine, sku, _evolutionInventoryItemRepository.Get, inventoryItem => salesOrderLine.InventoryItem = inventoryItem);
          })

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