/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.Extensions {
  public static class ViewFunctions {

    public static View CommitChangesIfEditStateThen(this View view, Action<Session> action) {
      view.CommitChangesIfEditState();
      action(view.GetSession());
      return view;
    }

    public static View? CommitChangesIfEditState(this View view, Action? beforeCommit = null) {
      if (view == null || view.CurrentObject == null || view.IsListView() || view.IsDetailViewInViewOnlyMode()) { return view; }

      if (view.ObjectSpace.IsModified) {
        beforeCommit?.Invoke();
        view.Commit();
      }

      return view;
    }

    public static void CommitAndCloseDetailView(this View view) {
      if (view == null || view.CurrentObject == null) { return; }
      if (view.IsListView()) { return; }
      if (!view.IsDetailViewInViewOnlyMode()) { view.Commit(); }
      view.CloseView();
      return;
    }

    public static Session GetSession(this View view) => ((XPObjectSpace)view.ObjectSpace).Session;

    public static View CommitAndRefresh(this View view) => view.Commit().RefreshView();

    public static void CloseIfDetailViewOrRefresh(this View view) {
      if (view.IsListView()) {
        view.RefreshView();
        return;
      }

      view.CloseView();
    }

    public static bool IsListView(this View view) => view is ListView;

    public static bool IsDetailViewInViewOnlyMode(this View view) => view is DetailView a && a.ViewEditMode == ViewEditMode.View;

    public static void CloseView(this View view) => view.Close();

    public static View Commit(this View view) {
      view.ObjectSpace.CommitChanges();
      return view;
    }

    public static DetailView? CreateEditDetailView(XafApplication application, Type type, string editViewId, int requestOid) {
      if (requestOid <= 0) { return null; }

      var objectSpace = application.CreateObjectSpace(type);
      var request = objectSpace.FindByOid(type, requestOid);
      if (request == null) { return null; }

      var detailView = application.CreateDetailView(objectSpace, editViewId, true, request);
      detailView.ViewEditMode = ViewEditMode.Edit;
      return detailView;
    }

    public static View Rollback(this View view) {
      view.ObjectSpace.Rollback();
      return view;
    }

    public static View RefreshView(this View view) {
      view.ObjectSpace.Refresh();
      return view;
    }

    public static void SetCaption(this View view, string propertyName, string caption) {
      if (string.IsNullOrWhiteSpace(caption)) { return; }

      if (view is DetailView v) {
        var editor = v.FindItem(propertyName);
        if (editor == null) { return; }
        editor.Caption = caption;
      }
      else if (view is ListView l) {
        var editor = (ColumnsListEditor)l.Editor;
        if (editor == null) { return; }
        var column = editor.FindColumn(propertyName);
        if (column == null) { return; }
        column.Caption = caption;
      }
    }

    public static IList<T?> GetDetailViewChildListViewSelectedObjects<T>(this View view, string listViewPropertyName) {
      if (view is DetailView detailView) {
        if (detailView.FindItem(listViewPropertyName) is ListPropertyEditor lines) {
          return lines.ListView.SelectedObjects.Cast<T?>().ToList() ?? new List<T?>();
        }
      }

      return new List<T?>();
    }

  }

}
