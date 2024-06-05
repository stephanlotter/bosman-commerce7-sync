/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2024-06-05
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices.Models {

  public interface IEvolutionCustomerDocument {

    string? Reference { get; }

    string? OrderNumber { get; }

    int? ProjectId { get; }

    string? WarehouseCode { get; }

    DateTime OrderDate { get; }

    int CustomerId { get; }
  }
}