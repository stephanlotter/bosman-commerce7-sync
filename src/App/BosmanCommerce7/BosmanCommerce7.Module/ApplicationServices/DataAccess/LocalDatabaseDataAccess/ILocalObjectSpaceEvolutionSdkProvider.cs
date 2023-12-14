/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-14
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Models.EvolutionSdk;
using CSharpFunctionalExtensions;
using DevExpress.ExpressApp;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {

  public interface ILocalObjectSpaceEvolutionSdkProvider {

    Result WrapInObjectSpaceEvolutionSdkTransaction(Func<IObjectSpace, IDatabaseConnection, Result> func);

    Result<T> WrapInObjectSpaceEvolutionSdkTransaction<T>(Func<IObjectSpace, IDatabaseConnection, Result<T>> func);

    Result<T> WrapInObjectSpaceTransaction<T>(Func<IObjectSpace, Result<T>> func);
  }
}