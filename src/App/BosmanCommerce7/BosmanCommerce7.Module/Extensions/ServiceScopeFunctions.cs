/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using Microsoft.Extensions.DependencyInjection;

namespace BosmanCommerce7.Module.Extensions {

  public static class ServiceScopeFunctions {

    private static IServiceProvider? _serviceProvider;

    public static void SetServiceProvider(IServiceProvider serviceProvider) {
      _serviceProvider = serviceProvider;
    }

    public static void WrapInScope(Action<IServiceScope> action) {
      using var serviceScope = _serviceProvider!.CreateScope();
      action(serviceScope);
    }

  }
}
