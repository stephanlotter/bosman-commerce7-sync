/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using System.Data.Common;
using Dapper;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public abstract class EvolutionRepositoryBase {

    protected DbConnection Connection => DatabaseContext.DBConnection;

    protected DbTransaction Transaction => DatabaseContext.DBTransaction;

    protected int? GetId(string sql, object param = null) {
      return Connection.QueryFirstOrDefault<int?>(sql, param, transaction: Transaction);
    }

  }

}
