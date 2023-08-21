/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.AppDataServices;
using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices;
using BosmanCommerce7.Module.ApplicationServices.RestApiClients;
using BosmanCommerce7.Module.Models;

namespace BosmanCommerce7.Blazor.Server.Extensions {
  public static class RegisterServicesFunctions {

    public static void RegisterServices(IServiceCollection services) {
      RegisterApiServices(services);
      RegisterLocalDatabaseServices(services);
      RegisterEvolutionServices(services);
      RegisterUtilityServices(services);
      RegisterSalesOrderServices(services);
      RegisterSalesOrderSyncServices(services);
      RegisterSalesOrderPostServices(services);
    }

    private static void RegisterLocalDatabaseServices(IServiceCollection services) {
      services.AddTransient<ILocalObjectSpaceProvider, LocalObjectSpaceProvider>();
    }

    private static void RegisterApiServices(IServiceCollection services) {
      services.AddTransient<IRestClientFactory, RestClientFactory>();
      services.AddTransient<IApiClientService, ApiClientService>();

      services.AddTransient<ISalesOrdersApiClient, SalesOrdersApiClient>();
    }

    private static void RegisterEvolutionServices(IServiceCollection services) {
      //services.AddTransient<IEvolutionInventoryRepository, EvolutionInventoryRepository>();
    }

    private static void RegisterUtilityServices(IServiceCollection services) {
      services.AddTransient<IAppDataFileManager, AppDataFileManager>();
      services.AddTransient<IValueStoreRepository, ValueStoreRepository>();
    }

    private static void RegisterSalesOrderServices(IServiceCollection services) {
      services.AddTransient<ISalesOrdersLocalRepository, SalesOrdersLocalRepository>();

    }

    private static void RegisterSalesOrderSyncServices(IServiceCollection services) {
      services.AddTransient<ISalesOrdersSyncQueueService, SalesOrdersSyncQueueService>();
      services.AddTransient<ISalesOrdersSyncService, SalesOrdersSyncService>();
      services.AddTransient<ISalesOrdersSyncValueStoreService, SalesOrdersSyncValueStoreService>();
    }

    private static void RegisterSalesOrderPostServices(IServiceCollection services) {
      services.AddTransient<ISalesOrdersPostQueueService, SalesOrdersPostQueueService>();
      services.AddTransient<ISalesOrdersPostService, SalesOrdersPostService>();
    }

    public static void RegisterConfig(IServiceCollection services, IConfiguration configuration) {

      void AddConfig(string sectionName, object instance) {
        configuration.GetSection(sectionName).Bind(instance);
        services.AddSingleton(instance.GetType(), instance);
      }

      var options = new ApplicationOptions();

      AddConfig("ApplicationOptions", options);

      services.AddSingleton<ILocalDatabaseConnectionStringProvider>(options.ConnectionStrings);
      services.AddSingleton<IEvolutionDatabaseConnectionStringProvider>(options.ConnectionStrings);
      services.AddSingleton(options.SalesOrdersSyncJobOptions);
      services.AddSingleton(options.SalesOrdersPostJobOptions);
      services.AddSingleton(options.ApiOptions);

    }

  }

}
