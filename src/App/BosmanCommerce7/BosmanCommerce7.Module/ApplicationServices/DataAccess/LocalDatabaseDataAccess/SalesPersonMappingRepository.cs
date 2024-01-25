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

    public Result<SalesPersonMapping?> FindMapping(IObjectSpace objectSpace, string? commerce7SalesAssociateName) {
      try {
        if (string.IsNullOrEmpty(commerce7SalesAssociateName)) { return Result.Failure<SalesPersonMapping?>("Cannot find Evolution sales rep code mapping. The Commerce7 Sales Associate name is empty."); }

        var mapping = objectSpace.FindObject<SalesPersonMapping>(nameof(SalesPersonMapping.Commerce7SalesAssociateName).IsEqualToOperator(commerce7SalesAssociateName));

        if (mapping == null) {
          return Result.Failure<SalesPersonMapping?>($"Cannot find Evolution sales rep code mapping. The Commerce7 Sales Associate name '{commerce7SalesAssociateName}' is not mapped to an Evolution sale rep code.");
        }

        return mapping;
      }
      catch (Exception ex) {
        Logger.LogError(ex, "Error finding Sales Person Mapping.");
        return Result.Failure<SalesPersonMapping?>(ex.Message);
      }
    }
  }
}