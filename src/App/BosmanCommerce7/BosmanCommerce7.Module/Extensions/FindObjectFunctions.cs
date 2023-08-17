/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Xpo;

namespace BosmanCommerce7.Module.Extensions {
  public static class FindObjectFunctions {

    public static T FindByOid<T>(this Session session, Guid oid) => session.FromCriteria<T>(oid.IsEqualOidOperator());

    public static T FindByOid<T>(this Session session, int oid) => session.FromCriteria<T>(oid.IsEqualOidOperator());

    public static T FindByOid<T>(this IObjectSpace objectSpace, Guid oid) => objectSpace.FromCriteria<T>(oid.IsEqualOidOperator());

    public static T FindByOid<T>(this IObjectSpace objectSpace, int oid) => objectSpace.FromCriteria<T>(oid.IsEqualOidOperator());

    public static object FindByOid(this IObjectSpace objectSpace, Type type, int oid) => objectSpace.FromCriteria(type, oid.IsEqualOidOperator());

    public static object FindByOid(this Session session, Type type, int oid) => session.FromCriteria(type, oid.IsEqualOidOperator());

    public static T FindByStringProperty<T>(this Session session, string propertyName, string value) => session.FromCriteria<T>(propertyName.StringCriteria(value));

    public static T FindByStringProperty<T>(this IObjectSpace objectSpace, string propertyName, string value) => objectSpace.FromCriteria<T>(propertyName.StringCriteria(value));

    public static T FromCriteria<T>(this Session session, CriteriaOperator criteria) => session.FindObject<T>(criteria);

    public static T FromCriteria<T>(this IObjectSpace objectSpace, CriteriaOperator criteria) => objectSpace.FindObject<T>(criteria);

    public static object FromCriteria(this IObjectSpace objectSpace, Type type, CriteriaOperator criteria) => objectSpace.FindObject(type, criteria);

    public static object FromCriteria(this Session session, Type type, CriteriaOperator criteria) => session.FindObject(type, criteria);

  }

}
