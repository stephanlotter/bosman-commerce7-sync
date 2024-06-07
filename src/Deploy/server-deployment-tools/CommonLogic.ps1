<#
.SYNOPSIS
  Common logic for handling Windows services and file operations.

.DESCRIPTION
  This script contains functions and logic for stopping and starting services,
  unzipping files, and copying files to a destination directory.

.NOTES
  Author     : Neurasoft Consulting CC
  Date       : 2024-06-06
#>

param(
  [string]$zipFileName,
  [string]$destination,
  [string]$service_name
)

$extractToPath = "publish"
$source = ".\$extractToPath"
$archive_root = ".\Archive"

#####################################################
# Functions
#####################################################
function WriteInfo {
    param($message)
    Write-Host $message
}

function WriteSuccess {
    param($message)
    Write-Host $message -ForegroundColor Green
}

function WriteWarn {
    param($message)
    Write-Host $message -ForegroundColor Yellow
}

function WriteError {
    param($message)
    Write-Host $message -ForegroundColor Red
}


function Stop-ServiceIfExists {
  param (
    [Parameter(Mandatory=$true)]
    [string]$ServiceName
  )

  $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
  if ($null -ne $service) {
    if ($service.Status -eq 'Running') {
      Stop-Service -Name $ServiceName -Force
      WriteSuccess "Service '$ServiceName' has been stopped."
    } else {
      WriteInfo "Service '$ServiceName' exists but is not running."
    }
  } else {
    WriteWarn "Service '$ServiceName' does not exist."
  }
}

function Start-ServiceIfExists {
  param (
    [Parameter(Mandatory=$true)]
    [string]$ServiceName
  )

  $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

  if($service.StartType -ne "Automatic") {
    WriteInfo "Service start type is not automatic. Service not started."
    return
  }

  if ($null -ne $service) {
    Start-Service -Name $ServiceName
    WriteSuccess "Service '$ServiceName' has been started."
  } else {
    WriteWarn "Service '$ServiceName' does not exist."
  }
}

#####################################################
# Unzip
#####################################################

if (-not $zipFileName) {
  WriteWarn "Please provide a zipFileName"
  WriteWarn "  E.g.: unzip-to-publish.ps1 -zipFileName='BosmanCommerce7-1.14.0.0.zip'"
  return
}

if (-not (Test-Path $zipFileName)) {
  WriteError "Zip file $zipFileName not found"
  return
}


WriteInfo "Unzip $zipFileName to .\$($extractToPath)"

if (Test-Path .\$extractToPath) {
  Remove-Item -Force -Recurse .\$extractToPath
}

New-Item -ItemType Directory -Name publish

.\7za.exe x -opublish $zipFileName

if ($LASTEXITCODE -ne 0) {
  WriteError ""
  WriteError ""
  WriteError "Error. Publish aborted"
  WriteError ""
  WriteError ""
  return
}

#####################################################
# Stop the service
#####################################################
Stop-ServiceIfExists -ServiceName $service_name


#####################################################
# Move zip file to Archive
#####################################################
Move-Item $zipFileName $archive_root

$current_location = Get-Location
try {

    Set-Location $archive_root

    $latest_zip = Get-ChildItem -Filter BosmanCommerce7-*.zip | Sort-Object -Descending { $_.Name } | Select-Object -f 2
    $latest_zip_names = $latest_zip | ForEach-Object { $_.Name }
    $latest_zip_names | ForEach-Object {  WriteInfo "Latest zip: " $_ }

    $old_zip_files = Get-ChildItem -Filter BosmanCommerce7-*.zip | Where-Object { $latest_zip_names -notcontains $_.Name }

    foreach($f in $old_zip_files) {
        WriteInfo "Delete old zip: " $f
        Remove-Item -Path $f
    }

}
finally {
    Set-Location $current_location
}


#####################################################
# Deploy files to bin
#####################################################
WriteInfo "Source: $source"
WriteInfo "Destination: $destination"

if (Test-Path $source\appsettings.json) {
  Remove-Item $source\appsettings.json
}

if (-not (Test-Path $destination)) {
  New-Item -ItemType Directory -Path $destination
}

Copy-Item "$source\*" "$destination\" -Recurse -Force

if ($LASTEXITCODE -ne 0) {
  WriteError ""
  WriteError ""
  WriteError "Error. Service will not be started. Fix the error and rerun this script, or manually update the program and start the service."
  WriteError ""
  WriteError ""
  return
}


#####################################################
# Start the service
#####################################################
Start-ServiceIfExists -ServiceName $service_name

WriteSuccess "Update complete"
