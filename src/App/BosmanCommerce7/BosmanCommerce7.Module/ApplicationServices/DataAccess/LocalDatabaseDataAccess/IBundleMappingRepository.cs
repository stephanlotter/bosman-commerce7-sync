/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.BusinessObjects.Settings;
using CSharpFunctionalExtensions;
using DevExpress.ExpressApp;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess
{

    public interface IBundleMappingRepository {

    Result<BundleMapping?> FindBundleMapping(IObjectSpace objectSpace, string sku);
  }
}