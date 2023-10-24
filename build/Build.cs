/* 
 * Copyright (C) Neurasoft Consulting cc.  All rights reserved.
 * www.neurasoft.co.za
 * Date created: 2023-08-22
 * Author	: Stephan J Lotter
 * Notes	: 
 *  
 */

using System.IO;
using System.IO.Compression;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;

class Build : NukeBuild {
  [Solution]
  readonly Solution Solution;

  public static int Main() {
    Logging.Level = LogLevel.Normal;
    return Execute<Build>(x => x.Publish);
  }

  [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
  readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

  [Parameter("Do not bump the project versions.")]
  readonly bool DoNotBumpVersion;

  [Parameter("Do not publish to FTP")]
  readonly bool DoNotFtp;

  private readonly ProjectVersionManager _projectVersionManager = new();

  private Project MainProject => Solution.AllProjects.FirstOrDefault(a => a.Name == "BosmanCommerce7.Blazor.Server");

  private AbsolutePath OutputRootDirectory => @"C:\_ProjectDependencies\Bosman\Commerce7";

  private AbsolutePath PublishRootDirectory => OutputRootDirectory / "publish";

  string VersionString => _projectVersionManager.Version.ToString();

  string ZipFileName => $"BosmanCommerce7-{VersionString}.zip";

  AbsolutePath AbsoluteZipFileName => OutputRootDirectory / ZipFileName;

  Target Clean => _ => _
      .Executes(() => {
        PublishRootDirectory.CreateOrCleanDirectory();
      });

  Target BumpVersion => _ => _
      .Executes(() => {
        _projectVersionManager.ReadProjectVersion(MainProject);
        if (DoNotBumpVersion) { return; }

        _projectVersionManager.BumpMinor();

        Log.Information($"New version: {_projectVersionManager.Version}");

        var projects = Solution.AllProjects.Where(a => a.Name != "_build");
        _projectVersionManager.SetProjectVersion(projects);
      });

  Target Compile => _ => _
        .DependsOn(Clean, BumpVersion)
        .Executes(() => {

          DotNetTasks.DotNetBuild(_ => _
            .SetProjectFile(MainProject)
            .SetOutputDirectory(PublishRootDirectory)
            );

        });

  Target RemoveUnwantedOutput => _ => _
      .DependsOn(Compile)
      .Executes(() => {
        Log.Debug("Delete unwanted files");

        void DeleteFile(string filename) { (PublishRootDirectory / filename).DeleteFile(); }
        void RenameFile(string filename, string newName) {
          if ((PublishRootDirectory / filename).FileExists()) { (PublishRootDirectory / filename).Rename(newName); }
        }

        DeleteFile("appsettings.Development.json");
        RenameFile("appsettings.json", "appsettings-sample.json");

      });

  Target AddSupportingFiles => _ => _
      .DependsOn(Compile)
      .Executes(() => {

        var sourceDirectory = RootDirectory / @"src\Deploy\install-files";
        var installFiles = (sourceDirectory).GlobFiles("*.*");

        installFiles.ForEach(f => FileSystemTasks.CopyFileToDirectory(f, PublishRootDirectory));

      });

  Target ZipOutput => _ => _
      .DependsOn(RemoveUnwantedOutput, AddSupportingFiles)
      .Executes(() => {
        var versionString = _projectVersionManager.Version.ToString();

        var zipFile = AbsoluteZipFileName;
        zipFile.DeleteFile();

        PublishRootDirectory.ZipTo(
            zipFile,
            compressionLevel: CompressionLevel.SmallestSize,
            fileMode: FileMode.CreateNew);
      });

  Target Publish => _ => _
      .DependsOn(ZipOutput)
      .Executes(() => {

      });

}
