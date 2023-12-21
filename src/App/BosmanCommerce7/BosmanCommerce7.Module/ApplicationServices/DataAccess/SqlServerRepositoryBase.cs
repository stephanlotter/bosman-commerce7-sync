/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess {

  public abstract class SqlServerRepositoryBase {
    protected readonly ILogger Logger;

    public SqlServerRepositoryBase(ILogger logger) {
      Logger = logger;
    }
  }
}