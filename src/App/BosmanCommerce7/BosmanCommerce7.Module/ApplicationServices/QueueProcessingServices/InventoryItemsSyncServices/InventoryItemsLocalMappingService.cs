/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-13
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.AppDataServices;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices {

  public class InventoryItemsLocalMappingService : LocalMappingServiceBase<EvolutionCustomerId, Commerce7CustomerId>, IInventoryItemsLocalMappingService {

    protected override string FolderName => "InventoryItemsSyncMappings";

    protected override string FileNamePrefix => "evo-inventory-mapping-";

    public InventoryItemsLocalMappingService(IAppDataFileManager appDataFileManager) : base(appDataFileManager) {
    }
  }
}