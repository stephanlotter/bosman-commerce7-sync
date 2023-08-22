/* 
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
  public class EvolutionDeliveryMethodRepository : EvolutionRepositoryBase, IEvolutionDeliveryMethodRepository {

    public Result<DeliveryMethod> Get(string? code) {
      if (string.IsNullOrWhiteSpace(code)) {
        return Result.Failure<DeliveryMethod>($"Delivery method lookup: code may not be empty.");
      }

      int? id = GetId("select Counter from DelTbl where Method = @code", new { code });

      if (id == null) {
        return Result.Failure<DeliveryMethod>($"Delivery Method code {code} not found");
      }

      return new DeliveryMethod(id.Value);
    }
  }

}
