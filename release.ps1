# --- 1. Configuration & Dependency Checks ---
$ProjectSubFolder = "qBittorrentCompanion.Desktop"
$ProjectFile = "$ProjectSubFolder\qBittorrentCompanion.Desktop.csproj"
$LinBuildDir = "build\linux-x64"

# Extract version from csproj
[xml]$csproj = Get-Content $ProjectFile
$Version = $csproj.Project.PropertyGroup.Version[0]

Write-Host "--- Starting Verified Pipeline for v$Version ---" -ForegroundColor Cyan

# Check for Flathub Fork Path Early
$ParentDir = Get-Item ".." 
$FlathubRepoDir = Join-Path $ParentDir.FullName "flathub-submission"
$SubmissionYamlPath = Join-Path $FlathubRepoDir "flathub\io.github.axeia.qBittorrentCompanion.yml"

if (-not (Test-Path $SubmissionYamlPath)) {
    Write-Host "CRITICAL: Flathub submission manifest not found at $SubmissionYamlPath" -ForegroundColor Red
    $choice = Read-Host "Would you like to stop and fix the path? (y) or proceed without Flathub sync? (n)"
    if ($choice -eq "y") { exit }
}

# Check for required CLIs
$RequiredTools = @("gh", "wingetcreate", "git", "wsl")
foreach ($tool in $RequiredTools) {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) {
        Write-Warning "Tool '$tool' not found. Some deployment steps will fail."
    }
}

# --- 2. Build Phase ---
Write-Host "[1/5] Building Binaries..." -ForegroundColor Yellow

# 1. Clean out the old junk
$ReleaseFolders = "build", "Releases", "$ProjectSubFolder/bin", "$ProjectSubFolder/obj"
foreach ($f in $ReleaseFolders) { if (Test-Path $f) { Remove-Item $f -Recurse -Force -ErrorAction SilentlyContinue } }

# 2. The Build
# We move INTO the folder to solve the "Ambiguous" naming error
Push-Location $ProjectSubFolder

# We use the absolute path for output to ensure it lands in the root 'build' folder
$FullOutputPath = "$PSScriptRoot\$LinBuildDir"

dotnet publish "qBittorrentCompanion.Desktop.csproj" `
    -c Release `
    -r linux-x64 `
    -f net8.0 `
    -o "$FullOutputPath" `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:DebugType=None

Pop-Location

if ($LASTEXITCODE -ne 0) { Write-Error "Build failed!"; exit }

# 3. Rename the binary for Linux
# Rename-Item: 1st arg is Path, 2nd arg is just the NEW NAME (no path)
$BinaryPath = Join-Path $PSScriptRoot "$LinBuildDir\qBittorrentCompanion.Desktop"
if (Test-Path $BinaryPath) {
    Rename-Item -Path $BinaryPath -NewName "qBittorrentCompanion" -Force
}

# --- 3. Prep & Test Launch ---
Write-Host "[2/5] Launching for Manual Verification..." -ForegroundColor Yellow

# Copy metadata/icon to root of build dir
Copy-Item "qBittorrentCompanion/Assets/qbc-logo.svg" -Destination "$LinBuildDir/qbc-logo.svg"
(Get-Content "io.github.axeia.qBittorrentCompanion.desktop") -join "`n" | Set-Content "$LinBuildDir/io.github.axeia.qBittorrentCompanion.desktop" -NoNewline
(Get-Content "io.github.axeia.qBittorrentCompanion.metainfo.xml") -join "`n" | Set-Content "$LinBuildDir/io.github.axeia.qBittorrentCompanion.metainfo.xml" -NoNewline

