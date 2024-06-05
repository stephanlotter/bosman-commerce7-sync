/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-06-05
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models {
  public record EvolutionCustomerDocument : IEvolutionCustomerDocument {
    public string? Reference { get; init; }

    public string? OrderNumber { get; init; }

    public int? ProjectId { get; init; }

    public string? WarehouseCode { get; init; }

    public DateTime OrderDate { get; init; }

    public int CustomerId { get; init; }
  }
}