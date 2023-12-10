/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices;
using BosmanCommerce7.Module.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BosmanCommerce7.Module.Extensions.QuartzTools
{

    public static class QuartzApplicationFunctions {
    private static IScheduler? _scheduler;

    public static void StartJobs(QuartzStartJobContext context) {
      _scheduler = QuartzFunctions.CreateScheduler();

      ScheduleCustomerMasterSyncJobQueueService(context, _scheduler);
      ScheduleSalesOrdersSyncJobQueueService(context, _scheduler);
      ScheduleSalesOrdersPostJobQueueService(context, _scheduler);

      _scheduler.Start();
    }

    private static void ScheduleCustomerMasterSyncJobQueueService(QuartzStartJobContext context, IScheduler scheduler) {
      var jobOptions = context.Options.CustomerMasterSyncJobOptions as JobOptionsBase ?? throw new Exception($"{nameof(context.Options.CustomerMasterSyncJobOptions)} not defined in appsettings.json.");

      ScheduleSyncQueueService<ICustomerMasterSyncQueueService>(context, new QuartzStartJobDescriptor {
        JobId = JobIds.CustomerMasterSyncJob,
        JobOptions = jobOptions,
        Scheduler = scheduler
      });
    }

    private static void ScheduleSalesOrdersSyncJobQueueService(QuartzStartJobContext context, IScheduler scheduler) {
      var jobOptions = context.Options.SalesOrdersSyncJobOptions as JobOptionsBase ?? throw new Exception($"{nameof(context.Options.SalesOrdersSyncJobOptions)} not defined in appsettings.json.");

      ScheduleSyncQueueService<ISalesOrdersSyncQueueService>(context, new QuartzStartJobDescriptor {
        JobId = JobIds.SalesOrdersSyncJob,
        JobOptions = jobOptions,
        Scheduler = scheduler
      });
    }

    private static void ScheduleSalesOrdersPostJobQueueService(QuartzStartJobContext context, IScheduler scheduler) {
      var jobOptions = context.Options.SalesOrdersPostJobOptions as JobOptionsBase ?? throw new Exception($"{nameof(context.Options.SalesOrdersPostJobOptions)} not defined in appsettings.json.");

      ScheduleSyncQueueService<ISalesOrdersPostQueueService>(context, new QuartzStartJobDescriptor {
        JobId = JobIds.SalesOrdersPostJob,
        JobOptions = jobOptions,
        Scheduler = scheduler
      });
    }

    private static void ScheduleSyncQueueService<T>(QuartzStartJobContext context, QuartzStartJobDescriptor jobDescriptor) {
      if (!jobDescriptor.JobOptions.Enabled) {
        context.Logger.LogWarning("{service} is disabled", jobDescriptor.JobId);
        return;
      }

      var service = context.ServiceProvider.GetService<T>();

      var job = JobBuilder.Create<ProcessQueueTableJob>()
        .WithIdentity(jobDescriptor.JobId, JobIds.JobsGroup)
        .SetJobData(new JobDataMap() { { JobIds.JobServiceInstance, service! } })
        .Build();

      var cronExpression = string.IsNullOrWhiteSpace(jobDescriptor.JobOptions.RepeatIntervalCronExpression)
        ? CronExpressionFunctions.SecondsInterval(jobDescriptor.JobOptions.RepeatIntervalSeconds)
        : jobDescriptor.JobOptions.RepeatIntervalCronExpression;

      var startTimeUtc = DateTime.Now.AddSeconds(jobDescriptor.JobOptions.StartDelaySeconds);

      context.Logger.LogDebug("Start {service} at {time:o}", jobDescriptor.JobId, startTimeUtc.ToLocalTime());//$"{startTimeUtc.ToLocalTime():o}"

      var trigger = TriggerBuilder.Create()
        .WithIdentity(jobDescriptor.JobId, JobIds.JobsGroup)
        .StartAt(startTimeUtc)
        .WithCronSchedule(cronExpression)
        .Build();

      trigger.PrintNextRun();

      jobDescriptor.Scheduler.ScheduleJob(job, trigger);
    }

    public static void StopJobs() {
      _scheduler?.Shutdown();
    }
  }

  public record QuartzStartJobDescriptor {
    public string JobId { get; init; } = default!;

    public IScheduler Scheduler { get; init; } = default!;

    public JobOptionsBase JobOptions { get; init; } = default!;
  }

  public record QuartzStartJobContext {
    public IServiceProvider ServiceProvider { get; init; } = default!;

    public ILogger Logger { get; init; } = default!;

    public ApplicationOptions Options { get; init; } = default!;
  }
}