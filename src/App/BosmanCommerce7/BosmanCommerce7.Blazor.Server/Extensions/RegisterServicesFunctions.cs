/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.AppDataServices;
using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices;
using BosmanCommerce7.Module.ApplicationServices.RestApiClients;
using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.EvolutionSdk;

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
      services.AddSingleton<IEvolutionCompanyDescriptor, EvolutionCompanyDescriptor>(serviceProvider => Instance(serviceProvider));
      services.AddTransient<IEvolutionSdk, EvolutionSdk>();
      services.AddTransient<IPostToEvolutionSalesOrderService, PostToEvolutionSalesOrderService>();
      services.AddTransient<IEvolutionCustomerRepository, EvolutionCustomerRepository>();
      services.AddTransient<IEvolutionProjectRepository, EvolutionProjectRepository>();
      services.AddTransient<IEvolutionSalesRepresentativeRepository, EvolutionSalesRepresentativeRepository>();
      services.AddTransient<IEvolutionDeliveryMethodRepository, EvolutionDeliveryMethodRepository>();
      services.AddTransient<IEvolutionInventoryItemRepository, EvolutionInventoryItemRepository>();
      services.AddTransient<IEvolutionWarehouseRepository, EvolutionWarehouseRepository>();
      services.AddTransient<IEvolutionPriceListRepository, EvolutionPriceListRepository>();
      services.AddTransient<IEvolutionGeneralLedgerAccountRepository, EvolutionGeneralLedgerAccountRepository>();
    }

    private static void RegisterUtilityServices(IServiceCollection services) {
      services.AddTransient<IAppDataFileManager, AppDataFileManager>();
      services.AddTransient<IValueStoreRepository, ValueStoreRepository>();
      services.AddTransient<IWarehouseRepository, WarehouseRepository>();
      services.AddTransient<IBundleMappingRepository, BundleMappingRepository>();
    }

    private static void RegisterSalesOrderServices(IServiceCollection services) {
    }

    private static void RegisterSalesOrderSyncServices(IServiceCollection services) {
      services.AddTransient<ISalesOrdersSyncQueueService, SalesOrdersSyncQueueService>();
      services.AddTransient<ISalesOrdersSyncService, SalesOrdersSyncService>();
      services.AddTransient<ISalesOrdersSyncValueStoreService, SalesOrdersSyncValueStoreService>();
    }

    private static void RegisterSalesOrderPostServices(IServiceCollection services) {
      services.AddTransient<ISalesOrdersPostQueueService, SalesOrdersPostQueueService>();
      services.AddTransient<ISalesOrdersPostService, SalesOrdersPostService>();
      services.AddTransient<ISalesOrdersPostValueStoreService, SalesOrdersPostValueStoreService>();
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

    private static EvolutionCompanyDescriptor Instance(IServiceProvider serviceProvider) {
      var options = serviceProvider.GetService<IEvolutionDatabaseConnectionStringProvider>();
      var companyDatabaseConnectionString = options!.EvolutionCompany;
      var commonDatabaseConnectionString = options!.EvolutionCommon;

      if (string.IsNullOrWhiteSpace(companyDatabaseConnectionString)) { throw new Exception("EvolutionCompanyConnectionString is null or empty"); }
      if (string.IsNullOrWhiteSpace(commonDatabaseConnectionString)) { throw new Exception("CommonDatabaseConnectionString is null or empty"); }

      return new EvolutionCompanyDescriptor(companyDatabaseConnectionString, commonDatabaseConnectionString);
    }
  }
}