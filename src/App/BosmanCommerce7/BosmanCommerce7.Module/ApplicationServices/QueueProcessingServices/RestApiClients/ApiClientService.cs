/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models.RestApi;
using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.RestApiClients {

  public class ApiClientService : IApiClientService {
    private readonly ILogger<ApiClientService> _logger;
    private readonly IRestClientFactory _restClient;

    public ApiClientService(ILogger<ApiClientService> logger, IRestClientFactory commerce7RestClient) {
      _logger = logger;
      _restClient = commerce7RestClient;
    }

    public ApiResponseBase Execute(ApiRequestBase apiRequest, Action<RestClientOptions>? configOptions = null, Action<RestRequest>? configRestRequest = null) {
      var requestResource = apiRequest.GetResource();

      var json = JsonFunctions.Serialize(apiRequest.Data);
      var useJson = apiRequest.Data != null && !string.IsNullOrWhiteSpace(json) && json != "null";

      _logger.LogInformation("Send Commerce7 request {method}:{resource}{json}", apiRequest.Method, requestResource, $":\r\n{json}" ?? "");

      try {
        var result = _restClient.NewRestClient(configOptions)
          .Bind(client => {
            var method = apiRequest.Method;
            var request = new RestRequest(requestResource, method);

            if (useJson) { request.AddJsonBody(json!); }

            configRestRequest?.Invoke(request);

            var response = client.Execute(request);

            _logger.LogDebug("{0} Commerce7 response status code {1}", requestResource, response.StatusCode);

            if (!response.IsSuccessful) {
              return Result.Success((ApiResponseBase)new ApiResponseBase.Failure {
                Uri = response.ResponseUri?.AbsoluteUri ?? "",
                ErrorMessage = response.ErrorMessage,
                ErrorException = response.ErrorException,
                ResonseBody = response.Content,
                StatusCode = $"{response.StatusCode}",
                StatusDescription = response.StatusDescription
              });
            }

            var responseIsBinaryData = response.ContentType?.Equals("image/jpeg", StringComparison.InvariantCultureIgnoreCase) ?? false;

            return Result.Success((ApiResponseBase)new ApiResponseBase.Success {
              ResonseBody = responseIsBinaryData ? null : response.Content,
              ResponseRawBytes = responseIsBinaryData ? response.RawBytes : null
            });
          });

        return result.IsSuccess ? result.Value : ReturnFailure(result);
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While sending HTTP request: {resource}", requestResource);
        return new ApiResponseBase.Failure { ErrorException = ex };
      }

      ApiResponseBase ReturnFailure(Result<ApiResponseBase> result) {
        _logger.LogError("While sending HTTP request: {resource}", requestResource);
        _logger.LogError("{error}", result.Error);
        return new ApiResponseBase.Failure { ErrorMessage = result.Error, StatusCode = "Error" };
      }
    }
  }
}