/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using System.Text;
using BosmanCommerce7.Module.Extensions;

namespace BosmanCommerce7.Module.Models.RestApi {
  public abstract record ApiResponseBase {

    public string? ResonseBody { get; init; }

    public byte[]? ResponseRawBytes { get; init; }

    public record Success : ApiResponseBase {

    }

    public record Failure : ApiResponseBase {

      public string? Uri { get; init; }

      public string? ErrorMessage { get; init; }

      public Exception? ErrorException { get; init; }

      public string FullErrorMessage => GetFullErrorMessage();

      public bool ProductNotFound => (FullErrorMessage?.Contains("not found", StringComparison.InvariantCultureIgnoreCase) ?? false) || (FullErrorMessage?.Contains("notfound", StringComparison.InvariantCultureIgnoreCase) ?? false);

      public bool UnprocessableEntity => (FullErrorMessage?.Contains("Unprocessable Entity", StringComparison.InvariantCultureIgnoreCase) ?? false) || (FullErrorMessage?.Contains("UnprocessableEntity", StringComparison.InvariantCultureIgnoreCase) ?? false);

      public string? StatusCode { get; init; }

      public string? StatusDescription { get; init; }

      private string GetFullErrorMessage() {
        var sb = new StringBuilder();

        void AppendLine(string caption, string value) {
          if (string.IsNullOrEmpty(value)) { return; }
          sb!.AppendLine($"{caption}: {value}");
        }

        sb!.AppendLine();
        sb!.AppendLine("Commerce7ApiResponse");
        AppendLine($"Status code:", StatusCode!);
        AppendLine($"Status description", StatusDescription!);
        AppendLine($"URI", Uri!);
        AppendLine($"Error message", ErrorMessage!);
        AppendLine($"Error message", ExceptionFunctions.GetMessages(ErrorException));
        AppendLine($"Response body", ResonseBody ?? "");

        return sb.ToString();
      }

    }

  }

}
