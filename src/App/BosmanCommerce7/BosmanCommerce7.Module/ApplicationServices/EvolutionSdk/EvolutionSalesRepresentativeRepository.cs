﻿/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using CSharpFunctionalExtensions;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class EvolutionSalesRepresentativeRepository : EvolutionRepositoryBase, IEvolutionSalesRepresentativeRepository {

    public Result<SalesRepresentative> Get(string? code) {
      int? id = GetId("select idSalesRep from SalesRep where Code = @code", new { code });

      if (id == null) {
        return Result.Failure<SalesRepresentative>($"Sales Representative with code {code} not found");
      }

      return new SalesRepresentative(id.Value);
    }
  }

}
