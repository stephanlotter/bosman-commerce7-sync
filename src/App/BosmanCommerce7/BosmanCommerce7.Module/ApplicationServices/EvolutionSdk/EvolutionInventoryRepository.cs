/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-12-20
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.Models.EvolutionSdk;
using CSharpFunctionalExtensions;
using Dapper;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {

  public class EvolutionInventoryRepository : EvolutionRepositoryBase, IEvolutionInventoryRepository {
    private readonly ILogger<EvolutionInventoryRepository> _logger;

    public EvolutionInventoryRepository(ILogger<EvolutionInventoryRepository> logger) {
      _logger = logger;
    }

    public Result<EvolutionInventoryLevel> Get(EvolutionInventoryItemId inventoryItemId, EvolutionWarehouseId warehouseId) {
      const string sql = @"
select si.cSimpleCode Sku,
	   wm.Code WarehouseCode,
	   ws.WHWhseID,
	   ws.WHStockLink,
	   isnull(ws.WHQtyOnHand, 0) QuantityOnHand,
	   isnull(ws.WHQtyOnSO, 0) QuantityOnSalesOrder,
	   isnull(ws.WHQtyReserved, 0) QuantityReserved
	from WhseStk ws
	  join StkItem si
		  on si.StockLink = ws.WHStockLink
	  join WhseMst wm
		  on wm.WhseLink = ws.WHWhseID
	where ws.WHWhseID = @WarehouseId
		and ws.WHStockLink = @InventoryItemId;
";

      var data = Connection.QueryFirstOrDefault<EvolutionInventoryLevel?>(sql, new { InventoryItemId = inventoryItemId, WarehouseId = warehouseId }, transaction: Transaction);

      return data != null
        ? Result.Success(data)
        : Result.Failure<EvolutionInventoryLevel>($"Could not find inventory level for product ID {inventoryItemId} in warehouse ID {warehouseId}.");
    }
  }
}