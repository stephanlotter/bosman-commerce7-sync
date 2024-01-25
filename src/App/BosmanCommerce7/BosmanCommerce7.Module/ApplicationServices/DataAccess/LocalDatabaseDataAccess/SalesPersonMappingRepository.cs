/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-20
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.BusinessObjects.Settings;
using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using DevExpress.ExpressApp;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {

  public class SalesPersonMappingRepository : LocalDatabaseRepositoryBase, ISalesPersonMappingRepository {

    public SalesPersonMappingRepository(ILogger<SalesPersonMappingRepository> logger, ILocalDatabaseConnectionStringProvider connectionStringProvider) : base(logger, connectionStringProvider) {
    }

    public Result<SalesPersonMapping?> FindMapping(IObjectSpace objectSpace, string commerce7SalesAssociateName) {
      try {
        var mapping = objectSpace.FindObject<SalesPersonMapping>("Commerce7SalesAssociateName".IsEqualToOperator(commerce7SalesAssociateName));
        return mapping;
      }
      catch (Exception ex) {
        Logger.LogError(ex, "Error finding Sales Person Mapping.");
        return Result.Failure<SalesPersonMapping?>(ex.Message);
      }
    }
  }
}