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
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Customers;
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Inventory;
using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.RestApi;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryStockLevelsSyncServices.RestApi;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.RestApiClients;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.RestApi;
using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.EvolutionSdk;

namespace BosmanCommerce7.Blazor.Server.Extensions {

  public static class RegisterServicesFunctions {

    public static void RegisterServices(IServiceCollection services) {
      RegisterApiServices(services);

      RegisterLocalDatabaseServices(services);

      RegisterEvolutionServices(services);

      RegisterUtilityServices(services);

      RegisterCustomerMasterSyncServices(services);
      RegisterInventoryMasterSyncServices(services);
      RegisterInventoryLevelsSyncServices(services);

      RegisterSalesOrderSyncServices(services);
      RegisterSalesOrderPostServices(services);
    }

    private static void RegisterLocalDatabaseServices(IServiceCollection services) {
      services.AddTransient<ILocalObjectSpaceProvider, LocalObjectSpaceProvider>();
      services.AddTransient<ILocalObjectSpaceEvolutionSdkProvider, LocalObjectSpaceEvolutionSdkProvider>();
    }

    private static void RegisterApiServices(IServiceCollection services) {
      services.AddTransient<IRestClientFactory, RestClientFactory>();
      services.AddTransient<IApiClientService, ApiClientService>();

      services.AddTransient<ICustomerMasterApiClient, CustomerMasterApiClient>();

      services.AddTransient<IInventoryItemApiClient, InventoryItemApiClient>();
      services.AddTransient<IInventoryLevelsApiClient, InventoryLevelsApiClient>();

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

    private static void RegisterCustomerMasterSyncServices(IServiceCollection services) {
      services.AddTransient<ICustomerMasterSyncQueueService, CustomerMasterSyncQueueService>();
      services.AddTransient<ICustomerMasterSyncService, CustomerMasterSyncService>();
      services.AddTransient<ICustomerMasterLocalMappingService, CustomerMasterLocalMappingService>();
    }

    private static void RegisterInventoryMasterSyncServices(IServiceCollection services) {
      services.AddTransient<IInventoryItemsSyncQueueService, InventoryItemsSyncQueueService>();
      services.AddTransient<IInventoryItemsSyncService, InventoryItemsSyncService>();
      services.AddTransient<IInventoryItemsLocalMappingService, InventoryItemsLocalMappingService>();
      services.AddTransient<IInventoryItemsLocalCache, InventoryItemsLocalCache>();
    }

    private static void RegisterInventoryLevelsSyncServices(IServiceCollection services) {
      services.AddTransient<IInventoryLevelsSyncQueueService, InventoryLevelsSyncQueueService>();
      services.AddTransient<IInventoryLevelsSyncService, InventoryLevelsSyncService>();
      services.AddTransient<IInventoryLevelsLocalMappingService, InventoryLevelsLocalMappingService>();
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
      services.AddSingleton(options.CustomerMasterSyncJobOptions);
      services.AddSingleton(options.InventoryItemsSyncJobOptions);
      services.AddSingleton(options.InventoryLevelsSyncJobOptions);
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