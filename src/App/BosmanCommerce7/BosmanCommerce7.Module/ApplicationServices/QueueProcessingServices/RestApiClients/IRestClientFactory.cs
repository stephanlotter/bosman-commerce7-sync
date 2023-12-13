/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using CSharpFunctionalExtensions;
using RestSharp;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.RestApiClients
{
    public interface IRestClientFactory
    {

        Result<RestClient> NewRestClient(Action<RestClientOptions>? configOptions = null);

        Result<RestClient> SetAuthorization(RestClient client, string token);

    }

}
