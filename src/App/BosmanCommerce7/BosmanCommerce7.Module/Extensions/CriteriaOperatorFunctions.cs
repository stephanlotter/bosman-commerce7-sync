/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using System.Collections;
using DevExpress.Data.Filtering;

namespace BosmanCommerce7.Module.Extensions {
  public static class CriteriaOperatorFunctions {

    public static CriteriaOperator InCriteriaOperator(this string propertyName, params object[] values) => new InOperator(propertyName, values);

    public static CriteriaOperator IsEqualOidOperator(this int oid) => "Oid".IsEqualToOperator(oid);

    public static CriteriaOperator IsEqualOidOperator(this Guid oid) => "Oid".IsEqualToOperator(oid);

    public static CriteriaOperator IsEqualToOperator(this string propertyName, object value) => new BinaryOperator(propertyName, value);

    public static CriteriaOperator IsGreaterThanOperator(this string propertyName, object value) => new BinaryOperator(propertyName, value, BinaryOperatorType.Greater);

    public static CriteriaOperator IsGreaterOrEqualOperator(this string propertyName, object value) => new BinaryOperator(propertyName, value, BinaryOperatorType.GreaterOrEqual);

    public static CriteriaOperator IsNotEqualToOperator(this string propertyName, object value) => new BinaryOperator(propertyName, value).Not();

    public static CriteriaOperator IsNotNullOperator(this string propertyName) => new NullOperator(propertyName).Not();

    public static CriteriaOperator IsNullOperator(this string propertyName) => new NullOperator(propertyName);

    public static CriteriaOperator? MapArrayToInOperator(this string propertyName, object[] values) {
      if (values == null || values.Length == 0) {
        return null;
      }

      return new InOperator(propertyName, values);
    }

    public static CriteriaOperator? MapArrayToInOperator<T>(this string propertyName, T[] values) {
      if (values == null || values.Length == 0) {
        return null;
      }

      return new InOperator(propertyName, values);
    }

    public static CriteriaOperator? MapArrayToInOperator(this string propertyName, int[] values) {
      if (values == null || values.Length == 0) {
        return null;
      }

      return new InOperator(propertyName, values);
    }

    public static ConstantValue MapToConstantValue(this string value) => new(value);

    public static CriteriaOperator? MapToInOperator(this string propertyName, params object[] values) {
      if (values == null || values.Length == 0) {
        return null;
      }

      return new InOperator(propertyName, (IEnumerable)values.Select(o => new OperandValue(o)));
    }

    public static OperandProperty MapToPropertyOperand(this string propertyName) => new(propertyName);

    public static CriteriaOperator NotInCriteriaOperator(this string propertyName, params object[] values) => new InOperator(propertyName, values).Not();

    public static CriteriaOperator PropertyDateTypeHasValue(this string propertyName) => new BinaryOperator(propertyName, new DateTime(2000, 1, 1), BinaryOperatorType.Greater);

    public static CriteriaOperator PropertyFalse(this string propertyName) => propertyName.IsEqualToOperator(false);

    public static CriteriaOperator PropertyLessThan(this string propertyName, object value) => new BinaryOperator(propertyName, value, BinaryOperatorType.Less);

    public static CriteriaOperator PropertyNotEqualTo(this string propertyName, object value) => new BinaryOperator(propertyName, value).Not();

    public static CriteriaOperator PropertyNotStartWith(this string propertyName, string startValue) => propertyName.PropertyStartsWith(startValue).Not();

    public static CriteriaOperator PropertyStartsWith(this string propertyName, string startValue) =>
      propertyName
        .MapToPropertyOperand()
        .StartsWith(startValue.MapToConstantValue());

    public static CriteriaOperator PropertyTrue(this string propertyName) => propertyName.IsEqualToOperator(true);

    public static CriteriaOperator StartsWith(this CriteriaOperator thisCriteriaOperator, CriteriaOperator startsWithCriteriaOperator) =>
      new FunctionOperator(FunctionOperatorType.StartsWith,
        thisCriteriaOperator,
        startsWithCriteriaOperator);

    public static CriteriaOperator StringCriteria(this string propertyName, string value) => CriteriaOperator.Parse($"lower({propertyName}) = lower('{value}')");

    public static string EnumOperand<T>(T value) {
      return $"##Enum#{typeof(T).FullName},{value}#";
    }

    public static string ToCriteriaString(this CriteriaOperator criteriaOperator) => criteriaOperator.LegacyToString();
  }

}
