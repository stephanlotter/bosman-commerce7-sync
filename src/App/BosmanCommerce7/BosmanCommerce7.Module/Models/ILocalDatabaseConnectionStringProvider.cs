﻿/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-17 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

namespace BosmanCommerce7.Module.Models {
  public interface ILocalDatabaseConnectionStringProvider {

    string? LocalDatabase { get; }

  }

}
