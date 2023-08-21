/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

namespace BosmanCommerce7.Module.Models.EvolutionSdk {

  public interface IEvolutionCompanyDescriptor {

    string BranchCode { get; }

    string EvolutionCommonDatabaseConnectionString { get; }

    string EvolutionCompanyDatabaseConnectionString { get; }

    string? UserName { get; }
  }

}
