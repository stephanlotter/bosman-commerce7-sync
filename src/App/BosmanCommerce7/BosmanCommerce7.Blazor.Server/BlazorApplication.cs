﻿/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-14
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor;

namespace BosmanCommerce7.Blazor.Server;

public class BosmanCommerce7BlazorApplication : BlazorApplication {

  public BosmanCommerce7BlazorApplication() {
    ApplicationName = "Bosman Commerce7 Sync";
    CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
    DatabaseVersionMismatch += BosmanCommerce7BlazorApplication_DatabaseVersionMismatch;
  }

  protected override void OnSetupStarted() {
    base.OnSetupStarted();
#if DEBUG
    if (System.Diagnostics.Debugger.IsAttached && CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema) {
      DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
    }
#endif
  }

  private void BosmanCommerce7BlazorApplication_DatabaseVersionMismatch(object? sender, DatabaseVersionMismatchEventArgs e) {
#if EASYTEST
        e.Updater.Update();
        e.Handled = true;
#else
    if (System.Diagnostics.Debugger.IsAttached) {
      e.Updater.Update();
      e.Handled = true;
    }
    else {
      string message = "The application cannot connect to the specified database, " +
          "because the database doesn't exist, its version is older " +
          "than that of the application or its schema does not match " +
          "the ORM data model structure. To avoid this error, use one " +
          "of the solutions from the https://www.devexpress.com/kb=T367835 KB Article.";

      if (e.CompatibilityError != null && e.CompatibilityError.Exception != null) {
        message += "\r\n\r\nInner exception: " + e.CompatibilityError.Exception.Message;
      }
      throw new InvalidOperationException(message);
    }
#endif
  }

}
