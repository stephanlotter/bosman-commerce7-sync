/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-01-25
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using Newtonsoft.Json;

namespace BosmanCommerce7.Module.ApplicationServices.OnlineSalesOrderServices {

  public class OnlineSalesOrderJsonProperties {
    private dynamic? _data;
    private List<OrderTender> _orderTenders = new List<OrderTender>();

    public OnlineSalesOrderJsonProperties(string json) {
      if (string.IsNullOrEmpty(json)) { throw new ArgumentNullException(nameof(json)); }
      _data = JsonConvert.DeserializeObject<dynamic>(json);
    }

    public string? SalesAssociateName() => $"{_data?.salesAssociate?.name}";

    private IEnumerable<OrderTender> SuccessTenders() => Tenders().Where(a => a.ChargeStatus.Equals("success", StringComparison.InvariantCultureIgnoreCase));

    private dynamic? PosProfile => _data?.posProfile;

    public Commerce7LocationId? ShipInventoryLocationId() => PosProfile == null ? null : Commerce7InventoryId.Parse($"{PosProfile?.shipInventoryLocationId}");

    public List<OrderTender> Tenders() {
      if (_orderTenders.Any()) { return _orderTenders; }
      var tenders = _data?.tenders ?? null;
      if (tenders == null) { return _orderTenders; }

      double Amount(dynamic amount) => ((double)amount) / 100;

      foreach (var tender in tenders) {
        var t = new OrderTender {
          TenderType = tender.tenderType,
          ChargeStatus = tender.chargeStatus,
          AmountTendered = Amount(tender.amountTendered),
          TipAmount = Amount(tender.tip)
        };

        _orderTenders.Add(t);
      }
      return _orderTenders;
    }

    public double PaymentAmount() => SuccessTenders().Sum(a => a.AmountTendered);

    public double TipAmount() => (_data?.tipTotal ?? 0d) / 100;
  }

  public record OrderTender {
    public required string TenderType { get; init; }

    public required string ChargeStatus { get; init; }

    public double AmountTendered { get; init; }

    public double TipAmount { get; init; }
  }
}