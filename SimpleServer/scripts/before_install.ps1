$siteDir    = "C:\inetpub\audioserver"
$backupRoot = "C:\inetpub\backups"
$stamp      = Get-Date -Format "yyyyMMdd_HHmmss"

# Ensure dirs exist
New-Item -ItemType Directory -Force -Path $siteDir | Out-Null
New-Item -ItemType Directory -Force -Path $backupRoot | Out-Null

# Backup current site (ok if first deploy and folder is empty)
$dst = Join-Path $backupRoot "audioserver_$stamp"
New-Item -ItemType Directory -Force -Path $dst | Out-Null
robocopy $siteDir $dst /MIR | Out-Null
Write-Host "Backed up '$siteDir' to '$dst'"
