/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-06
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.AppDataServices;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices {

  public class CustomerMasterLocalMappingService : LocalMappingServiceBase<EvolutionCustomerId, Commerce7CustomerId>, ICustomerMasterLocalMappingService {

    protected override string FolderName => "CustomerMasterSyncMappings";

    protected override string FileNamePrefix => "evo-customer-mapping-";

    public CustomerMasterLocalMappingService(IAppDataFileManager appDataFileManager) : base(appDataFileManager) {
    }
  }
}