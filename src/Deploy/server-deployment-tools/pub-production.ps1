$source=".\Publish"
$destination="..\production\bin"

# Run .\7za.exe x -opublish .\BosmanCommerce7-1.14.0.0.zip

if (Test-Path $source\appsettings.json) {
  Remove-Item $source\appsettings.json
}

if(-not (Test-Path $destination)) {

    New-Item -ItemType Directory -Path $destination

}

Copy-Item "$source\*" "$destination\" -Recurse -Force
