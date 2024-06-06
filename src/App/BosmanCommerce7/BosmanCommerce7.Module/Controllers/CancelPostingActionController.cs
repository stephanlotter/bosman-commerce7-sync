/*
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	:
 *
 */

using BosmanCommerce7.Module.ApplicationServices.DataAccess.LocalDatabaseDataAccess;
using BosmanCommerce7.Module.BusinessObjects.SalesOrders;
using BosmanCommerce7.Module.Extensions;
using BosmanCommerce7.Module.Models;
using CSharpFunctionalExtensions;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using Microsoft.Extensions.DependencyInjection;

namespace BosmanCommerce7.Module.Controllers {

  public class CancelPostingActionController : ActionControllerBase {
    private readonly IServiceProvider? _serviceProvider;

    public CancelPostingActionController() {
      TargetObjectType = typeof(OnlineSalesOrder);
      TargetViewType = ViewType.Any;
      TargetViewNesting = Nesting.Root;
    }

    [ActivatorUtilitiesConstructor]
    public CancelPostingActionController(IServiceProvider serviceProvider) : this() {
      _serviceProvider = serviceProvider;
    }

    protected override void CreateActions() {
      var criteria = "PostingStatus".InCriteriaOperator(SalesOrderPostingStatus.New, SalesOrderPostingStatus.Retrying).ToCriteriaString();
      var action = NewAction("Cancel Posting", (s, e) => { Execute(); },
        selectionDependencyType: SelectionDependencyType.RequireMultipleObjects,
        targetObjectsCriteria: criteria);
    }

    private void Execute() {
      var userHasSelectedSalesOrders = View.SelectedObjects.Count > 0;

      if (!userHasSelectedSalesOrders) {
        ShowErrorMessage("No sales orders selected.");
        return;
      }

      var selectedSalesOrders = userHasSelectedSalesOrders ? View.SelectedObjects.Cast<OnlineSalesOrder>().ToList() : new List<OnlineSalesOrder>();

      var localObjectSpaceProvider = _serviceProvider!.GetService<ILocalObjectSpaceProvider>()!;

      localObjectSpaceProvider.WrapInObjectSpaceTransaction(objectSpace => {
        foreach (var selectedSalesOrder in selectedSalesOrders) {
          var onlineSalesOrder = objectSpace.FindByOid<OnlineSalesOrder>(selectedSalesOrder.Oid);
          onlineSalesOrder.SetAsCancelled();
          onlineSalesOrder.Save();
        }
      });

      ShowSuccessMessage("Selected sales orders cancelled.");

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