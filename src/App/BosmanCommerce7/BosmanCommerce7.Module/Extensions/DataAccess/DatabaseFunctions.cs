/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using System.Data.Common;
using System.Data.SqlClient;
using Polly;

namespace BosmanCommerce7.Module.Extensions.DataAccess {
  public static class DatabaseFunctions {

    public static T WrapInOpenConnection<T>(this string? connectionString, Func<SqlConnection, T> func) {
      if (connectionString is null) {
        throw new ArgumentNullException(nameof(connectionString));
      }

      var waitAndRetry = Policy
        .Handle<SqlException>()
        .WaitAndRetry(5, i => TimeSpan.FromSeconds(i * 5));

      return
        waitAndRetry
          .Execute(
            () => {
              using var connection = new SqlConnection(connectionString);
              try {
                connection.Open();
                return func(connection);
              }
              finally {
                connection.Close();
              }
            });
    }

    public static void WrapInTransaction(this string? connectionString, Action<SqlConnection, SqlTransaction> action) {
      if (connectionString is null) {
        throw new ArgumentNullException(nameof(connectionString));
      }

      var waitAndRetry =
        Policy
          .Handle<SqlException>()
          .WaitAndRetry(5, i => TimeSpan.FromSeconds(i * 5));

      waitAndRetry
        .Execute(
          () => {
            using var connection = new SqlConnection(connectionString);
            DbTransaction? transaction = null;
            try {
              connection.Open();
              transaction = connection.BeginTransaction();
              action(connection, (SqlTransaction)transaction);
              transaction.Commit();
            }
            catch {
              transaction?.Rollback();
              throw;
            }
            finally {
              connection.Close();
            }
          });
    }
  }
}
