/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-22
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models.EvolutionSdk;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk
{
    public interface IEvolutionPriceListRepository {

    Result<EvolutionPriceListPrice> Get(int inventoryItemId, int? priceListId);

  }

}
