/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices {
  public abstract class SyncQueueServiceBase {
    protected ILogger Logger { get; }
    private readonly string _typeName = "";

    public SyncQueueServiceBase(ILogger logger) {
      Logger = logger;
      _typeName = GetType().Name;
    }

    public async Task Execute() {

      await Task.Run(() => {
        try {
          Logger.LogDebug("START: Execute {service}", _typeName);
          ProcessQueue();
        }
        catch (Exception ex) {
          Logger.LogError(ex, "While processing queue: {type}", _typeName);
        }
        finally {
          Logger.LogDebug("END: Execute {service}", _typeName);
        }
      });

    }

    protected abstract void ProcessQueue();
  }

}
