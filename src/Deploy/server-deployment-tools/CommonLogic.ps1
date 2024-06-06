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

### Publish logic

if (-not $zipFileName) {
  Write-Host "Please provide a zipFileName"
  Write-Host "  E.g.: unzip-to-publish.ps1 -zipFileName='BosmanCommerce7-1.14.0.0.zip'"
  return
}

Write-Host "Unzip $zipFileName to .\$($extractToPath)"

if (Test-Path .\$extractToPath) {
  Remove-Item -Force -Recurse .\$extractToPath
}

New-Item -ItemType Directory -Name publish

.\7za.exe x -opublish $zipFileName

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host ""
  Write-Host "Error. Publish aborted"
  Write-Host ""
  Write-Host ""
  return
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
      Write-Output "Service '$ServiceName' has been stopped."
    } else {
      Write-Output "Service '$ServiceName' exists but is not running."
    }
  } else {
    Write-Output "Service '$ServiceName' does not exist."
  }
}

function Start-ServiceIfExists {
  param (
    [Parameter(Mandatory=$true)]
    [string]$ServiceName
  )

  $service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
  if ($null -ne $service) {
    Start-Service -Name $ServiceName
    Write-Output "Service '$ServiceName' has been started."
  } else {
    Write-Output "Service '$ServiceName' does not exist."
  }
}

Stop-ServiceIfExists -ServiceName $service_name

Move-Item $zipFileName .\Archive

Write-Host "Source: $source"
Write-Host "Destination: $destination"

if (Test-Path $source\appsettings.json) {
  Remove-Item $source\appsettings.json
}

if (-not (Test-Path $destination)) {
  New-Item -ItemType Directory -Path $destination
}

Copy-Item "$source\*" "$destination\" -Recurse -Force

if ($LASTEXITCODE -ne 0) {
  Write-Host ""
  Write-Host ""
  Write-Host "Error. Service will not be started. Fix the error and rerun this script, or manually update the program and start the service."
  Write-Host ""
  Write-Host ""
  return
}

Start-ServiceIfExists -ServiceName $service_name
