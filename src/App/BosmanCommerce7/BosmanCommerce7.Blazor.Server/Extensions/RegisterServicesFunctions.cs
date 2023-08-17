/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.AppDataServices;
using BosmanCommerce7.Module.ApplicationServices.RestApiClients;
using BosmanCommerce7.Module.Models;

namespace BosmanCommerce7.Blazor.Server.Extensions {
  public static class RegisterServicesFunctions {

    public static void RegisterServices(IServiceCollection services) {
      RegisterApiServices(services);
      RegisterEvolutionServices(services);
      RegisterUtilityServices(services);
    }

    private static void RegisterApiServices(IServiceCollection services) {
      services.AddTransient<ICommerce7RestClient, Commerce7RestClient>();
      services.AddTransient<ICommerce7RestClientService, Commerce7RestClientService>();

      //services.AddTransient<IViewGroupRestClient, ViewGroupRestClient>();
      //services.AddTransient<IMaterialRestClient, MaterialRestClient>();
      //services.AddTransient<IItemMasterRestClient, ItemMasterRestClient>();
    }

    private static void RegisterEvolutionServices(IServiceCollection services) {
      //services.AddTransient<IEvolutionInventoryRepository, EvolutionInventoryRepository>();
    }

    private static void RegisterUtilityServices(IServiceCollection services) {
      services.AddTransient<IAppDataFileManager, AppDataFileManager>();
    }

    public static void RegisterConfig(IServiceCollection services, IConfiguration configuration) {

      void AddConfig(string sectionName, object instance) {
        configuration.GetSection(sectionName).Bind(instance);
        services.AddSingleton(instance.GetType(), instance);
      }

      var options = new BosmanCommerce7Options();

      AddConfig("BosmanCommerce7Options", options);

      services.AddSingleton<ILocalDatabaseConnectionStringProvider>(options.ConnectionStrings);
      services.AddSingleton<IEvolutionDatabaseConnectionStringProvider>(options.ConnectionStrings);
      services.AddSingleton(options.Commerce7ApiOptions);

    }

  }

}
