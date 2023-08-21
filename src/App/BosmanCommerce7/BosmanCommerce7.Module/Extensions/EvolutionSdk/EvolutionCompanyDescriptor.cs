/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

namespace BosmanCommerce7.Module.Extensions.EvolutionSdk {
  public class EvolutionCompanyDescriptor : IEvolutionCompanyDescriptor {

    public string BranchCode { get; }

    public string EvolutionCommonDatabaseConnectionString { get; }

    public string EvolutionCompanyDatabaseConnectionString { get; }

    public string? UserName { get; }

    public EvolutionCompanyDescriptor(string evolutionCompanyDatabaseConnectionString, string evolutionCommonDatabaseConnectionString, string? userName = null, string branchCode = "") {
      EvolutionCompanyDatabaseConnectionString = evolutionCompanyDatabaseConnectionString;
      EvolutionCommonDatabaseConnectionString = evolutionCommonDatabaseConnectionString;
      UserName = userName;
      BranchCode = branchCode;
    }
  }

}
