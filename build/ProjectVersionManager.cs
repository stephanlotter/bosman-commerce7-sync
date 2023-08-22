/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-22
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using Nuke.Common.ProjectModel;

public class ProjectVersionManager {
  public VersionDescriptor Version { get; private set; } = new("");

  public void ReadProjectVersion(Project project) {
    Version = GetProjectVersion(project);
  }

  public void BumpMinor() {
    Version = Version with { Minor = Version.Minor + 1 };
  }

  public VersionDescriptor GetProjectVersion(Project project) {
    var msBuildProject = project.GetMSBuildProject();
    var assemblyVersion = msBuildProject.GetProperty("AssemblyVersion");
    return new VersionDescriptor(assemblyVersion.EvaluatedValue);
  }

  public void SetProjectVersion(Project project) {
    SetProjectVersion(project, Version);
  }

  public void SetProjectVersion(Project project, VersionDescriptor Version) {
    var msBuildProject = project.GetMSBuildProject();
    msBuildProject.SetProperty("AssemblyVersion", Version.ToString());
    msBuildProject.SetProperty("FileVersion", Version.ToString());
    msBuildProject.Save();
  }

  public void SetProjectVersion(System.Collections.Generic.IEnumerable<Project> projects) {
    foreach (var e in projects) { SetProjectVersion(e); }
  }

}