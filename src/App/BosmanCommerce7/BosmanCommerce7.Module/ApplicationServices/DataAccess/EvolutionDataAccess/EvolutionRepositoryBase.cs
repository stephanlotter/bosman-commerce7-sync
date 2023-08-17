/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.EvolutionDataAccess {
  public abstract class EvolutionRepositoryBase : SqlServerRepositoryBase {
    protected readonly IEvolutionDatabaseConnectionStringProvider ConnectionStringProvider;

    public EvolutionRepositoryBase(ILogger logger, IEvolutionDatabaseConnectionStringProvider connectionStringProvider) : base(logger) {
      ConnectionStringProvider = connectionStringProvider;
    }

  }

}
