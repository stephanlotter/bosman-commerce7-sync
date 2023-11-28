/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
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

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess
{

    public class BundleMappingRepository : LocalDatabaseRepositoryBase, IBundleMappingRepository {

    public BundleMappingRepository(ILogger<BundleMappingRepository> logger, ILocalDatabaseConnectionStringProvider connectionStringProvider) : base(logger, connectionStringProvider) {
    }

    public Result<BundleMapping?> FindBundleMapping(IObjectSpace objectSpace, string sku) {
      var bundleMapping = objectSpace.FindObject<BundleMapping>("BundleSku".IsEqualToOperator(sku));
      return bundleMapping;
    }
  }
}