param(
  [string]$zipFileName
)

$destination = "..\production\bin"
$service_name = "BosmanCommerce7"

# Call the common logic script
. .\CommonLogic.ps1 -zipFileName $zipFileName -destination $destination -service_name $service_name
