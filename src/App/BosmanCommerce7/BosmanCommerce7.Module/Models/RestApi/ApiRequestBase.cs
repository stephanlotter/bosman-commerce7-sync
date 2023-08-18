/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using RestSharp;

namespace BosmanCommerce7.Module.Models.RestApi {

  public abstract record ApiRequestBase {

    public virtual object? Data { get; }

    public string Resource { get; init; } = default!;

    public Method Method { get; init; } = Method.Post;

    public bool DeserializeBody { get; init; } = true;

  }

}
