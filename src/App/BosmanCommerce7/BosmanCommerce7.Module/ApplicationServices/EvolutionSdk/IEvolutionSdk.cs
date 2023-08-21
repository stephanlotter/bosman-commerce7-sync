/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models.EvolutionSdk;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public interface IEvolutionSdk {

    DatabaseConnection? Connection { get; }

    void BeginTransaction();

    void CommitTransaction();

    void RollbackTransaction();

    Result<T> WrapInSdkTransaction<T>(Func<IDatabaseConnection, Result<T>> func);

    Result WrapInSdkTransaction(Func<IDatabaseConnection, Result> func);
  }

}
