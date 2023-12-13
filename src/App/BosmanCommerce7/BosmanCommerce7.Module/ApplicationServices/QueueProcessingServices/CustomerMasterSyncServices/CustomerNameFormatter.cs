/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-28
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using System.Text.RegularExpressions;
using NameParser;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices {
  public record CustomerNameFormatter {
    public const string NoFirstName = "No first name";
    public const string NoLastName = "No last name";

    public string CustomerName { get; }

    public string FirstName { get; }

    public string LastName { get; }

    public CustomerNameFormatter(string? customerName) {
      CustomerName = customerName?.Trim() ?? "";

      if (string.IsNullOrWhiteSpace(CustomerName)) {
        FirstName = NoFirstName;
        LastName = NoLastName;
        return;
      }

      var containsCompanyIdentifier = ContainsCompanyIdentifier(CustomerName);

      if (containsCompanyIdentifier) {
        FirstName = CustomerName;
        LastName = NoLastName;
        return;
      }

      var containsAnd = CustomerName.Contains("&", StringComparison.OrdinalIgnoreCase);

      if (containsAnd) {
        var customerNameSegments = $"{CustomerName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var numberOfSegments = customerNameSegments.Length;
        var andSegmentIndex = customerNameSegments.Select((s, i) => new { Segment = s, Index = i }).FirstOrDefault(s => s.Segment.Equals("&", StringComparison.OrdinalIgnoreCase))?.Index ?? -1;

        FirstName = string.Join(" ", customerNameSegments.Take(andSegmentIndex + 2));

        if (FirstName.StartsWith("&", StringComparison.OrdinalIgnoreCase)) {
          FirstName = FirstName[1..].Trim();
        }

        if (FirstName.EndsWith("&", StringComparison.OrdinalIgnoreCase)) {
          FirstName = FirstName[..^1].Trim();
        }

        var skipCount = andSegmentIndex + 2;
        if (skipCount >= numberOfSegments) {
          LastName = NoLastName;
          return;
        }

        LastName = string.Join(" ", customerNameSegments.Skip(andSegmentIndex + 2));
        return;
      }

      var h = new HumanName(CustomerName);

      if (h.IsUnparsable) {
        FirstName = CustomerName;
        LastName = NoLastName;
        return;
      }

      FirstName = string.IsNullOrWhiteSpace(h.First) ? NoFirstName : h.First;
      LastName = string.IsNullOrWhiteSpace(h.Last) ? NoLastName : h.Last;
    }

    private bool ContainsCompanyIdentifier(string customerName) {
      string pattern = @"\b(Pty|Ltd|EDMS|BPK|CC|BK|Shop|Liquors|Restaurant|Winery|Dienste|GmbH|Cellars|Kitchen|T/A|C/O|Boutique])\b|\b(\d)\b";
      Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
      return r.IsMatch(customerName);
    }
  }
}