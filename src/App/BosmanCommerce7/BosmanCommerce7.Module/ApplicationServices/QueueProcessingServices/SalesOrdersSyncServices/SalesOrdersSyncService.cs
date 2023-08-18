/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.RestApiClients;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {
  public class SalesOrdersSyncService : SyncServiceBase, ISalesOrdersSyncService {
    private const string ValueStoreKey = "sales-orders-sync-last-synced";

    private readonly SalesOrdersSyncJobOptions _salesOrdersSyncJobOptions;
    private readonly IValueStoreRepository _valueStoreRepository;
    private readonly ISalesOrdersApiClient _apiClient;
    private readonly ILocalObjectSpaceProvider _localObjectSpaceProvider;

    private readonly List<string> _processedOrders = new();

    public SalesOrdersSyncService(ILogger<SalesOrdersSyncService> logger,
      SalesOrdersSyncJobOptions salesOrdersSyncJobOptions,
      IValueStoreRepository valueStoreRepository,
      ISalesOrdersApiClient apiClient,
      ILocalObjectSpaceProvider localObjectSpaceProvider)
      : base(logger) {
      _salesOrdersSyncJobOptions = salesOrdersSyncJobOptions;
      _valueStoreRepository = valueStoreRepository;
      _apiClient = apiClient;
      _localObjectSpaceProvider = localObjectSpaceProvider;
    }

    public Result<SalesOrdersSyncResult> Execute(SalesOrdersSyncContext context) {
      if (string.IsNullOrWhiteSpace(_salesOrdersSyncJobOptions.ShippingGeneralLedgerAccountCode)) {
        Logger.LogError("ShippingGeneralLedgerAccountCode is not configured in appsettings.json");
        return Result.Failure<SalesOrdersSyncResult>("ShippingGeneralLedgerAccountCode is not configured in appsettings.json");
      }

      try {
        var lastSynced = _valueStoreRepository.GetDateTimeValue(ValueStoreKey, _salesOrdersSyncJobOptions.StartDate);

        if (lastSynced.IsFailure) {
          Logger.LogError("Unable to execute SalesOrdersSyncService: {error}", lastSynced.Error);
          return Result.Failure<SalesOrdersSyncResult>(lastSynced.Error);
        }

        var orderSubmittedDate = (lastSynced.Value ?? DateTime.Now).AddDays(-_salesOrdersSyncJobOptions.FetchNumberOfDaysBack).Date;

        Logger.LogInformation("Fetching sales orders since: {orderSubmittedDate}", $"{orderSubmittedDate:yyyy-MM-dd HH:mm:ss}");

        var salesOrdersResult = _apiClient.GetSalesOrders(orderSubmittedDate);

        if (salesOrdersResult.IsFailure) {
          Logger.LogError("Unable to execute SalesOrdersSyncService: {error}", salesOrdersResult.Error);
          return Result.Failure<SalesOrdersSyncResult>(salesOrdersResult.Error);
        }

        var response = salesOrdersResult.Value;
        var lastOrderDate = DateTime.MinValue;

        if (response.SalesOrders == null || response.SalesOrders!.Count == 0) {
          Logger.LogInformation("No sales orders found since: {orderSubmittedDate}", $"{orderSubmittedDate:yyyy-MM-dd HH:mm:ss}");
          return Result.Success(BuildResult());
        }

        string BuildShipToName(string firstName, string lastName) {
          return $"{(firstName ?? "").Trim()} {(lastName ?? "").Trim()}".Trim();
        }

        string BuildLineDescription(string productTitle, string productVariantTitle) {
          return $"{(productTitle ?? "").Trim()} {(productVariantTitle ?? "").Trim()}".Trim();
        }

        double ConvertCurrencyValue(dynamic amount) {
          if (double.TryParse($"{amount}", out var result)) {
            return result / 100d;
          }
          return 0d;
        }

        _localObjectSpaceProvider.WrapInObjectSpaceTransaction(objectSpace => {
          SalesOrder NewOrder(dynamic id) {
            var order = objectSpace.FindObject<SalesOrder>("OnlineId".IsEqualToOperator($"{id}"));
            return order ?? objectSpace.CreateObject<SalesOrder>();
          }

          SalesOrderLine NewLine(SalesOrder localSalesOrder) {
            var localSalesOrderLine = objectSpace.CreateObject<SalesOrderLine>();
            localSalesOrder.SalesOrderLines.Add(localSalesOrderLine);
            return localSalesOrderLine;
          }

          foreach (dynamic salesOrder in response.SalesOrders!) {
            Logger.LogInformation("Sales order found: {orderNumber}", $"{salesOrder.orderNumber}");

            var orderDate = (DateTime)salesOrder.orderSubmittedDate;
            lastOrderDate = lastOrderDate > orderDate ? lastOrderDate : orderDate;

            if (_processedOrders.Contains($"{salesOrder.id}")) {
              Logger.LogWarning("Sales order already processed: {orderNumber} [id:{id}]", $"{salesOrder.orderNumber}", $"{salesOrder.id}");
              continue;
            }

            _processedOrders.Add($"{salesOrder.id}");

            var localSalesOrder = NewOrder(salesOrder.id);

            if (localSalesOrder.OnlineId == $"{salesOrder.id}") {
              Logger.LogWarning("Sales order already exists: {orderNumber} [id:{id}]", $"{salesOrder.orderNumber}", $"{salesOrder.id}");
              continue;
            }

            localSalesOrder.CustomerId = salesOrder.customerId;
            localSalesOrder.OnlineId = salesOrder.id;
            localSalesOrder.Channel = salesOrder.channel;
            localSalesOrder.OrderDate = orderDate;
            localSalesOrder.OrderNumber = salesOrder.orderNumber;

            if (salesOrder.shipTo != null) {
              localSalesOrder.ShipToName = BuildShipToName(salesOrder.shipTo.firstName, salesOrder.shipTo.lastName);
              localSalesOrder.ShipToPhoneNumber = salesOrder.shipTo.phone;

              localSalesOrder.ShipToAddress1 = salesOrder.shipTo.address;
              localSalesOrder.ShipToAddress2 = salesOrder.shipTo.address2;
              localSalesOrder.ShipToAddressCity = salesOrder.shipTo.city;
              localSalesOrder.ShipToAddressProvince = salesOrder.shipTo.stateCode;
              localSalesOrder.ShipToAddressPostalCode = salesOrder.shipTo.zipCode;
              localSalesOrder.ShipToAddressCountryCode = salesOrder.shipTo.countryCode;
            }

            localSalesOrder.OrderJson = JsonConvert.SerializeObject(salesOrder);

            foreach (var item in salesOrder.items) {
              var localSalesOrderLine = NewLine(localSalesOrder);

              localSalesOrderLine.OnlineId = item.id;
              localSalesOrderLine.LineType = SalesOrderLineType.Inventory;

              localSalesOrderLine.Sku = item.sku;
              localSalesOrderLine.LineDescription = BuildLineDescription(item.productTitle, item.productVariantTitle);

              localSalesOrderLine.Quantity = item.quantity;
              localSalesOrderLine.TaxAmount = ConvertCurrencyValue(item.tax);
              localSalesOrderLine.OnlineTaxType = item.taxType;
              localSalesOrderLine.UnitPriceInVat = ConvertCurrencyValue(item.price);

              if (!string.IsNullOrWhiteSpace($"{item.notes}")) { localSalesOrderLine.LineNotes = $"{item.notes}"; }

              localSalesOrderLine.Save();

              localSalesOrder.OrderValueInVat += localSalesOrderLine.LineValueInVat;
            }

            if (salesOrder.shipping != null) {
              foreach (var shipping in salesOrder.shipping) {
                var price = ConvertCurrencyValue(shipping.price);
                if (price == 0) { continue; }

                var localSalesOrderLine = NewLine(localSalesOrder);

                localSalesOrderLine.OnlineId = shipping.id;
                localSalesOrderLine.LineType = SalesOrderLineType.GeneralLedger;

                localSalesOrderLine.Sku = _salesOrdersSyncJobOptions.ShippingGeneralLedgerAccountCode;
                localSalesOrderLine.LineDescription = shipping.title;

                localSalesOrderLine.Quantity = 1;
                localSalesOrderLine.TaxAmount = ConvertCurrencyValue(shipping.tax);
                localSalesOrderLine.OnlineTaxType = _salesOrdersSyncJobOptions.ShippingTaxType;
                localSalesOrderLine.UnitPriceInVat = ConvertCurrencyValue(shipping.price);

                localSalesOrder.Save();

                localSalesOrder.OrderValueInVat += localSalesOrderLine.LineValueInVat;
              }
            }

            localSalesOrder.Save();
          }

        });

        _valueStoreRepository.SetDateTimeValue(ValueStoreKey, lastOrderDate.Date);

        return Result.Success(BuildResult());
      }
      catch (Exception ex) {
        Logger.LogError("Unable to execute SalesOrdersSyncService: {error}", ex);
        return Result.Failure<SalesOrdersSyncResult>(ex.Message);
      }

      SalesOrdersSyncResult BuildResult() {
        return new SalesOrdersSyncResult { };
      }

    }

  }

}

