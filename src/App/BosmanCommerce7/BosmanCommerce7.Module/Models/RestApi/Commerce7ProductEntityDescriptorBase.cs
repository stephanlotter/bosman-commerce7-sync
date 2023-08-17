/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

namespace BosmanCommerce7.Module.Models.RestApi {
  public abstract record Commerce7ProductEntityDescriptorBase {

    public long? Id { get; init; }

    public string SimpleCode { get; init; } = default!;

    public string Name { get; init; } = default!;

    public string Description => Name;

    public string? ForeignSystemKey { get; init; }

    public double? Length { get; init; }

    public double? Width { get; init; }

    public double? Thickness { get; init; }

    public string? ModifiedAt { get; init; }

    protected double? GetNumericValueOrNull(string? segmentCode) {
      return double.TryParse(segmentCode, out var v) ? v : null;
    }

  }

}
