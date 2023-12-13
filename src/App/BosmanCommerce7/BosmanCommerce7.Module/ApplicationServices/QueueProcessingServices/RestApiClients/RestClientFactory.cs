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
using RestSharp.Authenticators;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.RestApiClients
{

    public class RestClientFactory : IRestClientFactory
    {
        private readonly ApiOptions _apiOptions;

        public RestClientFactory(ApiOptions apiOptions)
        {
            _apiOptions = apiOptions;

        }

        public Result<RestClient> NewRestClient(Action<RestClientOptions>? configOptions = null)
        {
            if (_apiOptions == null) { return Result.Failure<RestClient>("ApiOptions cannot be empty. Please define a ApiOptions section in the appsettings.json file under ApplicationOptions"); }

            if (string.IsNullOrWhiteSpace(_apiOptions.Endpoint)) { return Result.Failure<RestClient>("Endpoint cannot be empty. Please define the Endpoint in the appsettings.json file under ApplicationOptions.ApiOptions.Endpoint"); }
            if (string.IsNullOrWhiteSpace(_apiOptions.TenantId)) { return Result.Failure<RestClient>("TenantId cannot be empty. Please define the TenantId in the appsettings.json file under ApplicationOptions.ApiOptions.TenantId"); }
            if (string.IsNullOrWhiteSpace(_apiOptions.AppId)) { return Result.Failure<RestClient>("AppId cannot be empty. Please define the AppId in the appsettings.json file under ApplicationOptions.ApiOptions.AppId"); }
            if (string.IsNullOrWhiteSpace(_apiOptions.AppSecretKey)) { return Result.Failure<RestClient>("AppSecretKey cannot be empty. Please define the AppSecretKey in the appsettings.json file under ApplicationOptions.ApiOptions.AppSecretKey"); }

            var options = new RestClientOptions(_apiOptions.Endpoint!)
            {
                Authenticator = new HttpBasicAuthenticator(_apiOptions.AppId!, _apiOptions.AppSecretKey!)
            };

            configOptions?.Invoke(options);

            var client = new RestClient(options) { };

            client.AddDefaultHeader("tenant", $"{_apiOptions.TenantId.Trim()}");

            return Result.Success(client);
        }

        public Result<RestClient> SetAuthorization(RestClient client, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) { return Result.Failure<RestClient>("ApiToken cannot be empty. First do an AUTH call to get a token."); }
            client.AddDefaultHeader("Authorization", $"{token.Trim()}");
            return Result.Success(client);
        }

    }

}
