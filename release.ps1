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

# 7.5. Generate Flathub Manifest (Automated)
Write-Host "Generating Flathub submission manifest..." -ForegroundColor Cyan

$LinTarPath = "Releases\$LinTarName"
# Calculate the actual hash of the file we just built
$Sha256 = (Get-FileHash $LinTarPath -Algorithm SHA256).Hash.ToLower()
$DownloadUrl = "https://github.com/Axeia/qBittorrentCompanion/releases/download/v$Version/$LinTarName"

# Load the local YML (the one with 'type: dir')
$YamlContent = Get-Content "io.github.axeia.qBittorrentCompanion.yml" -Raw

# This Regex finds 'sources:' and EVERYTHING after it, replacing it with the Flathub-ready block
$NewSourceBlock = @"
    sources:
      - type: archive
        url: $DownloadUrl
        sha256: $Sha256
"@

# Replace the old sources section with the new one
$FlathubYaml = $YamlContent -replace '(?s)sources:.*', $NewSourceBlock

# Save to a new file so that the local testing YML doesn't get ovewriten
$FlathubYaml | Set-Content "Releases/io.github.axeia.qBittorrentCompanion.flathub.yml" -NoNewline

Write-Host "Flathub manifest generated: Releases/io.github.axeia.qBittorrentCompanion.flathub.yml" -ForegroundColor Green

# 8. Done at this point, show informative message
Write-Host "`n--- Done! ---" -ForegroundColor Cyan
Write-Host "Upload these 5 files to GitHub:" -ForegroundColor White
Write-Host "1. qBittorrentCompanion-v$Version-win-installer-x64.exe" -ForegroundColor Gray
Write-Host "2. $WinZipName"                                          -ForegroundColor Gray
Write-Host "3. $LinTarName"                                          -ForegroundColor Gray
Write-Host "4. RELEASES"                                             -ForegroundColor Gray
Write-Host "5. qBittorrentCompanion.$Version.nupkg"                  -ForegroundColor Gray

# 9. Automatic GitHub Release (Requires GitHub CLI 'gh' installed)
$GitHubConfirm = Read-Host "Create GitHub Release v$Version (upload) automatically? (y/n)"
if ($GitHubConfirm -eq "y") {
    Write-Host "Creating GitHub Release..." -ForegroundColor Magenta
    $ReleaseNotes = "Release v$Version"
    # Assets are gathered directly from the Releases folder
    gh release create "v$Version" (Get-ChildItem "Releases\*") --title "v$Version" --notes $ReleaseNotes
}
else {
    Write-Host "GitHub Release skipped." -ForegroundColor Yellow
}

# 10. WinGet Submission (Streamlined)
if (Get-Command "wingetcreate" -ErrorAction SilentlyContinue) {
    $WingetConfirm = Read-Host "Submit v$Version to WinGet? (y/n)"
    if ($WingetConfirm -eq "y") {
        Write-Host "Waiting for GitHub to process assets..." -ForegroundColor Gray
        Start-Sleep -Seconds 5

        $GitHubUser = "Axeia"
        $Repo = "qBittorrentCompanion"
        $FileName = "qBittorrentCompanion-v$Version-win-installer-x64.exe"
        $Url = "https://github.com/$GitHubUser/$Repo/releases/download/v$Version/$FileName"
        
        # Package ID as per PR #348198
        $PackageId = "qBittorrentCompanion.qBittorrentCompanion"
    
        Write-Host "Updating WinGet manifest..." -ForegroundColor Yellow
        wingetcreate update $PackageId --version $Version --urls $Url --submit
    }
}
else {
    Write-Warning "Skipped winget submission: 'wingetcreate' is not installed on this machine."
}

# 11. Local Flatpak Test Build (Optional/Experimental)
$FlatpakConfirm = Read-Host "Build local Flatpak for testing? (Requires WSL/flatpak-builder) (y/n)"
if ($FlatpakConfirm -eq "y") {
    Write-Host "Cleaning line endings and prepping WSL..." -ForegroundColor Gray
    
    # Ensure the icon is in the build folder first
    $IconDest = "$LinBuildDir/qBittorrentCompanion/Assets"
    if (-not (Test-Path $IconDest)) { New-Item -ItemType Directory -Path $IconDest -Force }
    Copy-Item "qBittorrentCompanion/Assets/qbc-logo.svg" -Destination $IconDest

    # Fix CRLF for Linux and save directly to build folder
    (Get-Content "io.github.axeia.qBittorrentCompanion.desktop") -join "`n" | Set-Content "$LinBuildDir/io.github.axeia.qBittorrentCompanion.desktop" -NoNewline
    (Get-Content "io.github.axeia.qBittorrentCompanion.metainfo.xml") -join "`n" | Set-Content "$LinBuildDir/io.github.axeia.qBittorrentCompanion.metainfo.xml" -NoNewline

    $WslWinPath = "/mnt/c/" + (Get-Location).Path.Substring(3).Replace('\', '/')
    $WslInternalPath = "/tmp/qbc-build"

    # 1. Build the Flatpak (one liner to avoid line ending issues)
    $BashCommand = "rm -rf $WslInternalPath && mkdir -p $WslInternalPath && " +
                   "cp -r '$WslWinPath/build/linux-x64/.' $WslInternalPath/ && " +
                   "cp '$WslWinPath/io.github.axeia.qBittorrentCompanion.yml' $WslInternalPath/ && " +
                   "cd $WslInternalPath && " +
                   "flatpak-builder --user --install --force-clean build-dir io.github.axeia.qBittorrentCompanion.yml"

    wsl bash -c "$BashCommand"

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Success! Attempting to launch..." -ForegroundColor Green
    
        $uid = (wsl id -u).Trim()
    
        # Convert the build path to WSL format
        $AbsoluteWinPath = (Get-Item $LinBuildDir).FullName
        $WslBinaryPath = "/mnt/c/" + $AbsoluteWinPath.Substring(3).Replace('\', '/') + "/qBittorrentCompanion.Desktop"
    
        wsl bash -c "DISPLAY=:0 WAYLAND_DISPLAY=wayland-0 XDG_RUNTIME_DIR=/run/user/$uid AVALONIA_USE_WAYLAND=1 '$WslBinaryPath'"
    }
}