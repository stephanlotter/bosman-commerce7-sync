/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using System.Text.RegularExpressions;
using BosmanCommerce7.Module.Extensions;

namespace BosmanCommerce7.Blazor.Server.Extensions {
  public static class LoggerInitFunctions {
    public static Logger Init(IServiceProvider serviceProvider) {

      var pattern =
        """
^Placing grain
|^Created .* with GrainContext
|^Activating grain
|^Finished activating grain
|^InitActivation is done
|^Task \[Id=\d*, Status=RanToCompletion\]
|^Found address
|^Received status update for pending request, Request
|^After collection
|^ProcessTableUpdate
|^Creating.*status update with diagnostics
|^Producing instance of Job
|^Batch acquisition of .* triggers
|^Trigger instruction : NoInstruction
|^Calling Execute on job JobsGroup
""";

      var regEx = new Regex(pattern.Replace("\r", "").Replace("\n", ""));

      var options = ServiceProviderFunctions.GetBosmanCommerce7Options(serviceProvider);
      var logFile = Path.Combine(options.InAppDataFolder("logs"), "log.txt");

      const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}";

      var logger = new LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Error)
        .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate: outputTemplate)
        .WriteTo.File(logFile, rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate)
        .Enrich.With(new SourceContextEnricher())
        .Filter.ByExcluding(l => {
          return regEx.IsMatch(l.MessageTemplate.Text);
        });

      if (options?.EnableDebugLoggingLevel ?? false) {
        logger.MinimumLevel.Debug();
      }
      else {
        logger.MinimumLevel.Information();
      }

      return logger.CreateLogger();
    }

    public class SourceContextEnricher : ILogEventEnricher {

      public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory) {
        if (logEvent.Properties.TryGetValue("SourceContext", out var property)) {
          var scalarValue = property as ScalarValue;
          var value = scalarValue?.Value as string;

          if (value?.StartsWith("BosmanCommerce7.") ?? false) {
            var lastElement = value.Split(".").LastOrDefault();
            if (!string.IsNullOrWhiteSpace(lastElement)) {
              logEvent.AddOrUpdateProperty(new LogEventProperty("SourceContext", new ScalarValue(lastElement)));
            }
          }

        }
      }
    }

  }
}
