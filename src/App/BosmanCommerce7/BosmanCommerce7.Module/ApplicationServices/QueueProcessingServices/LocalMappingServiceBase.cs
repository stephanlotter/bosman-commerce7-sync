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
using Serilog;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices {

  public abstract class LocalMappingServiceBase<TE, TC> {
    private readonly IAppDataFileManager _appDataFileManager;

    protected abstract string FolderName { get; }

    protected abstract string FileNamePrefix { get; }

    public LocalMappingServiceBase(IAppDataFileManager appDataFileManager) {
      _appDataFileManager = appDataFileManager;
    }

    public string FileName(TE evolutionId) {
      return $"{FileNamePrefix}{evolutionId}.txt";
    }

    public Result DeleteMapping(TE evolutionId) {
      return _appDataFileManager.RemoveFile(FolderName, FileName(evolutionId));
    }

    public Maybe<TC> GetLocalId(TE evolutionId) {
      var result = _appDataFileManager.LoadText(FolderName, FileName(evolutionId));
      if (result.IsFailure) { return Maybe<TC>.None; }

      try {
        var v = Convert.ChangeType(result.Value, typeof(TC));
        return Maybe<TC>.From((TC)v);
      }
      catch {
        Log.Error($"Error GetLocalId {evolutionId} for type {typeof(TC)}");
        DeleteMapping(evolutionId);

        return Maybe<TC>.None;
      }
    }

    public Result StoreMapping(TE evolutionId, TC commerce7Id) {
      return _appDataFileManager.StoreText(FolderName, FileName(evolutionId), $"{commerce7Id}");
    }
  }
}