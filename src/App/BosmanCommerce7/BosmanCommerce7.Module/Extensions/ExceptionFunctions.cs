/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using System.Text;

namespace BosmanCommerce7.Module.Extensions {
  public static class ExceptionFunctions {

    public static string GetMessages(Exception? ex) {
      var sb = new StringBuilder();
      var e = ex;
      while (e != null) {
        sb.AppendLine(e.Message);
        e = e.InnerException;
      }
      return sb.ToString();
    }

  }
}
