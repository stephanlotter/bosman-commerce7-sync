/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

namespace BosmanCommerce7.Module.Extensions.QuartzTools {
  public static class CronExpressionFunctions {

    public static string SecondsInterval(int seconds) => $"*/{seconds} * * ? * * *";

    public static string MinutesInterval(int minutes) => $"0 */{minutes} * ? * * *";
  }
}
