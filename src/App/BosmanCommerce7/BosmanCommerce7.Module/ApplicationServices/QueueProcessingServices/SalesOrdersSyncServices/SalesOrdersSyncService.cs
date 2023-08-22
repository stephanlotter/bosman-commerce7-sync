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
using CSharpFunctionalExtensions.ValueTasks;
using DevExpress.Data.Filtering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices {

  public class SalesOrdersSyncService : SyncServiceBase, ISalesOrdersSyncService {

    private readonly SalesOrdersSyncJobOptions _salesOrdersSyncJobOptions;
    private readonly ISalesOrdersSyncValueStoreService _salesOrdersSyncValueStoreService;
    private readonly ISalesOrdersApiClient _apiClient;
    private readonly ILocalObjectSpaceProvider _localObjectSpaceProvider;

    private readonly List<string> _processedOrders = new();

    public SalesOrdersSyncService(ILogger<SalesOrdersSyncService> logger,
      SalesOrdersSyncJobOptions salesOrdersSyncJobOptions,
      ISalesOrdersSyncValueStoreService salesOrdersSyncValueStoreService,
      ISalesOrdersApiClient apiClient,
      ILocalObjectSpaceProvider localObjectSpaceProvider)
      : base(logger) {
      _salesOrdersSyncJobOptions = salesOrdersSyncJobOptions;
      _salesOrdersSyncValueStoreService = salesOrdersSyncValueStoreService;
      _apiClient = apiClient;
      _localObjectSpaceProvider = localObjectSpaceProvider;
    }

    public Result<SalesOrdersSyncResult> Execute(SalesOrdersSyncContext context) {
      var parametersResult = LoadParameters();
      if (parametersResult.IsFailure) { return Result.Failure<SalesOrdersSyncResult>(parametersResult.Error); }
      var parameters = parametersResult.Value;

      try {
        var lastSynced = _salesOrdersSyncValueStoreService.GetSalesOrdersSyncLastSynced();

        if (lastSynced.IsFailure) {
          Logger.LogError("Unable to execute SalesOrdersSyncService: {error}", lastSynced.Error);
          return Result.Failure<SalesOrdersSyncResult>(lastSynced.Error);
        }

        var orderSubmittedDate = (lastSynced.Value ?? DateTime.Now).AddDays(-parameters.FetchNumberOfDaysBack).Date;

        Logger.LogInformation("Fetching sales orders since: {orderSubmittedDate}", $"{orderSubmittedDate:yyyy-MM-dd HH:mm:ss}");

        var salesOrdersResult = _apiClient.GetSalesOrders(orderSubmittedDate);

        if (salesOrdersResult.IsFailure) {
          Logger.LogError("Unable to execute SalesOrdersSyncService: {error}", salesOrdersResult.Error);
          return Result.Failure<SalesOrdersSyncResult>(salesOrdersResult.Error);
        }

        var response = salesOrdersResult.Value;

        var lastOrderDate = DateTime.MinValue;

        if (response.SalesOrders == null || response.SalesOrders!.Length == 0) {
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

        Result<string?> MapChannelToProjectCode(string? channel) {
          return _salesOrdersSyncValueStoreService
            .GetChannelProjectCode(channel)
            .OnFailureCompensate(error => Result.Failure<string?>($"Unable to map channel to project code: {error}"));
        }

        string NormalizeItemCode(string? code) => (code ?? "").Trim().ToUpper();

        _localObjectSpaceProvider.WrapInObjectSpaceTransaction(objectSpace => {
          OnlineSalesOrder NewOrder(dynamic id, dynamic orderNumber) {
            var criteria = CriteriaOperator.Or("OnlineId".IsEqualToOperator($"{id}"), "OrderNumber".IsEqualToOperator($"{orderNumber}"));
            var orders = objectSpace.GetObjects<OnlineSalesOrder>(criteria);
            if (orders.Count > 1) {
              Logger.LogWarning("Multiple sales orders found:[Order number:{orderNumber}][id:{id}]", $"{orderNumber}", $"{id}");
            }
            var order = orders.FirstOrDefault();
            return order ?? objectSpace.CreateObject<OnlineSalesOrder>();
          }

          OnlineSalesOrderLine NewLine(OnlineSalesOrder localSalesOrder) {
            var localSalesOrderLine = objectSpace.CreateObject<OnlineSalesOrderLine>();
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

            var localSalesOrder = NewOrder(salesOrder.id, salesOrder.orderNumber);

            if (localSalesOrder.OnlineId == $"{salesOrder.id}") {
              Logger.LogWarning("Sales order already exists: {orderNumber} [id:{id}]", $"{salesOrder.orderNumber}", $"{salesOrder.id}");
              continue;
            }

            var channelProjectCode = MapChannelToProjectCode(salesOrder.channel);

            if (channelProjectCode.IsFailure) { throw new Exception(channelProjectCode.Error); }

            localSalesOrder.CustomerOnlineId = salesOrder.customerId;
            localSalesOrder.EmailAddress = salesOrder.customer.emails.Count > 0 ? salesOrder.customer.emails[0]?.email : null;
            localSalesOrder.OnlineId = salesOrder.id;
            localSalesOrder.Channel = salesOrder.channel;
            localSalesOrder.OrderDate = orderDate;
            localSalesOrder.OrderNumber = salesOrder.orderNumber;
            localSalesOrder.ProjectCode = channelProjectCode.Value;

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

              localSalesOrderLine.Sku = NormalizeItemCode(item.sku);
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

                localSalesOrderLine.Sku = NormalizeItemCode(parameters.ShippingGeneralLedgerAccountCode);
                localSalesOrderLine.LineDescription = shipping.title;

                localSalesOrderLine.Quantity = 1;
                localSalesOrderLine.TaxAmount = ConvertCurrencyValue(shipping.tax);
                localSalesOrderLine.OnlineTaxType = parameters.ShippingTaxType;
                localSalesOrderLine.UnitPriceInVat = ConvertCurrencyValue(shipping.price);

                localSalesOrder.Save();

                localSalesOrder.OrderValueInVat += localSalesOrderLine.LineValueInVat;
              }
            }

            localSalesOrder.Save();
          }

        });

        _salesOrdersSyncValueStoreService.UpdateSalesOrdersSyncLastSynced(lastOrderDate.Date);

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

    private Result<SalesOrdersSyncParameters> LoadParameters() {

      Result<SalesOrdersSyncParameters> GetResult<T>(SalesOrdersSyncParameters p, Func<Result<T?>> getValue, Func<SalesOrdersSyncParameters, T?, Result<SalesOrdersSyncParameters>> onFound) {
        return getValue().Bind(value => onFound(p, value));
      }

      Result<SalesOrdersSyncParameters> ParameterNotDefined(string keyName) {
        var error = $"{keyName} is not defined in ValueStore table";
        Logger.LogError("{error}", error);
        return Result.Failure<SalesOrdersSyncParameters>(error);
      }

      var parameters = new SalesOrdersSyncParameters { };

      return GetResult(parameters,
      _salesOrdersSyncValueStoreService.GetShippingGeneralLedgerAccountCode, (p, v) => {
        if (string.IsNullOrWhiteSpace(v)) { return ParameterNotDefined("sales-orders-sync-shipping-general-ledger-account-code"); }
        return p with { ShippingGeneralLedgerAccountCode = v };
      })

      .Bind(pa => GetResult(pa, _salesOrdersSyncValueStoreService.GetShippingTaxType, (p, v) => {
        if (string.IsNullOrWhiteSpace(v)) { return ParameterNotDefined("sales-orders-sync-shipping-tax-type"); }
        return p with { ShippingTaxType = v };
      }))

      .Bind(pa => GetResult(pa, _salesOrdersSyncValueStoreService.GetFetchNumberOfDaysBack, (p, v) => p with { FetchNumberOfDaysBack = v ?? 3 }))

      ;
    }

  }

}

