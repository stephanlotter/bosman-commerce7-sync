/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models.RestApi;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.RestApiClients {

  public abstract class ApiClientBase {
    private readonly IApiClientService _apiClientService;

    protected ILogger Logger { get; }

    public ApiClientBase(ILogger logger, IApiClientService apiClientService) {
      Logger = logger;
      _apiClientService = apiClientService;
    }

    protected Result<ResponseBase> SendSingleResultRequest(ApiRequestBase apiRequest, Func<ResponseBase> apiResponseFactory) {
      ResponseBase apiResponse = apiResponseFactory();
      var list = new List<dynamic>();

      return SendRequest(apiRequest, data => {
        if (data == null) {
          return (Result.Failure<ResponseBase>("Response body not valid JSON."), ApiRequestPaginationStatus.Completed);
        }

        list.Add(data);

        apiResponse = apiResponse with { Data = list.ToArray() };

        return (Result.Success(apiResponse), ApiRequestPaginationStatus.Completed);
      });
    }

    protected Result<ResponseBase> SendListResultRequest(ApiRequestBase apiRequest, Func<ResponseBase> apiResponseFactory, string responseListPropertyName) {
      ResponseBase apiResponse = apiResponseFactory();
      var list = new List<dynamic>();

      return SendRequest(apiRequest, data => {
        if (data == null) {
          return (Result.Failure<ResponseBase>("Response body not valid JSON."), ApiRequestPaginationStatus.Completed);
        }

        var totalRecords = (int)data!.total;

        foreach (var order in data![responseListPropertyName]) { list.Add(order); }

        apiResponse = apiResponse with { Data = list.ToArray() };

        return (Result.Success(apiResponse), list.Count < totalRecords ? ApiRequestPaginationStatus.MorePages : ApiRequestPaginationStatus.Completed);
      });
    }

    protected Result<T> SendRequest<T>(ApiRequestBase apiRequest, Func<dynamic, (Result<T> result, ApiRequestPaginationStatus paginationStatus)> onSuccess, Action<RestRequest>? configRestRequest = null) {
      while (true) {
        if (apiRequest.IsPagedResponse) {
          apiRequest = apiRequest with { CurrentPage = apiRequest.CurrentPage + 1 };
        }

        var response = _apiClientService.Execute(apiRequest, configRestRequest: configRestRequest);

        if (response is ApiResponseBase.Failure f) {
          Logger.LogError("{error}", f.FullErrorMessage);
          return Result.Failure<T>(f.FullErrorMessage);
        }

        var successResponse = response as ApiResponseBase.Success;

        if (apiRequest.DeserializeBody && IsResponseBodyEmpty(successResponse!)) { return Result.Failure<T>("Response body empty."); }

        dynamic data = new { };

        if (apiRequest.DeserializeBody) {
          var dataResult = Deserialize<dynamic>(successResponse!);
          if (dataResult.IsFailure) { return Result.Failure<T>(dataResult.Error); }
          data = dataResult.Value;
        }

        (Result<T> result, ApiRequestPaginationStatus paginationStatus) r = onSuccess(data);

        if (r.result.IsFailure || !apiRequest.IsPagedResponse || r.paginationStatus == ApiRequestPaginationStatus.Completed) { return r.result; }
      }
    }

    protected bool IsResponseBodyEmpty(ApiResponseBase responseBase) {
      return string.IsNullOrWhiteSpace(responseBase.ResonseBody);
    }

    protected Result<T> Deserialize<T>(Result<ApiResponseBase.Success> response) {
      try {
        var value = response.Value;
        var data = JsonFunctions.Deserialize<T>(value.ResonseBody ?? "");

        if (data == null) {
          Logger.LogError("Response body not valid JSON.\r\n{body}", value?.ResonseBody);
          return Result.Failure<T>("Response body not valid JSON.");
        }

        return Result.Success(data);
      }
      catch (Exception ex) {
        Logger.LogError(ex, "While deserializing response");
        return Result.Failure<T>(ex.Message);
      }
    }
  }
}