﻿/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models.RestApi;
using RestSharp;

namespace BosmanCommerce7.Module.ApplicationServices.RestApiClients {
  public interface ICommerce7RestClientService {

    Commerce7ApiResponseBase Execute(Commerce7ApiRequestBase apiRequest, Action<RestClientOptions>? configOptions = null, Action<RestRequest>? configRestRequest = null);

  }

}
