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
using Serilog;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Customers {

  public class EvolutionCustomerRepository : EvolutionRepositoryBase, IEvolutionCustomerRepository {

    public Result<Customer> Get(CustomerDescriptor customerDescriptor) {
      Result<Customer> GetById(EvolutionCustomerId id) {
        try {
          var customer = new Customer(id);
          if (customer == null) {
            return Result.Failure<Customer>($"Customer with id {id} not found in Evolution");
          }

          return customer;
        }
        catch (Exception ex) {
          Log.Error(ex, $"Error getting customer with id {id} from Evolution.");
          return Result.Failure<Customer>($"Customer with id {id} not found in Evolution. ({ex.Message})");
        }
      }

      Result<Customer> GetByAccountCode(EvolutionCustomerCode? accountCode) {
        try {
          var customer = new Customer(accountCode);
          if (customer == null) {
            return Result.Failure<Customer>($"Customer with code {accountCode} not found in Evolution");
          }

          return customer;
        }
        catch (Exception ex) {
          Log.Error(ex, $"Error getting customer with code {accountCode} from Evolution.");
          return Result.Failure<Customer>($"Customer with code {accountCode} not found in Evolution. ({ex.Message})");
        }
      }

      Result<Customer> GetByEmailAddress(string? emailAddress) {
        if (string.IsNullOrWhiteSpace(emailAddress)) {
          return Result.Failure<Customer>($"Customer account lookup: Email address may not be empty.");
        }
        EvolutionCustomerId? id = GetId("SELECT DCLink FROM Client WHERE ucARwcEmail = @EmailAddress", new { emailAddress });

        if (!id.HasValue) {
          return Result.Failure<Customer>($"Customer with email address {emailAddress} not found in Wine Club E-mail field (ucARwcEmail)");
        }

        return GetById(id.Value);
      }

      if (customerDescriptor.CustomerId.HasValue) {
        var c = GetById(customerDescriptor.CustomerId.Value);
        if (c.IsSuccess) { return c; }
        if (string.IsNullOrWhiteSpace(customerDescriptor.EmailAddress) && string.IsNullOrWhiteSpace(customerDescriptor.AccountCode)) {
          return Result.Failure<Customer>("Cannot find customer by Id and no email address or account code was provided.");
        }
      }

      if (!string.IsNullOrWhiteSpace(customerDescriptor.AccountCode)) {
        return GetByAccountCode(customerDescriptor.AccountCode);
      }

      return GetByEmailAddress(customerDescriptor.EmailAddress);
    }
  }
}