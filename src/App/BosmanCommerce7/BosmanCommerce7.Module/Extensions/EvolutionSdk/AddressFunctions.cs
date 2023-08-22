/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using Pastel.Evolution;
using Quartz.Util;

namespace BosmanCommerce7.Module.Extensions.EvolutionSdk {
  public static class AddressFunctions {

    public static int FindFirstEmptyLine(this Address address) =>
      address.AddressLines()
             .Where(addressLine => addressLine.addressLine.IsNullOrWhiteSpace())
             .Select(addressLine => addressLine.lineNumber)
             .FirstOrDefault();

    public static string GetLineNumberValue(this Address address, int lineNumber) =>
      address.AddressLines()
             .Where(addressLine => addressLine.lineNumber == lineNumber)
             .Select(addressLine => addressLine.addressLine)
             .FirstOrDefault() ?? "";

    private static IEnumerable<(int lineNumber, string addressLine)> AddressLines(this Address address) {
      yield return (1, address.Line1);
      yield return (2, address.Line2);
      yield return (3, address.Line3);
      yield return (4, address.Line4);
      yield return (5, address.Line5);
      //Do not process postal code. yield return (6, address.PostalCode);
    }

    private static Address SetAddressLineValue(this Address address, int lineNumber, string newValue) {
      if (lineNumber == 1) {
        address.Line1 = newValue;
      }
      else if (lineNumber == 2) {
        address.Line2 = newValue;
      }
      else if (lineNumber == 3) {
        address.Line3 = newValue;
      }
      else if (lineNumber == 4) {
        address.Line4 = newValue;
      }
      else if (lineNumber == 5) {
        address.Line5 = newValue;
      }

      return address;
    }

    private static Address CloneAddress(this Address address) =>
      new() {
        Line1 = address.Line1,
        Line2 = address.Line2,
        Line3 = address.Line3,
        Line4 = address.Line4,
        Line5 = address.Line5,
        PostalCode = address.PostalCode
      };

    public static bool HasEmptySpace(this Address address) => address.FindFirstEmptyLine() > 0;

    public static Address WriteToFirstEmptySpace(this Address address, string value) {
      var firstEmptyLine = address.FindFirstEmptyLine();
      return firstEmptyLine == 0 ? address : address.SetAddressLineValue(firstEmptyLine, value);
    }

    public static Address MoveEmptyLineToBottom(this Address address) {
      var search = true;
      while (search) {
        search = false;
        var hasEmptyLine = false;
        foreach (var addressLine in address.AddressLines()) {
          if (!hasEmptyLine && addressLine.addressLine.IsNullOrWhiteSpace()) {
            hasEmptyLine = true;
          }

          if (!hasEmptyLine || addressLine.addressLine.IsNullOrWhiteSpace()) {
            continue;
          }

          address = address.SwapLines(addressLine.lineNumber, addressLine.lineNumber - 1);
          search = true;
        }
      }
      return address;
    }

    public static Address TryAddValueToTop(this Address address, string value) {
      if (!address.HasEmptySpace()) { return address; }

      return address
        .MakeSpaceAtTop()
        .SetAddressLineValue(1, value);
    }

    public static Address MakeSpaceAtTop(this Address address) => address.Line1.IsNullOrWhiteSpace() ? address : address.MoveLinesDown(1);

    public static Address SwapLines(this Address address, int line1, int line2) {
      var hold = address.GetLineNumberValue(line1);
      return address.SetAddressLineValue(line1, address.GetLineNumberValue(line2)).SetAddressLineValue(line2, hold);
    }

    public static Address MoveLinesDown(this Address address, int fromLineNumber) {
      var firstEmptyLine = address.FindFirstEmptyLine();
      if (firstEmptyLine == 0) { return address; }

      var newAddress = address.CloneAddress();
      for (var i = firstEmptyLine; i > fromLineNumber; i--) {
        newAddress = newAddress.SetAddressLineValue(i, address.GetLineNumberValue(i - 1));
      }
      newAddress = newAddress.SetAddressLineValue(fromLineNumber, "");
      newAddress.PostalCode = address.PostalCode;
      return newAddress;
    }

  }
}
