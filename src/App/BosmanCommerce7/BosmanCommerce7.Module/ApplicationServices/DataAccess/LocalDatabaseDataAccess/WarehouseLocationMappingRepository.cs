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

  public class WarehouseLocationMappingRepository : LocalDatabaseRepositoryBase, IWarehouseLocationMappingRepository {

    public WarehouseLocationMappingRepository(ILogger<WarehouseLocationMappingRepository> logger, ILocalDatabaseConnectionStringProvider connectionStringProvider) : base(logger, connectionStringProvider) {
    }

    public Result<WarehouseLocationMapping?> FindMappingByWarehouseCode(IObjectSpace objectSpace, EvolutionWarehouseCode warehouseCode) {
      try {
        var mapping = objectSpace.FindObject<WarehouseLocationMapping>("WarehouseCode".IsEqualToOperator(warehouseCode));
        return mapping;
      }
      catch (Exception ex) {
        Logger.LogError(ex, "Error finding warehouse location mapping.");
        return Result.Failure<WarehouseLocationMapping?>(ex.Message);
      }
    }

    public Result<WarehouseLocationMapping> FindMappingByLocationTitle(IObjectSpace objectSpace, Commerce7LocationTitle locationTile) {
      try {
        if (locationTile == null) {
          return Result.Failure<WarehouseLocationMapping>("Cannot find warehouse location mapping. Location title is empty.");
        }

        var mapping = objectSpace.FindObject<WarehouseLocationMapping>("LocationTitle".IsEqualToOperator(locationTile));
        return mapping == null
          ? Result.Failure<WarehouseLocationMapping>($"There is no warehouse location mapping for location title: '{locationTile}'")
          : Result.Success(mapping);
      }
      catch (Exception ex) {
        Logger.LogError(ex, "Error finding warehouse location mapping.");
        return Result.Failure<WarehouseLocationMapping>(ex.Message);
      }
    }
  }
}