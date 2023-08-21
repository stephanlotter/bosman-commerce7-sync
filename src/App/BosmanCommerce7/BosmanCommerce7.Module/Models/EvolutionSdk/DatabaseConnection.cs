/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using System.Data.SqlClient;

namespace BosmanCommerce7.Module.Models.EvolutionSdk {
  public class DatabaseConnection : IDatabaseConnection {

    public SqlConnection Connection { get; }

    public SqlTransaction Transaction { get; }

    public DatabaseConnection(SqlConnection connection, SqlTransaction transaction) {
      Connection = connection;
      Transaction = transaction;
    }
  }

}
