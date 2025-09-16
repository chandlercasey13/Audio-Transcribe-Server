# Ensure site directory exists (CodeDeploy copies after BeforeInstall)
$siteDir = "C:\inetpub\audioserver"
New-Item -ItemType Directory -Force -Path $siteDir | Out-Null

# (Optional) place any transforms/permissions here
Write-Host "AfterInstall completed for '$siteDir'"
