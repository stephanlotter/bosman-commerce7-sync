/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-01-26
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk.Sales {

  public abstract class PostToEvolutionJournalTransactionServiceBase {
    protected readonly ILogger<PostToEvolutionJournalTransactionServiceBase> _logger;
    protected readonly IValueStoreRepository _valueStoreRepository;

    public PostToEvolutionJournalTransactionServiceBase(ILogger<PostToEvolutionJournalTransactionServiceBase> logger, IValueStoreRepository valueStoreRepository) {
      _logger = logger;
      _valueStoreRepository = valueStoreRepository;
    }

    protected OrderDetail? GetFirstLineWithWarehouse(SalesDocumentBase customerDocument) {
      foreach (OrderDetail line in customerDocument!.Detail) {
        var warehouse = line.Warehouse;
        if (warehouse != null) { return line; }
      }

      return null;
    }

    protected Result<string> WarehouseCode(SalesDocumentBase customerDocument) {
      var salesOrderLine = GetFirstLineWithWarehouse(customerDocument);

      if (salesOrderLine == null) {
        return Result.Failure<string>("Cannot post transaction.");
      }

      return salesOrderLine.Warehouse.Code;
    }

    protected Result<string> GetCodeFromValueStore(string codeKey) {
      var result = _valueStoreRepository.GetValue(codeKey);

      if (result.IsFailure) {
        return Result.Failure<string>(result.Error);
      }

      if (string.IsNullOrWhiteSpace(result.Value)) {
        return Result.Failure<string>($"Could not find a code entry in ValueStore for {codeKey}");
      }

      return result.Value.Trim().ToUpper();
    }
  }
}