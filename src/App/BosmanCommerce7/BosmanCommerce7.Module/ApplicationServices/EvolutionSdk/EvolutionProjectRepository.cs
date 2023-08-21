/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-21
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using CSharpFunctionalExtensions;
using Pastel.Evolution;

namespace BosmanCommerce7.Module.ApplicationServices.EvolutionSdk {
  public class EvolutionProjectRepository : EvolutionRepositoryBase, IEvolutionProjectRepository {

    public Result<Project> GetProject(string? projectCode) {
      int? id = GetId("select p.ProjectLink from Project p where lower(p.ProjectCode)=lower(@projectCode)", new { projectCode });

      if (id == null) {
        return Result.Failure<Project>($"Projecy with code {projectCode} not found");
      }

      return new Project(id.Value);
    }
  }

}
