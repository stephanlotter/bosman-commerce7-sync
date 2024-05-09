/*
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
    e.Updater.Update();
    e.Handled = true;
  }
}