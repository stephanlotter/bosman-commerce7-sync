/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-06
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.AppDataServices;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.CustomerMasterSyncServices.Models {

  public class CustomerMasterLocalMappingService : ICustomerMasterLocalMappingService {
    private readonly IAppDataFileManager _appDataFileManager;

    private const string FolderName = "CustomerMasterSyncMappings";

    public CustomerMasterLocalMappingService(IAppDataFileManager appDataFileManager) {
      _appDataFileManager = appDataFileManager;
    }

    public string FileName(EvolutionCustomerId evolutionCustomerId) {
      return $"evo-customer-mapping-{evolutionCustomerId}.txt";
    }

    public Result DeleteMapping(EvolutionCustomerId evolutionCustomerId) {
      return _appDataFileManager.RemoveFile(FolderName, FileName(evolutionCustomerId));
    }

    public Maybe<Commerce7CustomerId> GetLocalCustomerId(EvolutionCustomerId evolutionCustomerId) {
      var result = _appDataFileManager.LoadText(FolderName, FileName(evolutionCustomerId));
      if (result.IsFailure) { return Maybe<Commerce7CustomerId>.None; }

      if (Commerce7CustomerId.TryParse(result.Value, out var commerce7CustomerId)) { return commerce7CustomerId; }

      DeleteMapping(evolutionCustomerId);

      return Maybe<Commerce7CustomerId>.None;
    }

    public Result StoreMapping(EvolutionCustomerId evolutionCustomerId, Commerce7CustomerId commerce7CustomerId) {
      return _appDataFileManager.StoreText(FolderName, FileName(evolutionCustomerId), commerce7CustomerId.ToString());
    }
  }
}