/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using RestSharp;

namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients {

  public class Commerce7RestClient : ICommerce7RestClient {
    private readonly Commerce7ApiOptions _apiOptions;

    public Commerce7RestClient(Commerce7ApiOptions apiOptions) {
      _apiOptions = apiOptions;

    }

    public Result<RestClient> NewRestClient(Action<RestClientOptions>? configOptions = null) {
      if (_apiOptions == null) { return Result.Failure<RestClient>("Commerce7ApiOptions cannot be empty. Please define a Commerce7ApiOptions section in the appsettings.json file under BosmanCommerce7Options"); }

      if (string.IsNullOrWhiteSpace(_apiOptions.Endpoint)) { return Result.Failure<RestClient>("Endpoint cannot be empty. Please define the Endpoint in the appsettings.json file under BosmanCommerce7Options.Commerce7ApiOptions.Endpoint"); }
      if (string.IsNullOrWhiteSpace(_apiOptions.ClientId)) { return Result.Failure<RestClient>("ClientId cannot be empty. Please define the ClientId in the appsettings.json file under BosmanCommerce7Options.Commerce7ApiOptions.ClientId"); }
      if (string.IsNullOrWhiteSpace(_apiOptions.ClientSecret)) { return Result.Failure<RestClient>("ClientSecret cannot be empty. Please define the ClientSecret in the appsettings.json file under BosmanCommerce7Options.Commerce7ApiOptions.ClientSecret"); }

      var options = new RestClientOptions(_apiOptions.Endpoint!) { };

      configOptions?.Invoke(options);

      var client = new RestClient(options) { };

      return Result.Success(client);
    }

    public Result<RestClient> SetAuthorization(RestClient client, string token) {
      if (string.IsNullOrWhiteSpace(token)) { return Result.Failure<RestClient>("ApiToken cannot be empty. First do an AUTH call to get a token."); }
      client.AddDefaultHeader("Authorization", $"{token.Trim()}");
      return Result.Success(client);
    }

  }

}
