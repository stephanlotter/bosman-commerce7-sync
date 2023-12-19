/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-19
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices {

  public interface IInventoryLevelsLocalMappingService {

    Maybe<Commerce7InventoryId> GetLocalId(EvolutionInventoryItemId evolutionId);

    Result StoreMapping(EvolutionInventoryItemId evolutionId, Commerce7InventoryId commerce7Id);

    Result DeleteMapping(EvolutionInventoryItemId evolutionId);
  }
}