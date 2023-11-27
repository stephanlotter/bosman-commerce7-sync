/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-27
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using Quartz;
using Serilog;

namespace BosmanCommerce7.Module.Extensions.QuartzTools {

  public static class TriggerFunctions {

    public static void PrintNextRun(this ITrigger trigger) {
      var next = trigger.GetNextFireTimeUtc();
      if (next != null) {
        Log.Logger.Debug("Next run {time}", $"{next.Value.ToLocalTime():o}");
      }
    }
  }
}