$pool   = "audio-site"
$appcmd = "$env:windir\system32\inetsrv\appcmd.exe"

# Stop only if the pool exists
& $appcmd list apppool /name:$pool *> $null
if ($LASTEXITCODE -eq 0) {
  Write-Host "Stopping app pool: $pool"
  & $appcmd stop apppool /apppool.name:$pool
} else {
  Write-Host "App pool '$pool' not found; skipping stop."
}
