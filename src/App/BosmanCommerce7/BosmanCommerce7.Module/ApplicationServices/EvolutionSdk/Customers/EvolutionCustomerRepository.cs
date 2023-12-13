/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Models.EvolutionSdk.Customers;
using CSharpFunctionalExtensions;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Customers
{

    public class EvolutionCustomerRepository : EvolutionRepositoryBase, IEvolutionCustomerRepository {

    public Result<Customer> GetCustomer(CustomerDescriptor customerDescriptor) {
      Result<Customer> Get(EvolutionCustomerId id) {
        try {
          var customer = new Customer(id);
          if (customer == null) {
            return Result.Failure<Customer>($"Customer with id {id} not found in Evolution");
          }

          return customer;
        }
        catch (Exception ex) {
          return Result.Failure<Customer>($"Customer with id {id} not found in Evolution. ({ex.Message})");
        }
      }

      if (customerDescriptor.CustomerId.HasValue) {
        var c = Get(customerDescriptor.CustomerId.Value);
        if (c.IsSuccess) { return c; }
      }

      if (string.IsNullOrWhiteSpace(customerDescriptor.EmailAddress)) {
        return Result.Failure<Customer>($"Customer account lookup: Email address may not be empty.");
      }

      EvolutionCustomerId? id = GetId("SELECT DCLink FROM Client WHERE ucARwcEmail = @EmailAddress", new { customerDescriptor.EmailAddress });

      if (!id.HasValue) {
        return Result.Failure<Customer>($"Customer with email address {customerDescriptor.EmailAddress} not found in Wine Club E-mail field");
      }

      return Get(id.Value);
    }
  }
}