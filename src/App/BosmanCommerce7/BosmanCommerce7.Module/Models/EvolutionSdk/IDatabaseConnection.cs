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

  public interface IDatabaseConnection {

    SqlConnection Connection { get; }

    SqlTransaction Transaction { get; }
  }

}
