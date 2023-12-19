/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-19
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.AppDataServices;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices {

  public class InventoryLevelsLocalMappingService : LocalMappingServiceBase<EvolutionCustomerId, Commerce7CustomerId>, IInventoryLevelsLocalMappingService {

    protected override string FolderName => "InventoryItemsSyncMappings";

    protected override string FileNamePrefix => "evo-inventory-mapping-";

    public InventoryLevelsLocalMappingService(IAppDataFileManager appDataFileManager) : base(appDataFileManager) {
    }
  }
}