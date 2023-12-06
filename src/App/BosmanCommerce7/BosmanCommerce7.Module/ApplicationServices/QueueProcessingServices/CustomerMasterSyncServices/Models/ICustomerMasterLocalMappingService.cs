/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-06
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models {

  public interface ICustomerMasterLocalMappingService {

    Maybe<Commerce7CustomerId> GetLocalCustomerId(EvolutionCustomerId evolutionCustomerId);

    Result StoreMapping(EvolutionCustomerId evolutionCustomerId, Commerce7CustomerId commerce7CustomerId);

    Result DeleteMapping(EvolutionCustomerId evolutionCustomerId);
  }
}