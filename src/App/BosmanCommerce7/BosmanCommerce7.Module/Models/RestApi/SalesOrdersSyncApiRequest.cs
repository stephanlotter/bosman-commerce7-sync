/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-18
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using RestSharp;

namespace BosmanCommerce7.Module.Models.RestApi {
  public record SalesOrdersSyncApiRequest : ApiRequestBase {

    public SalesOrdersSyncApiRequest(DateTime orderSubmittedDate) {
      Resource = $"/order?orderSubmittedDate=gt:{orderSubmittedDate:yyyy-MM-dd}";
      Method = Method.Get;
      IsPagedResponse = true;
    }

  }

}
