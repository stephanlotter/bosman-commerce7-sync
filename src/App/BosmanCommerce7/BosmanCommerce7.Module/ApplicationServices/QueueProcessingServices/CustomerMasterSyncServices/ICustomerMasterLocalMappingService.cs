/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-06
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices {

  public interface ICustomerMasterLocalMappingService {

    Maybe<Commerce7CustomerId> GetLocalId(EvolutionCustomerId evolutionId);

    Result StoreMapping(EvolutionCustomerId evolutionId, Commerce7CustomerId commerce7Id);

    Result DeleteMapping(EvolutionCustomerId evolutionId);
  }
}