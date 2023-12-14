/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using System.Data.SqlClient;
using BosmanCommerce7.Module.BusinessObjects.SalesOrders;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {

  public class LocalObjectSpaceProvider : ILocalObjectSpaceProvider {
    private readonly ILocalDatabaseConnectionStringProvider _localDatabaseConnectionStringProvider;

    public LocalObjectSpaceProvider(ILocalDatabaseConnectionStringProvider localDatabaseConnectionStringProvider) {
      _localDatabaseConnectionStringProvider = localDatabaseConnectionStringProvider;
    }

    public void WrapInObjectSpaceTransaction(Action<IObjectSpace> action) {
      using var connection = new SqlConnection(_localDatabaseConnectionStringProvider.LocalDatabase);
      using var osp = new XPObjectSpaceProvider(_localDatabaseConnectionStringProvider.LocalDatabase, connection, threadSafe: true, useSeparateDataLayers: true);

      using IObjectSpace objectSpace = osp.CreateObjectSpace();

      osp.TypesInfo.RegisterEntity(typeof(OnlineSalesOrder));

      try {
        action(objectSpace);
        objectSpace.CommitChanges();
      }
      catch {
        objectSpace.Rollback();
        throw;
      }
    }

    public Result WrapInObjectSpaceTransaction(Func<IObjectSpace, Result> func) {
      using var connection = new SqlConnection(_localDatabaseConnectionStringProvider.LocalDatabase);
      using var osp = new XPObjectSpaceProvider(_localDatabaseConnectionStringProvider.LocalDatabase, connection, threadSafe: true, useSeparateDataLayers: true);

      using IObjectSpace objectSpace = osp.CreateObjectSpace();

      osp.TypesInfo.RegisterEntity(typeof(OnlineSalesOrder));

      try {
        var result = func(objectSpace);
        objectSpace.CommitChanges();
        return result;
      }
      catch {
        objectSpace.Rollback();
        throw;
      }
    }

    public Result<T> WrapInObjectSpaceTransaction<T>(Func<IObjectSpace, Result<T>> func) {
      using var connection = new SqlConnection(_localDatabaseConnectionStringProvider.LocalDatabase);
      using var osp = new XPObjectSpaceProvider(_localDatabaseConnectionStringProvider.LocalDatabase, connection, threadSafe: true, useSeparateDataLayers: true);

      using IObjectSpace objectSpace = osp.CreateObjectSpace();

      osp.TypesInfo.RegisterEntity(typeof(OnlineSalesOrder));

      try {
        var result = func(objectSpace);
        objectSpace.CommitChanges();
        return result;
      }
      catch {
        objectSpace.Rollback();
        throw;
      }
    }
  }
}