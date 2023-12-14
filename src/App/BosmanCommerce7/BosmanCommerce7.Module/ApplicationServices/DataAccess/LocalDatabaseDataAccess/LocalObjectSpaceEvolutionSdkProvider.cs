/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-14
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.EvolutionSdk;
using BosmanCommerce7.Module.Models.EvolutionSdk;
using CSharpFunctionalExtensions;
using DevExpress.ExpressApp;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess {

  public class LocalObjectSpaceEvolutionSdkProvider : ILocalObjectSpaceEvolutionSdkProvider {

    public ILocalObjectSpaceProvider LocalObjectSpaceProvider { get; init; }

    public IEvolutionSdk EvolutionSdk { get; init; }

    public LocalObjectSpaceEvolutionSdkProvider(ILocalObjectSpaceProvider localObjectSpaceProvider, IEvolutionSdk evolutionSdk) {
      LocalObjectSpaceProvider = localObjectSpaceProvider;
      EvolutionSdk = evolutionSdk;
    }

    public Result WrapInObjectSpaceEvolutionSdkTransaction(Func<IObjectSpace, IDatabaseConnection, Result> func) {
      return LocalObjectSpaceProvider.WrapInObjectSpaceTransaction(objectSpace => {
        return EvolutionSdk.WrapInSdkTransaction(connection => {
          return func(objectSpace, connection);
        });
      });
    }

    public Result<T> WrapInObjectSpaceEvolutionSdkTransaction<T>(Func<IObjectSpace, IDatabaseConnection, Result<T>> func) {
      return LocalObjectSpaceProvider.WrapInObjectSpaceTransaction(objectSpace => {
        return EvolutionSdk.WrapInSdkTransaction(connection => {
          return func(objectSpace, connection);
        });
      });
    }

    public Result<T> WrapInObjectSpaceTransaction<T>(Func<IObjectSpace, Result<T>> func) {
      return LocalObjectSpaceProvider.WrapInObjectSpaceTransaction(objectSpace => {
        return func(objectSpace);
      });
    }
  }
}