param(
  [string]$zipFileName
)

$destination = "..\uat\bin"
$service_name = "BosmanCommerce7UAT"

# Call the common logic script
. .\CommonLogic.ps1 -zipFileName $zipFileName -destination $destination -service_name $service_name
