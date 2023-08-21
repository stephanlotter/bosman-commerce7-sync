/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using System.ComponentModel;
using BosmanCommerce7.Module.Extensions;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.Controllers {
  public abstract class ActionControllerBase : ViewController {
    protected IObjectSpace? PopupObjectSpace { get; private set; }

    protected virtual string? ActionId => null;

    protected int CurrentObjectOid {
      get {
        switch (View.CurrentObject) {
          case XPObject o:
            return o.Oid;

          default:
            throw new ArgumentOutOfRangeException();
        }
      }
    }

    protected IContainer Components { get; }

    protected ViewType PopupViewType;

    protected ActionControllerBase() {
      Components = new Container();
      PopupViewType = ViewType.DetailView;
    }

    protected override void OnAfterConstruction() {
      base.OnAfterConstruction();
      CreateActions();
    }

    protected abstract void CreateActions();

    protected override void Dispose(bool disposing) {
      if (disposing) {
        PopupObjectSpace?.Dispose();
        Components?.Dispose();
      }
      base.Dispose(disposing);
    }

    protected SimpleAction NewToolsAction(
    string caption,
    SimpleActionExecuteEventHandler handler,
    string targetObjectsCriteria = "",
    SelectionDependencyType selectionDependencyType = SelectionDependencyType.RequireSingleObject,
    bool confirmationPrompt = true) {

      var action = NewAction(
        caption,
        handler,
        targetObjectsCriteria,
        selectionDependencyType,
        confirmationPrompt);

      action.Category = "Tools";

      return action;
    }

    protected SimpleAction NewAction(
      string caption,
      SimpleActionExecuteEventHandler handler,
      string targetObjectsCriteria = "",
      SelectionDependencyType selectionDependencyType = SelectionDependencyType.RequireSingleObject,
      bool confirmationPrompt = true,
      string tooltip = "",
      string? actionId = null) {

      var action = new SimpleAction(Components) {
        Caption = caption,
        ConfirmationMessage = confirmationPrompt ? "Are you sure?" : string.Empty,
        Id = actionId ?? ActionId ?? $"[{caption}]{GetType()}",
        SelectionDependencyType = selectionDependencyType,
        TargetObjectsCriteriaMode = TargetObjectsCriteriaMode.TrueForAll,
        TargetObjectsCriteria = string.IsNullOrWhiteSpace(targetObjectsCriteria) ? null : targetObjectsCriteria,
        ToolTip = tooltip
      };

      action.Execute += handler;

      OnAfterActionCreated(action);

      Actions.Add(action);
      return action;
    }

    protected PopupWindowShowAction NewPopupWindowShowAction(
      string acceptButtonCaption,
      string caption,
      string actionId,
      string targetObjectsCriteria = "",
      SelectionDependencyType selectionDependencyType = SelectionDependencyType.RequireSingleObject) {
      var action = new PopupWindowShowAction(Components) {
        AcceptButtonCaption = acceptButtonCaption,
        CancelButtonCaption = null,
        Caption = caption,
        ConfirmationMessage = null,
        Id = actionId,
        SelectionDependencyType = selectionDependencyType,
        ToolTip = null,
        TargetObjectsCriteriaMode = TargetObjectsCriteriaMode.TrueForAll,
        TargetObjectsCriteria = targetObjectsCriteria
      };

      action.CustomizePopupWindowParams += Pop_CustomizePopupWindowParams;
      action.Execute += Popup_Execute;

      OnAfterActionCreated(action);

      Actions.Add(action);

      return action;
    }

    protected virtual void OnAfterActionCreated(ActionBase viewAction) {
    }

    private void Pop_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e) {
      View.CommitChangesIfEditState();
      AssignPopupView(sender, e, CreatePopupView(e));
      OnCustomizePopupWindowParams(sender, e);
    }

    protected virtual View CreatePopupView(CustomizePopupWindowParamsEventArgs e) {
      PopupObjectSpace = e.Application.CreateObjectSpace(GetPopupParameterObjectType(e.Action));
      OnObjectSpaceCreated(e.Application, PopupObjectSpace);

      if (PopupObjectSpace is NonPersistentObjectSpace compositeObjectSpace) {
        compositeObjectSpace.AutoDisposeAdditionalObjectSpaces = true;
        compositeObjectSpace.AutoRefreshAdditionalObjectSpaces = true;
      }

      if (PopupViewType == ViewType.ListView) {
        var listView = CreatePopupListView(e.Application, e.Action, PopupObjectSpace);
        return listView;
      }

      var detailView = CreatePopupDetailView(e.Application, e.Action, PopupObjectSpace);
      detailView.ViewEditMode = ViewEditMode.Edit;
      return detailView;
    }

    protected virtual void OnObjectSpaceCreated(XafApplication application, IObjectSpace PopupObjectSpace) {
    }

    protected virtual void AssignPopupView(object sender, CustomizePopupWindowParamsEventArgs e, View view) {
      e.View = view;
    }

    protected virtual void OnCustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e) {
    }

    protected DetailView CreatePopupDetailView(XafApplication application, ActionBase action, IObjectSpace PopupObjectSpace) {
      return application.CreateDetailView(PopupObjectSpace, GetPopupDetailViewModel(action), true);
    }

    protected ListView CreatePopupListView(XafApplication application, ActionBase action, IObjectSpace PopupObjectSpace) {
      var viewId = GetListViewId();
      var collectionSource = Application.CreateCollectionSource(PopupObjectSpace, GetListViewCollectionSourceType(), viewId);
      OnAfterCollectionSourceCreated(collectionSource);
      var criteria = GetListViewCollectionSourceCriteria();
      if (!string.IsNullOrWhiteSpace(criteria)) { collectionSource.SetCriteria(viewId, criteria); }

      var view = application.CreateListView(viewId, collectionSource, true);

      return view;
    }

    protected virtual void OnAfterCollectionSourceCreated(CollectionSourceBase collectionSource) {
    }

    protected virtual string? GetListViewCollectionSourceCriteria() {
      return null;
    }

    protected virtual string GetListViewId() {
      throw new NotImplementedException();
    }

    protected virtual Type GetListViewCollectionSourceType() {
      throw new NotImplementedException();
    }

    protected abstract object GetPopupDetailViewModel(ActionBase action);

    protected abstract Type GetPopupParameterObjectType(ActionBase action);

    private void Popup_Execute(object sender, PopupWindowShowActionExecuteEventArgs e) => ExecutePopup(sender, e);

    protected abstract void ExecutePopup(object sender, PopupWindowShowActionExecuteEventArgs args);

    protected void ShowErrorMessage(string error) => Application.ShowErrorMessage(error);

    protected void ShowSuccessMessage(string message = "Success") => Application.ShowSuccessMessage(message);

    protected object GetParentObject() => ((NestedFrame)Frame).ViewItem.View.CurrentObject;

    protected View GetParentView() => ((NestedFrame)Frame).ViewItem.View;

    protected T? ResolveService<T>() => (T?)Application.ServiceProvider.GetService(typeof(T));

  }
}
