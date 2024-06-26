﻿/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.Models.EvolutionSdk.Customers {
  public record CustomerDescriptor {
    public EvolutionCustomerId? CustomerId { get; init; }

    public string? EmailAddress { get; init; }

    public EvolutionCustomerCode? AccountCode { get; init; }
  }
}