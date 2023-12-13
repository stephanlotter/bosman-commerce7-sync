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
    public interface IEvolutionCustomerRepository
    {

        Result<Customer> GetCustomer(CustomerDescriptor customerDescriptor);

    }

}
