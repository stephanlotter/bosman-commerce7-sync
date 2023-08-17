/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using Quartz;
using Quartz.Impl;

namespace BosmanCommerce7.Module.Extensions.QuartzTools {
  public static class QuartzFunctions {

    private static IScheduler? _scheduler;

    internal static IScheduler GetIScheduler() => (IScheduler)GetScheduler();

    public static object GetScheduler() => _scheduler ??= CreateScheduler();

    public static void StartScheduler() {
      var scheduler = GetIScheduler();
      if (scheduler.IsStarted) { return; }
      scheduler.Start();
    }

    public static void ShutdownScheduler() {
      var scheduler = GetIScheduler();
      if (scheduler.IsShutdown) { return; }
      scheduler.Shutdown();
    }

    public static void StopTrigger(ITrigger trigger) {
      var scheduler = GetIScheduler();
      scheduler.UnscheduleJob(trigger.Key);
    }

    internal static IScheduler CreateScheduler() {
      var factory = new StdSchedulerFactory();
      return factory.GetScheduler().Result;
    }
  }
}
