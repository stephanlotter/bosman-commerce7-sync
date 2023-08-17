echo off
cls

call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsDevCmd.bat"

set project_root=..\App\<******** path-to-project... ***********>
set project_name=<****** SampleProject (without .csproj) *************>
set nuget_repo_folder=C:\Gdrive\Business\Neurasoft\Development\LocalNuGetFeed\<****** path-to-repo-folder-for-package *************>

set output_root=bin\Debug\

cd %project_root%

rmdir /S /Q bin
rmdir /S /Q obj

msbuild %project_name%.csproj /t:Clean,Build
msbuild %project_name%.csproj /p:TargetFrameworkVersion=v4.6.1;OutDir=%output_root%\net461 /t:Clean,Build

:nuget
nuget pack %project_name%.csproj
if not exist "%nuget_repo_folder%" mkdir "%nuget_repo_folder%"
copy %project_name%.*.nupkg %nuget_repo_folder%
del %project_name%.*.nupkg

:end
pause
echo on
