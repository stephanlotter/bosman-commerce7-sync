param(
    [string]$zipFileName
)

$extractToPath = "publish"

if(-not $zipFileName) {
    Write-Host "Please provide a zipFileName"
    Write-Host "  E.g.: unzip-to-publish.ps1 - zipFileName='BosmanCommerce7-1.14.0.0.zip'"
    return
}

Write-Host "Unzip $zipFileName to .\$($extractToPath)"

if(Test-Path .\$extractToPath) {
    Remove-Item -Force -Recurse .\$extractToPath
}

New-Item -ItemType Directory -Name publish

.\7za.exe x -opublish $zipFileName

