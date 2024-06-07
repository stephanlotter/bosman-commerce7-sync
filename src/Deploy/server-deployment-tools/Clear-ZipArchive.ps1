
$archive_root = ".\Archive"

$current_location = Get-Location
try {

    Set-Location $archive_root

    $latest_zip = Get-ChildItem -Filter BosmanCommerce7-*.zip | Sort-Object -Descending { $_.Name } | Select-Object -f 2
    $latest_zip_names = $latest_zip | ForEach-Object { $_.Name }
    $latest_zip_names | ForEach-Object {  Write-Host "Latest zip: " $_ }

    $old_zip_files = Get-ChildItem -Filter BosmanCommerce7-*.zip | Where-Object { $latest_zip_names -notcontains $_.Name }

    foreach($f in $old_zip_files) {
        Write-Host "Delete old zip: " $f
        Remove-Item -Path $f
    }

}
finally {
    Set-Location $current_location
}
