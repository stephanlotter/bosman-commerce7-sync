/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

namespace BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices {
  public static class RetryPolicy {
    public const int MaxRetryCount = 6;

    public static DateTime GetRetryAfter(int retryCount) {
      var minutes = retryCount switch {
        1 => 10,
        2 => 12,
        3 => 15,
        4 => 30,
        5 => 45,
        6 => 60,
        _ => 30
      };

      return DateTime.Now.AddMinutes(minutes);
    }
  }
}