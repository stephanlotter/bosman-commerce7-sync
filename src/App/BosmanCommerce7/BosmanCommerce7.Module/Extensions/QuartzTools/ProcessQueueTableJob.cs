/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using Quartz;
using Serilog;

namespace BosmanCommerce7.Module.Extensions.QuartzTools {
  [DisallowConcurrentExecution]
  public class ProcessQueueTableJob : IJob {

    public async Task Execute(IJobExecutionContext context) {
      var name = context.JobDetail.Key.Name;
      try {
        if (context.JobDetail.JobDataMap.Get(JobIds.JobServiceInstance) is not ISyncQueueService syncQueueService) {
          Log.Logger.Warning($"Service {nameof(ISyncQueueService)} not found in this Job's context.");
          await Task.CompletedTask;
        }
        else {
          await syncQueueService.Execute();
        }
      }
      catch (Exception ex) {
        throw new JobExecutionException(ex);
      }
    }
  }

}
