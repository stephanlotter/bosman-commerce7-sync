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
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class EvolutionCustomerRepository : EvolutionRepositoryBase, IEvolutionCustomerRepository {

    public Result<Customer> Get(CustomerDescriptor customerDescriptor) {
      int? id = GetId("SELECT DCLink FROM Client WHERE ucARwcEmail = @EmailAddress", new { customerDescriptor.EmailAddress });

      if (id == null) {
        return Result.Failure<Customer>($"Customer with email address {customerDescriptor.EmailAddress} not found");
      }

      return new Customer(id.Value);
    }
  }

}
