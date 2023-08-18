/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.RestApi;
using BosmanCommerce7.Module.Models.RestApi.Authentication;
using CSharpFunctionalExtensions;
using CSharpFunctionalExtensions.ValueTasks;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;

namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients
{
    public class Commerce7RestClientService : ICommerce7RestClientService {
    private readonly ILogger<Commerce7RestClientService> _logger;
    private readonly ICommerce7RestClient _commerce7RestClient;
    private readonly Commerce7ApiOptions _apiOptions;

    private bool _isAuthenticating;
    private string? _token;
    private DateTime _tokenExpiryDate = DateTime.MinValue;

    public bool MustAuthenticate => string.IsNullOrWhiteSpace(_token) || _tokenExpiryDate <= DateTime.Now;

    public Commerce7RestClientService(ILogger<Commerce7RestClientService> logger, ICommerce7RestClient commerce7RestClient, Commerce7ApiOptions apiOptions) {
      _logger = logger;
      _commerce7RestClient = commerce7RestClient;
      _apiOptions = apiOptions;
    }

    public Commerce7ApiResponseBase Execute(Commerce7ApiRequestBase apiRequest, Action<RestClientOptions>? configOptions = null, Action<RestRequest>? configRestRequest = null) {
      var json = JsonFunctions.Serialize(apiRequest.Data);
      var useJson = apiRequest.Data != null && !string.IsNullOrWhiteSpace(json) && json != "null";
      _logger.LogInformation("Send CAD+T request {method}:{resource}:\r\n{json}", apiRequest.Method, apiRequest.Resource, json ?? "");

      try {

        if (!_isAuthenticating && MustAuthenticate) {
          var authenticationResult = Authenticate();
          if (authenticationResult.IsFailure) { return new Commerce7ApiResponseBase.Failure { ErrorMessage = $"Authentication failed: {authenticationResult.Error}" }; }
        }

        var result = _commerce7RestClient.NewRestClient(configOptions)
          .Bind(client => {
            var method = apiRequest.Method;
            var requestResource = apiRequest.Resource;
            var request = new RestRequest(requestResource, method);

            if (!string.IsNullOrWhiteSpace(_token)) {
              _commerce7RestClient.SetAuthorization(client, _token);
            }

            if (useJson) { request.AddJsonBody(json!); }

            configRestRequest?.Invoke(request);

            var response = client.Execute(request);

            _logger.LogDebug("{0} CAD+T response status code {1}", apiRequest.Resource, response.StatusCode);

            if (!response.IsSuccessful) {
              return Result.Success((Commerce7ApiResponseBase)new Commerce7ApiResponseBase.Failure {
                Uri = response.ResponseUri?.AbsoluteUri ?? "",
                ErrorMessage = response.ErrorMessage,
                ErrorException = response.ErrorException,
                ResonseBody = response.Content,
                StatusCode = $"{response.StatusCode}",
                StatusDescription = response.StatusDescription
              });
            }

            var responseIsBinaryData = response.ContentType?.Equals("image/jpeg", StringComparison.InvariantCultureIgnoreCase) ?? false;

            return Result.Success((Commerce7ApiResponseBase)new Commerce7ApiResponseBase.Success {
              ResonseBody = responseIsBinaryData ? null : response.Content,
              ResponseRawBytes = responseIsBinaryData ? response.RawBytes : null
            });

          });

        return result.IsSuccess ? result.Value : ReturnFailure(result);

      }
      catch (Exception ex) {
        _logger.LogError(ex, "While sending HTTP request: {resource}", apiRequest.Resource);
        return new Commerce7ApiResponseBase.Failure { ErrorException = ex };
      }

      Commerce7ApiResponseBase ReturnFailure(Result<Commerce7ApiResponseBase> result) {
        _logger.LogError("While sending HTTP request: {resource}", apiRequest.Resource);
        _logger.LogError("{error}", result.Error);
        return new Commerce7ApiResponseBase.Failure { ErrorMessage = result.Error, StatusCode = "Error" };
      }
    }

    public Result Authenticate() {
      try {
        _isAuthenticating = true;
        _token = null;
        _tokenExpiryDate = DateTime.MinValue;

        var request = new Commerce7AuthenticateApiRequest { };
        var response = Execute(request, options => {
          options.Authenticator = new HttpBasicAuthenticator(_apiOptions.AppId!, _apiOptions.AppSecretKey!);
        },
        request => {
          request.AddParameter("application/x-www-form-urlencoded", $"grant_type=client_credentials", ParameterType.RequestBody);
        });

        if (response is Commerce7ApiResponseBase.Failure f) { return Result.Failure(f.FullErrorMessage); }

        var s = response as Commerce7ApiResponseBase.Success;

        var apiResponse = JsonFunctions.Deserialize<Commerce7AuthenticateApiResponse>(s!.ResonseBody ?? "");

        if (apiResponse == null) { return Result.Failure("Auth request's response was empty or invalid."); }
        if (string.IsNullOrWhiteSpace(apiResponse.TokenType)) { return Result.Failure("Auth request's TokenType response was empty or invalid."); }
        if (string.IsNullOrWhiteSpace(apiResponse.AccessToken)) { return Result.Failure("Auth request's AccessToken response was empty."); }

        _token = $"{apiResponse.TokenType} {apiResponse.AccessToken}";
        _tokenExpiryDate = DateTime.Now.AddSeconds(apiResponse.ExpiresInSeconds ?? 0);

        return Result.Success();
      }
      finally {
        _isAuthenticating = false;
      }
    }

  }

}
