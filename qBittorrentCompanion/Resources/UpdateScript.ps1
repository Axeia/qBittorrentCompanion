param(
    [Parameter(Mandatory=$true)][int]$ParentPid,
    [Parameter(Mandatory=$true)][string]$InstallPath,
    [Parameter(Mandatory=$true)][string]$ZipPath
)

# 1. Wait for the app to exit
$timeout = 10
while ((Get-Process -Id $ParentPid -ErrorAction SilentlyContinue) -and ($timeout -gt 0)) {
    Start-Sleep -Seconds 1
    $timeout--
}

# 2. Force kill if still hanging
if (Get-Process -Id $ParentPid -ErrorAction SilentlyContinue) {
    Stop-Process -Id $ParentPid -Force -ErrorAction SilentlyContinue
}

# 3. Extract and Overwrite
try {
    # Expand-Archive -Force is great for single-file overwrites
    Expand-Archive -Path $ZipPath -DestinationPath $InstallPath -Force
    
    # 4. Restart
    $ExePath = Join-Path $InstallPath "qBittorrentCompanion.Desktop.exe"
    Start-Process -FilePath $ExePath
}
catch {
    $Error[0] | Out-File (Join-Path $env:TEMP "qbc_update_error.txt")
}