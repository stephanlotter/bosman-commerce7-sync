param(
    [Parameter(Mandatory=$true)]
    [string]$inputFile
)

Clear-Host

$source=".\bin-uat"
$destination="..\UAT\bin"

.\7za.exe x -obin-uat $inputFile


if (Test-Path $source\appsettings.json) {
  Remove-Item $source\appsettings.json
}

Copy-Item "$source\*" "$destination\" -Recurse -Force
