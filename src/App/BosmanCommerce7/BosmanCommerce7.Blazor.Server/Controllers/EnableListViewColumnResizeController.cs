/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-22
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Editors;

namespace BosmanCommerce7.Blazor.Server.Controllers {
  public class EnableListViewColumnResizeController : ViewController<ListView> {

    protected override void OnViewControlsCreated() {
      base.OnViewControlsCreated();
      if (View.Editor is DxGridListEditor gridListEditor) {
        IDxGridAdapter dataGridAdapter = gridListEditor.GetGridAdapter();
        dataGridAdapter.GridModel.ColumnResizeMode = DevExpress.Blazor.GridColumnResizeMode.ColumnsContainer;
      }
    }
  }
}
