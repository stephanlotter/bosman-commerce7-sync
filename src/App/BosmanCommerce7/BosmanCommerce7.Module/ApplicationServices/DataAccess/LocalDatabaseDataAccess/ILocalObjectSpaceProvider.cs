/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using CSharpFunctionalExtensions;
using DevExpress.ExpressApp;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {

  public interface ILocalObjectSpaceProvider {

    void WrapInObjectSpaceTransaction(Action<IObjectSpace> action);

    Result WrapInObjectSpaceTransaction(Func<IObjectSpace, Result> func);
  }
}