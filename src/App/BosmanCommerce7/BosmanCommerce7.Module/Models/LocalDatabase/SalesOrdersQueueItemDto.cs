/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

namespace BosmanCommerce7.Module.Models.LocalDatabase {
  public record SalesOrdersQueueItemDto {

    public int OID { get; init; }

    public string SimpleCode { get; init; } = default!;

    public QueueItemStatus QueueItemStatus { get; init; } = default!;

    public int RetryCount { get; init; }

  }

}
