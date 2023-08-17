/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using Newtonsoft.Json;

namespace BosmanCommerce7.Module.Models.RestApi.Authentication {
  public record Commerce7AuthenticateApiResponse {

    [JsonProperty("access_token")]
    public string? AccessToken { get; init; }

    [JsonProperty("token_type")]
    public string? TokenType { get; init; }

    [JsonProperty("expires_in")]
    public int? ExpiresInSeconds { get; init; }

  }

}
