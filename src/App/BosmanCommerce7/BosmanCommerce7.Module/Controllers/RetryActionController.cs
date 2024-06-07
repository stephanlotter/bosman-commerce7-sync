/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-11-27
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.ApplicationServices.QueueProcessingServices.SalesOrdersPostServices;
using BosmanCommerce7.Module.BusinessObjects.SalesOrders;
using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BosmanCommerce7.Module.Controllers {

  public class RetryActionController : ActionControllerBase {
    private readonly IServiceProvider? _serviceProvider;
    private readonly ILogger<SalesOrdersPostService>? _logger;

    public RetryActionController() {
      TargetObjectType = typeof(OnlineSalesOrder);
      TargetViewType = ViewType.Any;
      TargetViewNesting = Nesting.Root;
    }

    [ActivatorUtilitiesConstructor]
    public RetryActionController(IServiceProvider serviceProvider, ILogger<SalesOrdersPostService> logger) : this() {
      _serviceProvider = serviceProvider;
      _logger = logger;
    }

    protected override void CreateActions() {
      var criteriaPostingStatus = "PostingStatus".InCriteriaOperator(SalesOrderPostingStatus.Failed);
      var criteriaPostingWorkflowState = "PostingWorkflowState".InCriteriaOperator(SalesOrderPostingWorkflowState.Completed).Not();
      var criteria = CriteriaOperator.And(criteriaPostingStatus, criteriaPostingWorkflowState).ToCriteriaString();

      var action = NewAction("Retry", (s, e) => { Execute(); },
        selectionDependencyType: SelectionDependencyType.Independent,
        targetObjectsCriteria: criteria);
    }

    private void Execute() {
      try {
        var localObjectSpaceProvider = _serviceProvider!.GetService<ILocalObjectSpaceProvider>()!;

        localObjectSpaceProvider.WrapInObjectSpaceTransaction(objectSpace => {
          foreach (var salesOrder in View.SelectedObjects.Cast<OnlineSalesOrder>()) {
            var o = objectSpace.GetObjectByKey<OnlineSalesOrder>(salesOrder.Oid);
            o.ResetPostingStatus();
            o.Save();
          }
        });

        View.RefreshView();
        ShowSuccessMessage();
      }
      catch (Exception ex) {
        _logger?.LogError(ex, "Error while retrying sales order posting.");
        ShowErrorMessage(ex.Message);
      }
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