/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using Bogus;
using BosmanCommerce7.Module.Models.EvolutionDatabase;
using CSharpFunctionalExtensions;

namespace BosmanCommerce7.Module.ApplicationServices.DataAccess.EvolutionDataAccess {
  public class FakeEvolutionInventoryRepository : IEvolutionInventoryRepository {
    public Result<EvolutionInventoryDto> GetItem(string simpleCode) {

      var result = new Faker<EvolutionInventoryDto>()
        .RuleFor(a => a.ItemId, f => int.Parse(simpleCode[1..]))
        .RuleFor(a => a.SimpleCode, f => simpleCode)
        .RuleFor(a => a.Description_1, f => f.Random.Words(2))

        .RuleFor(a => a.CategoryName, f => f.Random.Word())

        .RuleFor(a => a.SegmentCode1, f => simpleCode)
        .RuleFor(a => a.SegmentCode2, f => f.Random.String2(5))
        .RuleFor(a => a.SegmentCode3, f => f.Random.String2(5))

        .RuleFor(a => a.SegmentCode4, f => $"{f.Random.Number(500)}")
        .RuleFor(a => a.SegmentCode5, f => $"{f.Random.Number(500)}")
        .RuleFor(a => a.SegmentCode6, f => $"{f.Random.Number(500)}")
        ;

      return Result.Success(result.Generate());
    }
  }

}
