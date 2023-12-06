/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices {

  public abstract class SyncServiceBase {
    protected const int MaxRetryCount = 6;
    protected int _errorCount;
    protected readonly ILocalObjectSpaceProvider LocalObjectSpaceProvider;

    protected ILogger Logger { get; }

    public SyncServiceBase(ILogger logger, ILocalObjectSpaceProvider localObjectSpaceProvider) {
      Logger = logger;
      LocalObjectSpaceProvider = localObjectSpaceProvider;
    }

    protected DateTime GetRetryAfter(int retryCount) {
      var minutes = retryCount switch {
        1 => 1,
        2 => 2,
        3 => 5,
        4 => 10,
        5 => 15,
        6 => 30,
        _ => 30
      };

      return DateTime.Now.AddMinutes(minutes);
    }
  }
}