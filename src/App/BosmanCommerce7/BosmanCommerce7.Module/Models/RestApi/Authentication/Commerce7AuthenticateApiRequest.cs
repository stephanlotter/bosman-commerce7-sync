/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using RestSharp;

namespace BosmanCommerce7.Module.Models.RestApi.Authentication {

  public record Commerce7AuthenticateApiRequest : Commerce7ApiRequestBase {

    public Commerce7AuthenticateApiRequest() {
      Resource = "/connect/token";
      Method = Method.Post;
    }

  }

}
