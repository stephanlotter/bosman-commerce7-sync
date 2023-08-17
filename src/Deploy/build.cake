#addin nuget:?package=Cake.Git&version=0.21.0

using System.Text.RegularExpressions;

// Load other scripts.
#load "/DropBox/Neurasoft/Development/Cake/neurasoft-lib.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Full-deploy");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

public class BuildData
{
    private ICakeContext _context;

    public string Configuration { get; }
    public ConvertableDirectoryPath ProjectRootDir { get; }
    public ConvertableDirectoryPath RepositoryRootDir { get; }
    public ConvertableDirectoryPath SolutionRootDir { get; }
    public ConvertableFilePath SolutionFile { get; }
    public ConvertableDirectoryPath MainProjectRootDir { get; }
    public ConvertableFilePath AppConfigFile { get; }
    public ConvertableDirectoryPath BuildDir { get; }
    public ConvertableDirectoryPath PublishRootDir { get; }
    public ConvertableDirectoryPath PublishOutputDir { get; }
    public ConvertableDirectoryPath UpdatesRootDir { get; }
    public ConvertableFilePath WyBuildProjectFile { get; }
    public ProjectVersion ProjectVersion { get; set; }
    public string NewVersion => $"{ProjectVersion.NewVersion.Major}.{ProjectVersion.NewVersion.Minor}.{ProjectVersion.NewVersion.Patch}";

    public BuildData(ICakeContext context, string configuration)
    {
        _context = context;
        Configuration = configuration;
        ProjectRootDir = context.Directory("../..");
        RepositoryRootDir = ProjectRootDir;
        SolutionRootDir = context.Directory("???");
        SolutionFile = context.Directory(SolutionRootDir) + context.File("????.sln");
        MainProjectRootDir = context.Directory(SolutionRootDir) + context.Directory("?????");
        BuildDir = context.Directory(MainProjectRootDir) + context.Directory("bin");
        PublishRootDir = context.Directory("C:/_ProjectDependencies/??????");
        PublishOutputDir = context.Directory(PublishRootDir) + context.Directory("publish");
        AppConfigFile = context.File("??????.config");
        UpdatesRootDir = context.Directory("../Updates");
        WyBuildProjectFile = context.File("UpdatesProject.wyp");
        ProjectVersion = new ProjectVersion();
    }

    public ConvertableDirectoryPath WithPublishOutputDir(string subDirectory) {
        return PublishOutputDir + _context.Directory(subDirectory);
    }

    public ConvertableFilePath WithPublishOutputDir(ConvertableFilePath file) {
        return PublishOutputDir + file;
    }

}

Setup<BuildData>(setupContext => {
    return new BuildData(Context, 
        configuration: Argument("configuration", "Debug")
    );
});

Teardown<BuildData>((context, data) =>
{
    Information($"{target}: Completed.");
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does<BuildData>(data =>
{
    CleanDirectory(data.BuildDir);
    CleanDirectory(data.PublishOutputDir);
});

Task("BumpVersion")
    .Does<BuildData>(data =>
{   
    data.ProjectVersion = ProjectVersionFunctions.BumpPatchVersion(data.MainProjectRootDir);
});

Task("Build")
    .Does<BuildData>(data =>
{
      MSBuild(data.SolutionFile, settings => {
            settings.SetConfiguration(data.Configuration);
            settings.WithTarget("Clean,Build");
            //settings.WithProperty("DeployOnBuild", "true");
            //settings.WithProperty("PublishProfile", "FolderProfile");
            settings.SetMaxCpuCount(8);
            settings.SetVerbosity(Verbosity.Minimal);
        });
});

Task("Copy-all-dependencies")
    .Does<BuildData>(data =>
{
    // copy all missing files 
});

Task("Deploy-clean-publish-folder")
    .Does<BuildData>(data => {
        //IoFunctions.DeleteSubDirectories(data.PublishOutputDir, new [] {"bin/de", "bin/es", "bin/ja", "bin/ru"});
        //IoFunctions.AppendExtension(data.WithPublishOutputDir(data.AppConfigFile), ".original");
        //IoFunctions.DeleteFiles(data.PublishOutputDir, "*.xml");
        //IoFunctions.DeleteFile(data.WithPublishOutputDir(File("Readme-log4net.txt")));
        //IoFunctions.DeleteFile(data.WithPublishOutputDir(File("Readme-RadiusCSharp.Core.txt")));
    });

Task("Deploy-wy-update")
    .Does<BuildData>(data => {
        var wyUpdateDescriptor = new WyUpdateDescriptor(data.UpdatesRootDir, data.WyBuildProjectFile, data.ProjectVersion.NewVersion);
        wyUpdateDescriptor.CopyClientWycToVersionFolder = true;
        var versionFolder = wyUpdateDescriptor.VersionFolder;

        void CopyOutputFiles(string subDirectory, string pattern, string excludePattern = "") {
            bool IncludeFile (IFile file) {
                if(excludePattern == "") return true;
                return !Regex.IsMatch(file.Path.FullPath, excludePattern);
            }

            var files = GetFiles(data.PublishOutputDir + Directory(subDirectory) + Directory(pattern), 
                new GlobberSettings { 
                    FilePredicate = IncludeFile 
                });

            WyBuildFunctions.CopyBuildOutputToVersionFolder(files, versionFolder + Directory(subDirectory));
        }

        CopyOutputFiles("", "*.*");
        //CopyOutputFiles("Images", "*.*");
        //CopyOutputFiles("bin", "*.*", "DevExpress");

        WyBuildFunctions.Build(wyUpdateDescriptor);
    });

Task("Deploy-full-zip")
    .IsDependentOn("Deploy-wy-update")
    .Does<BuildData>(data => {
        WyBuildFunctions.CopyWyUpdateFiles(data.UpdatesRootDir, data.PublishOutputDir);

        var zipFile = data.PublishRootDir + 
            File($"full-{data.NewVersion}.zip");
        IoFunctions.DeleteFile(zipFile);
        Zip(data.PublishOutputDir, zipFile);
    });

Task("Deploy-hot-fix")
    .Does<BuildData>(data => {
        //var publishOutputDir = data.PublishOutputDir.Path;

        //var hotFixFilesOnly = GetFiles(publishOutputDir + "/*.*");
        //hotFixFilesOnly.Add(GetFiles(publishOutputDir + "/bin/Fusion2Client*.*"));
        //hotFixFilesOnly.Add(GetFiles(publishOutputDir + "/bin/RadiusCSharp*.*"));

        //var zipFileHotFix = data.PublishRootDir + 
        //    File($"hotfix-{DateTime.Now:yyyyMMdd_HHmmss}.zip");
        //DeleteFile(zipFileHotFix);
        //Zip(publishOutputDir, zipFileHotFix, hotFixFilesOnly);
    });

Task("CommitGit")
    .Does<BuildData>(data => {
        Neurasoft.WriteMessage("Commit update to git.");
        GitFunctions.CommitDeployment(data.RepositoryRootDir, data.ProjectVersion.NewVersion);
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Full-deploy")
    .IsDependentOn("Clean")
    //.IsDependentOn("BumpVersion")
    //.IsDependentOn("Build")
    //.IsDependentOn("Copy-all-dependencies")
    //.IsDependentOn("Deploy-clean-publish-folder")
    //.IsDependentOn("Deploy-wy-update")
    //.IsDependentOn("Deploy-full-zip")
    //.IsDependentOn("CommitGit")
    ;

Task("fix")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .IsDependentOn("Deploy-clean-publish-folder")
    .IsDependentOn("Deploy-hot-fix")
    ;

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);