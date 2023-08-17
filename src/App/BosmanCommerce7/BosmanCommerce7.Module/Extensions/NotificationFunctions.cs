/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using System.Text;
using DevExpress.ExpressApp;

namespace BosmanCommerce7.Module.Extensions {
  public static class NotificationFunctions {

    public static void ShowInfoMessage(this XafApplication application, string message) {
      ShowMessage(application, message, InformationType.Info);
    }

    public static void ShowErrorMessage(this XafApplication application, string message) {
      ShowMessage(application, message, InformationType.Error, 60000);
    }

    public static void ShowErrorMessage(this XafApplication application, Exception ex) {
      var sb = new StringBuilder();
      var e = ex;
      while (e != null) {
        sb.Append(e.Message);
        e = e.InnerException;
      }

      var message = sb.ToString();
      ShowMessage(application, message, InformationType.Error, 60000);
    }

    public static void ShowSuccessMessage(this XafApplication application) {
      application.ShowSuccessMessage("Success");
    }

    public static void ShowSuccessMessage(this XafApplication application, string message) {
      ShowMessage(application, message, InformationType.Success);
    }

    public static void ShowWarnMessage(this XafApplication application, string message) {
      ShowMessage(application, message, InformationType.Warning, 15000);
    }

    private static void ShowMessage(XafApplication application, string message, InformationType informationType, int displayInterval = 3000) {
      application.ShowViewStrategy.ShowMessage(message, informationType, displayInterval, InformationPosition.Bottom);
    }
  }

}
