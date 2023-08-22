/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-22
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using CSharpFunctionalExtensions;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class EvolutionGeneralLedgerAccountRepository : EvolutionRepositoryBase, IEvolutionGeneralLedgerAccountRepository {

    public Result<GLAccount> Get(string? code) {
      if (string.IsNullOrWhiteSpace(code)) {
        return Result.Failure<GLAccount>($"General ledger account lookup: code may not be empty.");
      }

      int? id = GetId("select AccountLink from Accounts where lower(Master_Sub_Account)=lower(@code)", new { code });

      if (id == null) {
        return Result.Failure<GLAccount>($"General ledger account with code {code} not found");
      }

      return new GLAccount(id.Value);
    }
  }

}
