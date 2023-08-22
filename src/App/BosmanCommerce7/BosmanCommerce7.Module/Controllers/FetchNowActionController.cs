/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersSyncServices.Models;
using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.Extensions;
using CSharpFunctionalExtensions;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using Microsoft.Extensions.DependencyInjection;

namespace BosmanCommerce7.Module.Controllers {

  public class FetchNowActionController : ActionControllerBase {
    private readonly IServiceProvider? _serviceProvider;

    public FetchNowActionController() {
      TargetObjectType = typeof(OnlineSalesOrder);
      TargetViewType = ViewType.ListView;
      TargetViewNesting = Nesting.Root;
    }

    [ActivatorUtilitiesConstructor]
    public FetchNowActionController(IServiceProvider serviceProvider) : this() {
      _serviceProvider = serviceProvider;
    }

    protected override void CreateActions() {
      var action = NewAction("Fetch now", (s, e) => { Execute(); }, selectionDependencyType: SelectionDependencyType.Independent);
    }

    private void Execute() {
      var service = _serviceProvider!.GetService<ISalesOrdersSyncService>();
      var context = new SalesOrdersSyncContext();

      service!
        .Execute(context)
        .Tap(() => { ShowSuccessMessage(); })
        .TapError(ShowErrorMessage);

      View.RefreshView();
    }

    protected override void ExecutePopup(object sender, PopupWindowShowActionExecuteEventArgs args) {
      throw new NotImplementedException();
    }

    protected override object GetPopupDetailViewModel(ActionBase action) {
      throw new NotImplementedException();
    }

    protected override Type GetPopupParameterObjectType(ActionBase action) {
      throw new NotImplementedException();
    }

  }

}
