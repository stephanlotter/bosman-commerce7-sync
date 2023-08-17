/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.Extensions.DataAccess;
using BosmanCommerce7.Module.Models;
using BosmanCommerce7.Module.Models.EvolutionDatabase;
using CSharpFunctionalExtensions;
using Dapper;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.EvolutionDataAccess {

  public class EvolutionInventoryRepository : EvolutionRepositoryBase, IEvolutionInventoryRepository {
    public EvolutionInventoryRepository(ILogger<EvolutionInventoryRepository> logger, IEvolutionDatabaseConnectionStringProvider connectionStringProvider) : base(logger, connectionStringProvider) {
    }

    public Result<EvolutionInventoryDto> GetItem(string simpleCode) {
      const string sql = @"
select i.StockLink ItemId,
       i.cSimpleCode SimpleCode,
       i.Description_1 Description_1,
       sc.cCategoryName CategoryName,
       isv1.cValue SegmentCode1,
       isv2.cValue SegmentCode2,
       isv3.cValue SegmentCode3,
       isv4.cValue SegmentCode4,
       isv5.cValue SegmentCode5,
       isv6.cValue SegmentCode6,
       isv7.cValue SegmentCode7
    from StkItem i
      left join _etblStockDetails sd
          on sd.StockID = i.StockLink
              and sd.WhseID = -1
      left join _etblStockCategories sc
          on sc.idStockCategories = sd.ItemCategoryID
      left join _etblInvSegValue isv1
          on isv1.idInvSegValue = i.iInvSegValue1ID
      left join _etblInvSegValue isv2
          on isv2.idInvSegValue = i.iInvSegValue2ID
      left join _etblInvSegValue isv3
          on isv3.idInvSegValue = i.iInvSegValue3ID
      left join _etblInvSegValue isv4
          on isv4.idInvSegValue = i.iInvSegValue4ID
      left join _etblInvSegValue isv5
          on isv5.idInvSegValue = i.iInvSegValue5ID
      left join _etblInvSegValue isv6
          on isv6.idInvSegValue = i.iInvSegValue6ID
      left join _etblInvSegValue isv7
          on isv7.idInvSegValue = i.iInvSegValue7ID
    where 1=1
        and i.ItemActive = 1
        and i.cSimpleCode = @simpleCode
";

      try {
        var data = ConnectionStringProvider.EvolutionCompany.WrapInOpenConnection(connection => {
          return connection.QueryFirstOrDefault<EvolutionInventoryDto>(sql, new { simpleCode });
        });

        return data != null ? Result.Success(data) : Result.Failure<EvolutionInventoryDto>($"Evolution item {simpleCode} not found or it is inactive.");
      }
      catch (Exception ex) {
        _logger.LogError(ex, "While GetItem");
        return Result.Failure<EvolutionInventoryDto>(ex.Message);
      }

    }

  }

}
