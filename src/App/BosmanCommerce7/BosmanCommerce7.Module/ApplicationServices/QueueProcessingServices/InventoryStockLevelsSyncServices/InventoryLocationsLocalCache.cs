/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-19
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.AppDataServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.Models;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventoryItemsSyncServices.RestApi;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.InventorySyncServices {

  public class InventoryLocationsLocalCache : IInventoryLocationsLocalCache {
    private readonly IInventoryItemApiClient _inventoryItemApiClient;
    private readonly IAppDataFileManager _appDataFileManager;
    private InventoryLocationRecord[]? _locationRecords;

    private const string _folderName = "LocationCache";
    private const string _fileName = "locations.json";

    public InventoryLocationsLocalCache(IInventoryItemApiClient inventoryItemApiClient, IAppDataFileManager appDataFileManager) {
      _inventoryItemApiClient = inventoryItemApiClient;
      _appDataFileManager = appDataFileManager;
    }

    public Result<InventoryLocationRecord> GetLocationByTitle(string locationTitle) {
      var tryCount = 0;
      while (true) {
        var r = LoadLocalCache();
        if (r.IsFailure) { return Result.Failure<InventoryLocationRecord>(r.Error); }

        var location = _locationRecords?.FirstOrDefault(x => x.Title.Equals(locationTitle, StringComparison.InvariantCultureIgnoreCase));

        if (location != null) { return Result.Success(location); }

        if (tryCount == 0) {
          r = UpdateLocalCache();
          if (r.IsFailure) { return Result.Failure<InventoryLocationRecord>($"Unable to update local location cache. {r.Error}"); }
        }
        else {
          return Result.Failure<InventoryLocationRecord>($"Location {locationTitle} not found on Commerce7.");
        }
        tryCount++;
      }
    }

    private Result LoadLocalCache() {
      if (_locationRecords is not null || !_appDataFileManager.FileExists(_folderName, _fileName)) { return Result.Success(); }
      var result = _appDataFileManager.LoadJson<InventoryLocationRecord[]>(_folderName, _fileName);
      if (result.IsFailure) { return Result.Failure($"Unable to load local cache. [{_folderName}\\{_fileName}] {result.Error}"); }
      _locationRecords = result.Value;
      return Result.Success();
    }

    public Result UpdateLocalCache() {
      return _inventoryItemApiClient.GetLocations()
        .Bind(MapLocationsToLocationRecords)
        .Bind(y => {
          _appDataFileManager.StoreJson(_folderName, _fileName, y);
          _locationRecords = y;
          return Result.Success();
        });
    }

    private Result<InventoryLocationRecord[]> MapLocationsToLocationRecords(InventoryLocationsResponse value) {
      var locations = value.Data ?? null;

      if (locations == null || locations == null || locations!.Length == 0) { return Result.Failure<InventoryLocationRecord[]>("No data"); }

      var list = new List<InventoryLocationRecord>();

      foreach (var location in locations!) {
        var locationRecord = new InventoryLocationRecord {
          Id = Commerce7InventoryId.Parse($"{location.id}"),
          Title = location.title
        };

        list.Add(locationRecord);
      }

      return Result.Success(list.ToArray());
    }
  }
}