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
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models;
using BosmanCommerce7.Module.BusinessObjects.SalesOrders;
using BosmanCommerce7.Module.BusinessObjects.Settings;
using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.EvolutionSdk.Customers;
using BosmanCommerce7.Module.Models.LocalDatabase;
using CSharpFunctionalExtensions;
using DevExpress.ExpressApp;
using Microsoft.Extensions.Logging;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales {

  public class PostToEvolutionSalesOrderService : IPostToEvolutionSalesOrderService {
    private readonly ILogger<PostToEvolutionSalesOrderService> _logger;
    private readonly ISalesOrdersPostValueStoreService _salesOrdersPostValueStoreService;
    private readonly IBundleMappingRepository _bundleMappingRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ISalesPersonMappingRepository _salesPersonMappingRepository;
    private readonly IInventoryLocationsLocalCache _inventoryLocationsLocalCache;
    private readonly IWarehouseLocationMappingRepository _warehouseLocationMappingRepository;
    private readonly IEvolutionCustomerRepository _evolutionCustomerRepository;
    private readonly IEvolutionProjectRepository _evolutionProjectRepository;
    private readonly IEvolutionDeliveryMethodRepository _evolutionDeliveryMethodRepository;
    private readonly IEvolutionSalesRepresentativeRepository _evolutionSalesRepresentativeRepository;
    private readonly IEvolutionInventoryItemRepository _evolutionInventoryItemRepository;
    private readonly IEvolutionWarehouseRepository _evolutionWarehouseRepository;
    private readonly IEvolutionPriceListRepository _evolutionPriceListRepository;
    private readonly IEvolutionGeneralLedgerAccountRepository _evolutionGeneralLedgerAccountRepository;

    public PostToEvolutionSalesOrderService(ILogger<PostToEvolutionSalesOrderService> logger,
      ISalesOrdersPostValueStoreService salesOrdersPostValueStoreService,
      IBundleMappingRepository bundleMappingRepository,
      IWarehouseRepository warehouseRepository,
      ISalesPersonMappingRepository salesPersonMappingRepository,
      IInventoryLocationsLocalCache inventoryLocationsLocalCache,
      IWarehouseLocationMappingRepository warehouseLocationMappingRepository,
      IEvolutionCustomerRepository evolutionCustomerRepository,
      IEvolutionProjectRepository evolutionProjectRepository,
      IEvolutionDeliveryMethodRepository evolutionDeliveryMethodRepository,
      IEvolutionSalesRepresentativeRepository evolutionSalesRepresentativeRepository,
      IEvolutionInventoryItemRepository evolutionInventoryItemRepository,
      IEvolutionWarehouseRepository evolutionWarehouseRepository,
      IEvolutionPriceListRepository evolutionPriceListRepository,
      IEvolutionGeneralLedgerAccountRepository evolutionGeneralLedgerAccountRepository) {
      _logger = logger;
      _salesOrdersPostValueStoreService = salesOrdersPostValueStoreService;
      _bundleMappingRepository = bundleMappingRepository;
      _warehouseRepository = warehouseRepository;
      _salesPersonMappingRepository = salesPersonMappingRepository;
      _inventoryLocationsLocalCache = inventoryLocationsLocalCache;
      _warehouseLocationMappingRepository = warehouseLocationMappingRepository;
      _evolutionCustomerRepository = evolutionCustomerRepository;
      _evolutionProjectRepository = evolutionProjectRepository;
      _evolutionDeliveryMethodRepository = evolutionDeliveryMethodRepository;
      _evolutionSalesRepresentativeRepository = evolutionSalesRepresentativeRepository;
      _evolutionInventoryItemRepository = evolutionInventoryItemRepository;
      _evolutionWarehouseRepository = evolutionWarehouseRepository;
      _evolutionPriceListRepository = evolutionPriceListRepository;
      _evolutionGeneralLedgerAccountRepository = evolutionGeneralLedgerAccountRepository;
    }

    public Result<IOnlineSalesOrder> Post(PostToEvolutionSalesOrderContext context, IOnlineSalesOrder onlineSalesOrder) {
      var logTransactionType = onlineSalesOrder.IsRefund ? "sales order refund" : "sales order";
      var logTransactionIdentifier = onlineSalesOrder.IsRefund
        ? $"[Posting Sales Order][POS Reference:{onlineSalesOrder.OrderNumber}][POS Refunded Order Reference:{onlineSalesOrder.LinkedOrderNumber}]"
        : $"[Posting Sales Order][POS Reference:{onlineSalesOrder.OrderNumber}]";

      _logger.LogInformation("START: Posting {logTransactionType} {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);

      try {
        return CreateSalesOrderHeader(context, onlineSalesOrder)

        .Bind(salesOrder => AddSalesOrderLines(context, salesOrder, onlineSalesOrder))

        .Bind(salesOrder => {
          if (onlineSalesOrder.IsRefund || onlineSalesOrder.IsPosOrder) {
            salesOrder.InvoiceDate = onlineSalesOrder.TransactionDate();
            var invoiceNumber = salesOrder.Complete();
            onlineSalesOrder.SetEvolutionInvoiceNumber(invoiceNumber);
          }
          else {
            salesOrder.Save();
          }

          onlineSalesOrder.SetEvolutionSalesOrderNumber(salesOrder.OrderNo);
          return Result.Success((onlineSalesOrder, salesOrder));
        })
        .Bind(orderDetails => {
          var (onlineSalesOrder, salesOrder) = orderDetails;
          return Result.Success(onlineSalesOrder);
        });
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While posting {logTransactionType} {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);
        return Result.Failure<IOnlineSalesOrder>(ex.Message);
      }
      finally {
        _logger.LogInformation("END: Posting {logTransactionType} {logTransactionIdentifier}", logTransactionType, logTransactionIdentifier);
      }
    }

    private Result<SalesDocumentBase> CreateSalesOrderHeader(PostToEvolutionSalesOrderContext context, IOnlineSalesOrder onlineSalesOrder) {
      return NewSalesOrder()
        .Bind(AssignCustomer)
        .Bind(AssignProject)
        .Bind(AssignSalesPerson)
        .Bind(AssignDeliveryMethod)
        .Bind(AssignDeliveryAddress)
        .Bind(AssignDiscountPercent);

      Result<SalesDocumentBase> NewSalesOrder() {
        if (onlineSalesOrder.IsRefund) {
          return Result.Success((SalesDocumentBase)new CreditNote {
            ExternalOrderNo = $"{onlineSalesOrder.OrderNumber}",
            OrderDate = onlineSalesOrder.TransactionDate(),
            TaxMode = TaxMode.Inclusive
          });
        }

        return Result.Success((SalesDocumentBase)new SalesOrder {
          ExternalOrderNo = $"{onlineSalesOrder.OrderNumber}",
          OrderDate = onlineSalesOrder.TransactionOrderDate(),
          TaxMode = TaxMode.Inclusive
        });
      }

      Result<SalesDocumentBase> AssignCustomer(SalesDocumentBase salesOrder) {
        if (onlineSalesOrder.UseAccountCustomer) {
          var parameters = new CustomerDescriptor { EmailAddress = onlineSalesOrder.EmailAddress };
          return RepositoryGet(salesOrder, parameters, p => _evolutionCustomerRepository.Get((CustomerDescriptor)p), customer => salesOrder.Customer = customer);
        }

        return GetWarehouseLocationMapping(onlineSalesOrder.JsonProperties.ShipInventoryLocationId(), context.ObjectSpace)
          .Bind(warehouseLocationMapping => _salesOrdersPostValueStoreService.GetDefaultCashCustomerCode(warehouseLocationMapping.WarehouseCode!))
          .Bind(defaultCashCustomerCode => {
            _logger.LogWarning("No customer assigned. Using default cash customer {cashCustomerAccountCode}. Order Number {OrderNumber}", defaultCashCustomerCode, onlineSalesOrder.OrderNumber);

            onlineSalesOrder.PostLog($"Using default cash customer {defaultCashCustomerCode}");
            var parameters = new CustomerDescriptor { AccountCode = defaultCashCustomerCode };

            return RepositoryGet(salesOrder, parameters, p => _evolutionCustomerRepository.Get((CustomerDescriptor)p), customer => salesOrder.Customer = customer);
          });
      }

      Result<SalesDocumentBase> AssignProject(SalesDocumentBase salesOrder) {
        return RepositoryGetFromCode(salesOrder, onlineSalesOrder.ProjectCode, _evolutionProjectRepository.Get, project => salesOrder.Project = project);
      }

      Result<SalesDocumentBase> AssignSalesPerson(SalesDocumentBase salesOrder) {
        return (onlineSalesOrder.IsPosOrder
          ? _salesPersonMappingRepository.FindMapping(context.ObjectSpace, onlineSalesOrder.JsonProperties.SalesAssociateName()).Map(a => a?.EvolutionSalesRepCode)
          : _salesOrdersPostValueStoreService.GetDefaultSalesRepresentativeCode())
          .Bind(code => RepositoryGetFromCode(salesOrder, code, _evolutionSalesRepresentativeRepository.Get, representative => salesOrder.Representative = representative));
      }

      Result<SalesDocumentBase> AssignDeliveryMethod(SalesDocumentBase salesOrder) {
        return _salesOrdersPostValueStoreService
                  .GetDefaultDeliveryMethodCode()
                  .Bind(code => RepositoryGetFromCode(salesOrder, code, _evolutionDeliveryMethodRepository.Get, deliveryMethod => salesOrder.DeliveryMethod = deliveryMethod));
      }

      Result<SalesDocumentBase> AssignDeliveryAddress(SalesDocumentBase salesOrder) {
        var deliveryAddress = onlineSalesOrder.ShipToAddress();
        salesOrder.DeliverTo = deliveryAddress;
        return Result.Success(salesOrder);
      }

      Result<SalesDocumentBase> AssignDiscountPercent(SalesDocumentBase salesOrder) {
        salesOrder.DiscountPercent = onlineSalesOrder.IsStoreOrder ? salesOrder.Customer.AutomaticDiscount : 0d;
        return Result.Success(salesOrder);
      }
    }

    private Result<SalesDocumentBase> AddSalesOrderLines(PostToEvolutionSalesOrderContext context, SalesDocumentBase salesOrder, IOnlineSalesOrder onlineSalesOrder) {
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

    private Result<SalesDocumentBase> AddSalesOrderInventoryLine(PostToEvolutionSalesOrderContext context, SalesDocumentBase salesOrder, IOnlineSalesOrder onlineSalesOrder, OnlineSalesOrderLine onlineSalesOrderLine) {
      try {
        if (string.IsNullOrWhiteSpace(onlineSalesOrderLine.Sku)) {
          return Result.Failure<SalesDocumentBase>($"Sku on line with Oid {onlineSalesOrderLine.Oid} is blank");
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
            if (onlineSalesOrder.IsPosOrder) {
              return GetWarehouseLocationMapping(onlineSalesOrder.JsonProperties.ShipInventoryLocationId(), context.ObjectSpace)
                .Bind(warehouseMapping => RepositoryGetFromCode(salesOrderLine, warehouseMapping.WarehouseCode, _evolutionWarehouseRepository.Get, warehouse => salesOrderLine.Warehouse = warehouse));
            }

            return _warehouseRepository.FindWarehouseCode(new FindWarehouseCodeDescriptor {
              IsStoreOrder = onlineSalesOrder.IsStoreOrder,
              PostalCode = onlineSalesOrder.ShipToAddressPostalCode,
              ObjectSpace = context.ObjectSpace
            })
            .Bind(warehouseCode => RepositoryGetFromCode(salesOrderLine, warehouseCode, _evolutionWarehouseRepository.Get, warehouse => salesOrderLine.Warehouse = warehouse));
          })

          .Bind(salesOrderLine => {
            if (onlineSalesOrder.IsPosOrder) {
              salesOrderLine.UnitSellingPrice = onlineSalesOrderLine.UnitPriceInVat;
              return Result.Success(salesOrderLine);
            }

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
            salesOrderLine.Quantity = (onlineSalesOrder.IsRefund ? -1 : 1) * onlineSalesOrderLine.Quantity;

            if (!onlineSalesOrder.IsRefund) {
              salesOrderLine.Reserved = Math.Min(salesOrderLine.WarehouseContext.QtyFree, onlineSalesOrderLine.Quantity);
            }

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
        return Result.Failure<SalesDocumentBase>(message);
      }
    }

    private Result<SalesDocumentBase> AddSalesOrderGeneralLedgerLine(PostToEvolutionSalesOrderContext context, SalesDocumentBase salesOrder, IOnlineSalesOrder onlineSalesOrder, OnlineSalesOrderLine onlineSalesOrderLine) {
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
        return Result.Failure<SalesDocumentBase>(message);
      }
    }

    private Result<OrderDetail> NewSalesOrderLine(SalesDocumentBase salesOrder) {
      return Result.Success(new OrderDetail {
        TaxMode = salesOrder.TaxMode
      })
      .Bind(salesOrderLine => {
        salesOrder.Detail.Add(salesOrderLine);
        return Result.Success(salesOrderLine);
      });
    }

    private Result<WarehouseLocationMapping> GetWarehouseLocationMapping(Commerce7LocationId? inventoryLocationId, IObjectSpace objectSpace) {
      return _inventoryLocationsLocalCache.GetLocationById(inventoryLocationId)
             .Bind(a => _warehouseLocationMappingRepository.FindMappingByLocationTitle(objectSpace, a.Title));
    }

    private Result<SalesDocumentBase> RepositoryGet<T>(SalesDocumentBase salesOrder, object parameters, Func<object, Result<T>> get, Action<T> onSuccess) {
      return get(parameters).Bind(result => {
        onSuccess(result);
        return Result.Success(salesOrder);
      });
    }

    private Result<SalesDocumentBase> RepositoryGetFromCode<T>(SalesDocumentBase salesOrder, string? parameters, Func<string, Result<T>> get, Action<T> onSuccess) {
      return parameters != null ? RepositoryGet(salesOrder, parameters, p => get((string)p), onSuccess) : Result.Failure<SalesDocumentBase>("Parameters may not be null");
    }

    private Result<OrderDetail> RepositoryGetFromCode<T>(OrderDetail salesOrderLine, string? parameters, Func<string, Result<T>> get, Action<T> onSuccess) {
      return parameters != null ? get(parameters).Bind(result => {
        onSuccess(result);
        return Result.Success(salesOrderLine);
      }) : Result.Failure<OrderDetail>("Parameters may not be null");
    }
  }
}