/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models.EvolutionDatabase;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.EvolutionDataAccess {
  public interface IEvolutionInventoryRepository {
    Result<EvolutionInventoryDto> GetItem(string simpleCode);
  }
}