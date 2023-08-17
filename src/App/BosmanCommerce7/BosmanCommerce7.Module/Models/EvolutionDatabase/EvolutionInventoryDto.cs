/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

namespace BosmanCommerce7.Module.Models.EvolutionDatabase {
  public record EvolutionInventoryDto {

    public int ItemId { get; init; }

    public string SimpleCode { get; init; } = default!;

    public string Description_1 { get; init; } = default!;

    public string? CategoryName { get; init; }

    public string? SegmentCode1 { get; init; }

    public string? SegmentCode2 { get; init; }

    public string? SegmentCode3 { get; init; }

    public string? SegmentCode4 { get; init; }

    public string? SegmentCode5 { get; init; }

    public string? SegmentCode6 { get; init; }

    public string? SegmentCode7 { get; init; }

    public double LastCost { get; init; }

  }
}
