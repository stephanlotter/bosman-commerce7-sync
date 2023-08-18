/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {

  public abstract class LocalDatabaseRepositoryBase : SqlServerRepositoryBase {
    protected ILocalDatabaseConnectionStringProvider ConnectionStringProvider { get; }

    public LocalDatabaseRepositoryBase(ILogger logger, ILocalDatabaseConnectionStringProvider connectionStringProvider) : base(logger) {
      ConnectionStringProvider = connectionStringProvider;
    }

  }

}
