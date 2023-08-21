/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using BosmanCommerce7.Module.BusinessObjects;
using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using Microsoft.Extensions.DependencyInjection;

namespace BosmanCommerce7.Module.Controllers {
  public class PostNowActionController : ActionControllerBase {
    private readonly IServiceProvider? _serviceProvider;

    public PostNowActionController() {
      TargetObjectType = typeof(SalesOrder);
      TargetViewType = ViewType.ListView;
      TargetViewNesting = Nesting.Root;
    }

    [ActivatorUtilitiesConstructor]
    public PostNowActionController(IServiceProvider serviceProvider) : this() {
      _serviceProvider = serviceProvider;
    }

    protected override void CreateActions() {
      var criteria = "PostingStatus".InCriteriaOperator(QueueItemStatus.New, SalesOrderPostingStatus.Retrying, QueueItemStatus.Failed).ToCriteriaString();
      var action = NewAction("Post now", (s, e) => { Execute(); },
        selectionDependencyType: SelectionDependencyType.RequireMultipleObjects,
        targetObjectsCriteria: criteria);
    }

    private void Execute() {
      View.CommitChangesIfEditState();

      var salesOrders = View.SelectedObjects.Cast<SalesOrder>();

      // TODO: Implement posting of sales orders

      View.CommitChangesIfEditState();
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
