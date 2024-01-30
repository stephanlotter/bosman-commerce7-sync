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

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Inventory {

  public class EvolutionWarehouseRepository : EvolutionRepositoryBase, IEvolutionWarehouseRepository {

    public Result<Warehouse> Get(string? code) {
      if (string.IsNullOrWhiteSpace(code)) {
        return Result.Failure<Warehouse>($"Warehouse lookup: code may not be empty.");
      }

      int? id = GetId("select WhseLink from WhseMst where lower(Code)=lower(@code)", new { code });

      if (id == null) {
        return Result.Failure<Warehouse>($"Warehouse with code {code} not found");
      }

      return new Warehouse(id.Value);
    }
  }
}