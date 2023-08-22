/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-22
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Models.EvolutionSdk;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk
{

    public class EvolutionPriceListRepository : EvolutionRepositoryBase, IEvolutionPriceListRepository {
    private readonly ILogger<EvolutionPriceListRepository> _logger;

    public EvolutionPriceListRepository(ILogger<EvolutionPriceListRepository> logger) {
      _logger = logger;
    }

    public Result<EvolutionPriceListPrice> Get(int inventoryItemId, int? priceListId) {
      const string sql = @"
if not exists(select 1 from _etblPriceListPrices where iPriceListNameID = @priceListId) 
  select -1;
else
  select fInclPrice PriceValue
	  from _etblPriceListPrices
	  where iPriceListNameID = @priceListId
		  and iStockID = @inventoryItemId
		  and iWarehouseID = 0;
";

      var useDefaultPriceList = !priceListId.HasValue || priceListId.Value <= 0;
      static int? DefaultPriceListId() => PriceList.GetDefault()?.ID;

      priceListId = useDefaultPriceList ? DefaultPriceListId() : priceListId;

      if (!priceListId.HasValue) {
        return Result.Failure<EvolutionPriceListPrice>("No price list id provided and there is no default price list defined in Evolution.");
      }

      var priceValue = GetDouble(sql, new { priceListId, inventoryItemId });

      if (!priceValue.HasValue && !useDefaultPriceList) {
        _logger.LogWarning("Could not find price list ID:{priceListId} in database. Trying default price list.", priceListId);
        priceValue = GetDouble(sql, new { priceListId = DefaultPriceListId(), inventoryItemId });
      }

      return priceValue.HasValue
        ? Result.Success(new EvolutionPriceListPrice(priceListId.Value, priceValue!.Value))
        : Result.Failure<EvolutionPriceListPrice>($"Could not find price list ID:{priceListId} in database.");
    }
  }

}
