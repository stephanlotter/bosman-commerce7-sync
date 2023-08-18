/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using System.Reflection;
using BosmanCommerce7.Blazor.Server.Extensions;
using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Extensions.QuartzTools;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.DesignTime;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.Utils;
using Serilog;

namespace BosmanCommerce7.Blazor.Server;

public class Program : IDesignTimeApplicationFactory {
  private static bool ContainsArgument(string[] args, string argument) {
    return args.Any(arg => arg.TrimStart('/').TrimStart('-').ToLower() == argument.ToLower());
  }

  public static int Main(string[] args) {

    if (ContainsArgument(args, "help") || ContainsArgument(args, "h")) {
      Console.WriteLine("Updates the database when its version does not match the application's version.");
      Console.WriteLine();
      Console.WriteLine($"    {Assembly.GetExecutingAssembly().GetName().Name}.exe --updateDatabase [--forceUpdate --silent]");
      Console.WriteLine();
      Console.WriteLine("--forceUpdate - Marks that the database must be updated whether its version matches the application's version or not.");
      Console.WriteLine("--silent - Marks that database update proceeds automatically and does not require any interaction with the user.");
      Console.WriteLine();
      Console.WriteLine($"Exit codes: 0 - {DBUpdaterStatus.UpdateCompleted}");
      Console.WriteLine($"            1 - {DBUpdaterStatus.UpdateError}");
      Console.WriteLine($"            2 - {DBUpdaterStatus.UpdateNotNeeded}");
    }
    else {
      FrameworkSettings.DefaultSettingsCompatibilityMode = FrameworkSettingsCompatibilityMode.Latest;

      IHost host = CreateHostBuilder(args).Build();

      if (ContainsArgument(args, "updateDatabase")) {
        using var serviceScope = host.Services.CreateScope();
        return serviceScope.ServiceProvider.GetRequiredService<IDBUpdater>().Update(ContainsArgument(args, "forceUpdate"), ContainsArgument(args, "silent"));
      }
      else {
        ServiceScopeFunctions.SetServiceProvider(host.Services);

        ServiceScopeFunctions.WrapInScope(serviceScope => {
          InitLogging(serviceScope.ServiceProvider);
          StartHostedServices(serviceScope.ServiceProvider);
        });

        Log.Information("App running");

        host.Run();
      }
    }
    return 0;
  }

  public static IHostBuilder CreateHostBuilder(string[] args) {
    var hostBuilder = Host
      .CreateDefaultBuilder(args)
      .ConfigureWebHostDefaults(webBuilder => {
        webBuilder.UseStartup<Startup>();
      });

    hostBuilder.UseSerilog();

    return hostBuilder;
  }

  XafApplication IDesignTimeApplicationFactory.Create() {
    IHostBuilder hostBuilder = CreateHostBuilder(Array.Empty<string>());
    return DesignTimeApplicationFactoryHelper.Create(hostBuilder);
  }

  
  private static void InitLogging(IServiceProvider serviceProvider) {
    Log.Logger = LoggerInitFunctions.Init(serviceProvider);
  }

  private static void StartHostedServices(IServiceProvider serviceProvider) {
    var options = ServiceProviderFunctions.GetApplicationOptions(serviceProvider);
    var logger = serviceProvider.GetService<ILogger<Program>>();

    QuartzApplicationFunctions.StartJobs(new QuartzStartJobContext {
      Logger = logger!,
      Options = options,
      ServiceProvider = serviceProvider
    });
  }
}
