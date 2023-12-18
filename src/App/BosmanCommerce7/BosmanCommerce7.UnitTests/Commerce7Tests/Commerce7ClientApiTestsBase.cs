/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-18
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.RestApiClients;
using BosmanCommerce7.Module.Models;

namespace BosmanCommerce7.UnitTests.Commerce7Tests {

  public abstract class Commerce7ClientApiTestsBase {

    protected ApiClientService ApiClientService { get; }

    public Commerce7ClientApiTestsBase() {
      var apiOptions = new ApiOptions {
        AppId = "evolution-integration",
        AppSecretKey = Environment.GetEnvironmentVariable("ApplicationOptions__ApiOptions__AppSecretKey", EnvironmentVariableTarget.Machine),
        Endpoint = "https://api.commerce7.com/v1",
        TenantId = "neurasoft-sandbox"
      };

      var restClientFactory = new RestClientFactory(apiOptions);
      ApiClientService = new ApiClientService(A.Fake<ILogger<ApiClientService>>(), restClientFactory);
    }
  }
}