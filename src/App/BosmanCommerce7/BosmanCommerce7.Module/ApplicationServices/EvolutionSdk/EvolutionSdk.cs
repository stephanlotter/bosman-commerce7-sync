/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using System.Text.RegularExpressions;
using BosmanCommerce7.Module.Models.EvolutionSdk;
using CSharpFunctionalExtensions;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {

  public class EvolutionSdk : IEvolutionSdk {
    private readonly IEvolutionCompanyDescriptor _companyDescriptor;

    public DatabaseConnection Connection => new(DatabaseContext.DBConnection, DatabaseContext.DBTransaction);

    public EvolutionSdk(IEvolutionCompanyDescriptor companyDescriptor) {
      _companyDescriptor = companyDescriptor;
    }

    public Result WrapInSdkTransaction(Func<IDatabaseConnection, Result> func) {
      try {
        BeginTransaction();

        var result = func(new DatabaseConnection(DatabaseContext.DBConnection, DatabaseContext.DBTransaction));

        result
          .Tap(() => { CommitTransaction(); })
          .TapError(() => { RollbackTransaction(); });

        return result;
      }
      catch {
        DatabaseContext.RollbackTran();
        throw;
      }
    }

    public Result<T> WrapInSdkTransaction<T>(Func<IDatabaseConnection, Result<T>> func) {
      try {
        BeginTransaction();

        var result = func(new DatabaseConnection(DatabaseContext.DBConnection, DatabaseContext.DBTransaction));

        result
          .Tap(() => { CommitTransaction(); })
          .TapError(() => { RollbackTransaction(); });

        return result;
      }
      catch {
        RollbackTransaction();
        throw;
      }
    }

    public void BeginTransaction() {
      BeginTransaction(_companyDescriptor.EvolutionCompanyDatabaseConnectionString,
        _companyDescriptor.EvolutionCommonDatabaseConnectionString,
        _companyDescriptor.BranchCode,
        string.IsNullOrWhiteSpace(_companyDescriptor.UserName) ? null : _companyDescriptor.UserName);
    }

    public void CommitTransaction() {
      if (!DatabaseContext.IsTransactionPending) { return; }
      DatabaseContext.CommitTran();
    }

    public void RollbackTransaction() {
      if (!DatabaseContext.IsTransactionPending) { return; }
      DatabaseContext.RollbackTran();
    }

    private static void BeginTransaction(string companyConnectionString, string commonDatabaseConnectionString, string branchCode, string? userName) {
      if (DatabaseContext.IsConnectionOpen) { return; }

      var retryCount = 0;

      while (true) {

        try {
          DatabaseContext.CreateCommonDBConnection(commonDatabaseConnectionString);
          DatabaseContext.SetLicense("DE09110064", "2428759");
          DatabaseContext.CreateConnection(companyConnectionString);
          DatabaseContext.BeginTran();

          if (!string.IsNullOrWhiteSpace(branchCode)) {
            var branch = new Branch(branchCode);
            DatabaseContext.SetBranchContext(branch.ID);
          }

          if (!string.IsNullOrWhiteSpace(userName)) {
            DatabaseContext.CurrentAgent = new Agent(userName);
          }

          return;

        }
        catch (Exception ex) when (Regex.IsMatch(ex.Message, ".*registration.*invalid.*")) {
          throw new Exception("Evolution SDK registration is invalid. Check that Evolution is registered and that the version of the SDK is the same as that of Evolution.");
        }
        catch (FormatException ex) when (Regex.IsMatch(ex.Message, ".*Input string was not in a correct format.*")) {
          if (retryCount > 3) {
            throw;
          }
          Thread.Sleep(2000);

          retryCount++;
        }

      }

    }
  }

}
