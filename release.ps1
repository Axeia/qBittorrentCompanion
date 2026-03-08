# 1. Configuration
$ProjectSubFolder = "qBittorrentCompanion.Desktop"
$ProjectFile = "$ProjectSubFolder\qBittorrentCompanion.Desktop.csproj"
$Configuration = "Release"

$WinRuntime = "win-x64"
$WinTFM = "net8.0-windows"
$LinRuntime = "linux-x64"
$LinTFM = "net8.0"

# 2. Extract version number from .csproj
[xml]$csproj = Get-Content $ProjectFile
$Version = $csproj.Project.PropertyGroup.Version[0]

Write-Host "--- Preparing release v$Version ---" -ForegroundColor Cyan

# 3. Clean Folders
$ReleaseFolders = "build", "Releases", "$ProjectSubFolder/bin/$Configuration", "$ProjectSubFolder/obj/$Configuration"
foreach ($f in $ReleaseFolders) {
    if (Test-Path $f) { 
        Write-Host "Cleaning $f..." -ForegroundColor Gray
        Remove-Item -Path $f -Recurse -Force -ErrorAction SilentlyContinue 
    }
}

# 4. Build Windows & Linux
Write-Host "Building Binaries..." -ForegroundColor Yellow
$WinBuildDir = "build\$WinRuntime"
$LinBuildDir = "build\$LinRuntime"

# Define as an array so PowerShell passes them correctly
$BuildFlags = @(
    "-p:PublishSingleFile=true",
    "-p:SelfContained=true",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-p:DebugType=None",
    "-p:DebugSymbols=false"
)

# Use the @ symbol to "splat" the array into the command
dotnet publish $ProjectFile -c $Configuration -r $WinRuntime -f $WinTFM -o $WinBuildDir @BuildFlags
dotnet publish $ProjectFile -c $Configuration -r $LinRuntime -f $LinTFM -o $LinBuildDir @BuildFlags

# 5. Velopack pack
Write-Host "Packaging Velopack..." -ForegroundColor Green
vpk pack -u "qBittorrentCompanion" -v $Version -p $WinBuildDir -e "qBittorrentCompanion.Desktop.exe"

# 6. Cleanup & rename velopack output
Write-Host "Standardizing & cleaning up releases folder..." -ForegroundColor Green
# Rename Setup.exe to a name similar to the previous regular releases (using a wildcard as Velopack's naming is not garantueed to stay the same)
Get-ChildItem "Releases\*Setup.exe" | Rename-Item -NewName "qBittorrentCompanion-v$Version-win-installer-x64.exe"
# Remove the redundant portable zip Velopack creates automatically (the regular build ought to be portable)
Get-ChildItem "Releases\*-Portable.zip" | Remove-Item -Force
# Remove Velopack json files not needed for distribution
Get-ChildItem "Releases\*.json" | Remove-Item -Force

# 7. Create compressed files
$WinZipName = "qBittorrentCompanion-v$Version-win-x64.zip"
Compress-Archive -Path "$WinBuildDir\*" -DestinationPath "Releases\$WinZipName" -Force

$LinTarName = "qBittorrentCompanion-v$Version-linux-x64.tar.gz"
# Check if WSL is available and has at least one distribution installed
$hasWsl = Get-Command wsl -ErrorAction SilentlyContinue
$hasDistro = if ($hasWsl) { wsl --list --quiet | Out-String } else { $null }

if ($hasWsl -and ![string]::IsNullOrWhiteSpace($hasDistro)) {
    Write-Host "Setting Linux permissions via WSL..." -ForegroundColor Yellow
    
    # Convert Windows path to WSL /mnt/c/ format
    $AbsoluteWinPath = (Get-Item $LinBuildDir).FullName
    $WslPath = "/mnt/c/" + $AbsoluteWinPath.Substring(3).Replace('\', '/')
    
    # Try to set permissions, but don't crash if it fails
    try {
        wsl chmod +x "$WslPath/qBittorrentCompanion.Desktop"
    } catch {
        Write-Warning "WSL call failed. Proceeding with standard tar."
    }
} else {
    Write-Host "WSL not found or no distro installed. Skipping permission tagging." -ForegroundColor Gray
}

Write-Host "Compressing Linux .tar.gz..." -ForegroundColor Yellow
tar -czf "Releases\$LinTarName" -C "$LinBuildDir" .

# 8. Done at this point, show informative message
Write-Host "`n--- Done! ---" -ForegroundColor Cyan
Write-Host "Upload these 5 files to GitHub:" -ForegroundColor White
Write-Host "1. qBittorrentCompanion-v$Version-win-installer-x64.exe" -ForegroundColor Gray
Write-Host "2. $WinZipName"                                          -ForegroundColor Gray
Write-Host "3. $LinTarName"                                          -ForegroundColor Gray
Write-Host "4. RELEASES"                                             -ForegroundColor Gray
Write-Host "5. qBittorrentCompanion.$Version.nupkg"                  -ForegroundColor Gray

# 9. Open the folder so things can be dragged to GitHub easily
explorer "Releases"