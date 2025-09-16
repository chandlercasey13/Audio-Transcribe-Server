$pool   = "audio-site"
$appcmd = "$env:windir\system32\inetsrv\appcmd.exe"

Write-Host "Starting app pool: $pool"
& $appcmd start apppool /apppool.name:$pool

# Drop a marker so you can confirm overwrites
$stamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
"Deployed at $stamp" | Out-File "C:\inetpub\audioserver\DEPLOYED.txt" -Encoding UTF8
