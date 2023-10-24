/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Blazor.Server.Extensions;
using BosmanCommerce7.Blazor.Server.Services;
using BosmanCommerce7.Module.Extensions;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BosmanCommerce7.Blazor.Server;

public class Startup {

  public Startup(IConfiguration configuration) {
    Configuration = configuration;
  }

  public IConfiguration Configuration { get; }

  // This method gets called by the runtime. Use this method to add services to the container.
  // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
  public void ConfigureServices(IServiceCollection services) {
    services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));

    RegisterServicesFunctions.RegisterConfig(services, Configuration);
    RegisterServicesFunctions.RegisterServices(services);

    services.AddRazorPages();
    services.AddServerSideBlazor();
    services.AddHttpContextAccessor();
    services.AddScoped<CircuitHandler, CircuitHandlerProxy>();

    services.AddXaf(Configuration, builder => {
      builder.UseApplication<BosmanCommerce7BlazorApplication>();

      builder.Modules
          .AddConditionalAppearance()
          .AddViewVariants()
          .AddValidation(options => {
            options.AllowValidationDetailsAccess = false;
          })
          .Add<Module.BosmanCommerce7Module>()
          .Add<BosmanCommerce7BlazorModule>();

      builder.ObjectSpaceProviders
          .AddXpo((serviceProvider, options) => {
            var o = ServiceProviderFunctions.GetApplicationOptions(serviceProvider);
            string? connectionString = o.ConnectionStrings.LocalDatabase;

            ArgumentNullException.ThrowIfNull(connectionString);
            options.ConnectionString = connectionString;
            options.ThreadSafe = true;
            options.UseSharedDataStoreProvider = true;
          })
          .AddNonPersistent();
    });
  }

  // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
    if (env.IsDevelopment()) {
      app.UseDeveloperExceptionPage();
    }
    else {
      app.UseExceptionHandler("/Error");
      // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
      //app.UseHsts();
    }

    //app.UseHttpsRedirection();

    app.UseRequestLocalization();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseXaf();

    app.UseEndpoints(endpoints => {
      endpoints.MapXafEndpoints();
      endpoints.MapBlazorHub();
      endpoints.MapFallbackToPage("/_Host");
      endpoints.MapControllers();
    });
  }
}