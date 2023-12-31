﻿/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.Models {
  public record ApplicationOptions {
    public string AppDataFolder { get; init; } = @"..\App_Data";

    public ApiOptions ApiOptions { get; init; } = default!;

    public ConnectionStrings ConnectionStrings { get; init; } = default!;

    public bool EnableDebugLoggingLevel { get; init; }

    public CustomerMasterSyncJobOptions CustomerMasterSyncJobOptions { get; init; } = default!;

    public InventoryItemsSyncJobOptions InventoryItemsSyncJobOptions { get; init; } = default!;

    public InventoryLevelsSyncJobOptions InventoryLevelsSyncJobOptions { get; init; } = default!;

    public SalesOrdersSyncJobOptions SalesOrdersSyncJobOptions { get; init; } = default!;

    public SalesOrdersPostJobOptions SalesOrdersPostJobOptions { get; init; } = default!;

    public string InAppDataFolder(string path) {
      var p = Path.Combine(AppDataFolder, path);
      if (!Directory.Exists(p)) { Directory.CreateDirectory(p); }
      return p;
    }
  }

  public record ApiOptions {
    public string? Endpoint { get; set; }

    public string? TenantId { get; set; }

    public string? AppId { get; set; }

    public string? AppSecretKey { get; set; }
  }

  public abstract record JobOptionsBase {
    public bool Enabled { get; init; }

    public int RepeatIntervalSeconds { get; init; }

    public string? RepeatIntervalCronExpression { get; init; }

    public int StartDelaySeconds { get; init; }
  }

  public record SalesOrdersSyncJobOptions : JobOptionsBase {
    public string[]? ChannelsToProcess { get; init; }
  }

  public record SalesOrdersPostJobOptions : JobOptionsBase {
  }

  public record CustomerMasterSyncJobOptions : JobOptionsBase {
  }

  public record InventoryItemsSyncJobOptions : JobOptionsBase {
  }

  public record InventoryLevelsSyncJobOptions : JobOptionsBase {
  }

  public record ConnectionStrings : ILocalDatabaseConnectionStringProvider, IEvolutionDatabaseConnectionStringProvider {
    public string? LocalDatabase { get; init; }

    public string? EvolutionCompany { get; init; }

    public string? EvolutionCommon { get; init; }
  }
}