# Build local Flatpak in WSL
$WslWinPath = "/mnt/c/" + (Get-Location).Path.Substring(3).Replace('\', '/')
$WslInternalPath = "/tmp/qbc-build"
$WslYmlPath = "$WslWinPath/$ProjectSubFolder/flatpak/io.github.axeia.qBittorrentCompanion.yml"

$BuildCmd = "rm -rf $WslInternalPath && mkdir -p $WslInternalPath && " +
            "cp -r '$WslWinPath/$($LinBuildDir.Replace('\', '/'))/.' $WslInternalPath/ && " +
            "cp '$WslYmlPath' $WslInternalPath/io.github.axeia.qBittorrentCompanion.yml && " +
            "cd $WslInternalPath && flatpak-builder --user --install --force-clean build-dir io.github.axeia.qBittorrentCompanion.yml"

wsl bash -c "$BuildCmd"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Flatpak built successfully. Launching binary directly for UI check..." -ForegroundColor Green

    $BinaryWslPath = "$WslWinPath/$($LinBuildDir.Replace('\', '/'))/qBittorrentCompanion"
    $LaunchCmd = "DISPLAY=:0 WAYLAND_DISPLAY=wayland-0 XDG_RUNTIME_DIR=/run/user/1000 AVALONIA_USE_WAYLAND=1 `"$BinaryWslPath`""
    $AppJob = Start-Job -ScriptBlock { param($cmd) wsl bash -c $cmd } -ArgumentList $LaunchCmd
    
    $DebugDisplay = "wsl bash -c 'DISPLAY=:0 WAYLAND_DISPLAY=wayland-0 XDG_RUNTIME_DIR=/run/user/1000 AVALONIA_USE_WAYLAND=1 `"$BinaryWslPath`"'"

    Write-Host "`n************************************************" -ForegroundColor Cyan
    Write-Host " App launched! Check your taskbar for the icon. " -ForegroundColor White
    Write-Host " Manual Debug Line (if needed):                 " -ForegroundColor Gray
    Write-Host " $DebugDisplay "                                  -ForegroundColor Gray
    Write-Host " PRESS ANY KEY in this window to KILL the app. " -ForegroundColor White
    Write-Host "************************************************`n" -ForegroundColor Cyan

    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    
    wsl pkill -f "qBittorrentCompanion"
    Stop-Job $AppJob; Remove-Job $AppJob
} else {
    Write-Error "Flatpak build failed."; exit
}

# --- 4. The Decision Gate ---
$Confirm = Read-Host "App killed. Proceed with PUBLIC RELEASE v$Version? (y/n)"
if ($Confirm -ne "y") { exit }

# --- 5. Deployment ---
Write-Host "[3/5] Creating Git Tag and GitHub Release..." -ForegroundColor Magenta
git tag "v$Version"
git push origin "v$Version"

if (-not (Test-Path "Releases")) { New-Item -ItemType Directory -Path "Releases" }
tar -czf "Releases/qBittorrentCompanion-v$Version-linux-x64.tar.gz" -C "$LinBuildDir" .
gh release create "v$Version" (Get-ChildItem "Releases\*") --title "v$Version" --notes "Release v$Version"

# --- 6. Automated Flathub PR Update ---
#if (Test-Path $SubmissionYamlPath) {
#    $FlathubConfirm = Read-Host "Submit update to Flathub PR? (y/n)"
#    if ($FlathubConfirm -eq "y") {
#        Write-Host "[4/5] Updating Flathub Fork..." -ForegroundColor Cyan
#        $LocalYmlPath = "$ProjectSubFolder\flatpak\io.github.axeia.qBittorrentCompanion.yml"
#        
#        if (Test-Path $LocalYmlPath) {
#            $BaseYaml = Get-Content $LocalYmlPath -Raw
#            $UpdatedYaml = $BaseYaml -replace 'tag: v[\d\.]+', "tag: v$Version"
#            
#            Push-Location (Split-Path $SubmissionYamlPath)
#            $UpdatedYaml | Set-Content (Split-Path $SubmissionYamlPath -Leaf) -NoNewline
#            
#            git add .
#            git commit -m "Update to v$Version"
#            git push origin submission-qbc-v2
#            Pop-Location
#            Write-Host "Flathub Fork updated successfully." -ForegroundColor Green
#        }
#    }
#}

# --- 7. WinGet Update ---
Write-Host "[5/5] Updating WinGet..." -ForegroundColor Yellow
$Url = "https://github.com/Axeia/qBittorrentCompanion/releases/download/v$Version/qBittorrentCompanion-v$Version-win-installer-x64.exe"
wingetcreate update qBittorrentCompanion.qBittorrentCompanion --version $Version --urls $Url --submit

Write-Host "`n--- Pipeline Complete! ---" -ForegroundColor Cyan