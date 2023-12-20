/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.AppDataServices {

  public class AppDataFileManager : IAppDataFileManager {
    private readonly ILogger<AppDataFileManager> _logger;
    private readonly ApplicationOptions _applicationOptions;

    public string RootFolder => _applicationOptions.AppDataFolder;

    public AppDataFileManager(ILogger<AppDataFileManager> logger, ApplicationOptions applicationOptions) {
      _logger = logger;
      _applicationOptions = applicationOptions;
    }

    public bool FileExists(string subfolderName, string filename) => File.Exists(BuildFilename(subfolderName, filename));

    public Result<T?> LoadJson<T>(string subfolderName, string filename) {
      try {
        return LoadText(subfolderName, filename)
          .Map(json => JsonFunctions.Deserialize<T>(json));
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While loading JSON");
        return Result.Failure<T?>(ex.Message);
      }
    }

    public Result<string> LoadText(string subfolderName, string filename) {
      try {
        var file = BuildFilename(subfolderName, filename);

        if (!File.Exists(file)) {
          return Result.Failure<string>($"File '{file}' does not exist.");
        }

        return Result.Success(File.ReadAllText(file));
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While loading text");
        return Result.Failure<string>(ex.Message);
      }
    }

    public Result RemoveFile(string subfolderName, string filename) {
      try {
        var file = BuildFilename(subfolderName, filename);

        File.Delete(file);
        return Result.Success();
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While removing file");
        return Result.Failure(ex.Message);
      }
    }

    public Result<string> StoreJson(string subfolderName, string filename, object? data) {
      var json = JsonFunctions.Serialize(data);
      return StoreText(subfolderName, filename, json);
    }

    public Result<string> StoreText(string subfolderName, string filename, string? data) {
      try {
        var file = BuildFilename(subfolderName, filename);

        File.WriteAllText(file, data ?? "");
        return Result.Success(file);
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While storing text");
        return Result.Failure<string>(ex.Message);
      }
    }

    private string BuildFilename(string subfolderName, string filename) {
      var path = _applicationOptions.InAppDataFolder(subfolderName);
      var file = Path.Combine(path, filename);
      return file;
    }
  }
}