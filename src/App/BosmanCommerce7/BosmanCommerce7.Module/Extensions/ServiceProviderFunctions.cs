/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BosmanCommerce7.Module.Extensions {
  public static class ServiceProviderFunctions {

    public static ApplicationOptions GetApplicationOptions(IServiceProvider serviceProvider) {
      var service = serviceProvider.GetService<ApplicationOptions>() ?? throw new Exception($"{nameof(ApplicationOptions)} not defined in appsettings.json.");
      return service;
    }

  }
}
