# --- 1. Configuration & Dependency Checks ---
$ProjectSubFolder = "qBittorrentCompanion.Desktop"
$ProjectFile = "$ProjectSubFolder\qBittorrentCompanion.Desktop.csproj"
$LinBuildDir = "build\linux-x64"
$WinBuildDir = "build\win-x64"
$WinInstallerWorkDir = "build\win-installer"
$RepoUrl = "https://github.com/Axeia/qBittorrentCompanion"
$WinMainExe = "qBittorrentCompanion.Desktop.exe"

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
$RequiredTools = @("gh", "wingetcreate", "git", "wsl", "vpk")
foreach ($tool in $RequiredTools) {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) {
        Write-Warning "Tool '$tool' not found. Some deployment steps will fail."
    }
}

# --- 2. Build Phase ---
Write-Host "[1/6] Building Binaries..." -ForegroundColor Yellow

# 1. Clean out the old junk
$ReleaseFolders = "build", "Releases", "$ProjectSubFolder/bin", "$ProjectSubFolder/obj"
foreach ($f in $ReleaseFolders) { if (Test-Path $f) { Remove-Item $f -Recurse -Force -ErrorAction SilentlyContinue } }

# 2. The Linux Build
# We move INTO the folder to solve the "Ambiguous" naming error
Push-Location $ProjectSubFolder

# Absolute paths for output to ensure it lands in the root 'build' folder
$FullLinOutputPath = "$PSScriptRoot\$LinBuildDir"
$FullWinOutputPath = "$PSScriptRoot\$WinBuildDir"

dotnet publish "qBittorrentCompanion.Desktop.csproj" `
    -c Release `
    -r linux-x64 `
    -f net10.0 `
    -o "$FullLinOutputPath" `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:DebugType=None

if ($LASTEXITCODE -ne 0) { Pop-Location; Write-Error "Linux build failed!"; exit 1 }

# 3. The Windows Build
dotnet publish "qBittorrentCompanion.Desktop.csproj" `
    -c Release `
    -r win-x64 `
    -f net10.0 `
    -o "$FullWinOutputPath" `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:DebugType=None

if ($LASTEXITCODE -ne 0) { Pop-Location; Write-Error "Windows build failed!"; exit 1 }

Pop-Location

# 4. Rename the binary for Linux
# Rename-Item: 1st arg is Path, 2nd arg is just the NEW NAME (no path)
$BinaryPath = Join-Path $PSScriptRoot "$LinBuildDir\qBittorrentCompanion.Desktop"
if (Test-Path $BinaryPath) {
    Rename-Item -Path $BinaryPath -NewName "qBittorrentCompanion" -Force
}

# --- 3. Prep & Test Launch ---
Write-Host "[2/6] Launching for Manual Verification..." -ForegroundColor Yellow

# Copy metadata/icon to root of build dir
Copy-Item "qBittorrentCompanion/Assets/qbc-logo.svg" -Destination "$LinBuildDir/qbc-logo.svg"
(Get-Content "io.github.axeia.qBittorrentCompanion.desktop") -join "`n" | Set-Content "$LinBuildDir/io.github.axeia.qBittorrentCompanion.desktop" -NoNewline
(Get-Content "io.github.axeia.qBittorrentCompanion.metainfo.xml") -join "`n" | Set-Content "$LinBuildDir/io.github.axeia.qBittorrentCompanion.metainfo.xml" -NoNewline

# Build local Flatpak in WSL
$CurrentPath = (Get-Location).Path
$DriveLetter = $CurrentPath.Substring(0, 1).ToLower()
$WslWinPath = "/mnt/$DriveLetter/" + $CurrentPath.Substring(3).Replace('\', '/')
$WslLinBuildDir = "$WslWinPath/$($LinBuildDir.Replace('\', '/'))"
$WslInternalPath = "/tmp/qbc-build"
$WslYmlPath = "$WslWinPath/$ProjectSubFolder/flatpak/io.github.axeia.qBittorrentCompanion.yml"

$BuildCmd = "rm -rf $WslInternalPath && mkdir -p $WslInternalPath && " +
            "cp -r '$WslLinBuildDir/.' $WslInternalPath/ && " +
            "cp '$WslYmlPath' $WslInternalPath/io.github.axeia.qBittorrentCompanion.yml && " +
            "cd $WslInternalPath && flatpak-builder --user --install --force-clean build-dir io.github.axeia.qBittorrentCompanion.yml"

wsl bash -c "$BuildCmd"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Flatpak built successfully. Launching binary directly for UI check..." -ForegroundColor Green

    $BinaryWslPath = "$WslLinBuildDir/qBittorrentCompanion"
    $LaunchCmd = "XDG_RUNTIME_DIR=/run/user/`$(id -u) `"$BinaryWslPath`""
    $AppJob = Start-Job -ScriptBlock { param($cmd) wsl bash -c $cmd } -ArgumentList $LaunchCmd

    $DebugDisplay = "wsl bash -c 'XDG_RUNTIME_DIR=/run/user/`$(id -u) `"$BinaryWslPath`"'"

    Write-Host "`n************************************************" -ForegroundColor Cyan
    Write-Host " App launched! Check your taskbar for the icon. " -ForegroundColor White
    Write-Host " Manual Debug Line (if needed):                 " -ForegroundColor Gray
    Write-Host " $DebugDisplay "                                  -ForegroundColor Gray
    Write-Host " PRESS ANY KEY in this window to KILL the app. " -ForegroundColor White
    Write-Host "************************************************`n" -ForegroundColor Cyan

    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

    wsl pkill -f "/qBittorrentCompanion"
    Stop-Job $AppJob -ErrorAction SilentlyContinue
    Remove-Job $AppJob -ErrorAction SilentlyContinue
} else {
    Write-Error "Flatpak build failed."; exit 1
}

# --- 4. The Decision Gate ---
$Confirm = Read-Host "App killed. Proceed with PUBLIC RELEASE v$Version? (y/n)"
if ($Confirm -ne "y") { exit }

# --- 5. Package & Deployment ---
Write-Host "[3/6] Packaging Release Archives & Installer..." -ForegroundColor Magenta

if (-not (Test-Path "Releases")) { New-Item -ItemType Directory -Path "Releases" | Out-Null }

# Package Linux build (untar and run).
# Done inside WSL so the binary reliably keeps its executable bit.
$LinTarName = "qBittorrentCompanion-v$Version-linux-x64.tar.gz"
$TarCmd = "chmod +x '$WslLinBuildDir/qBittorrentCompanion' && " +
          "tar -czf '$WslWinPath/Releases/$LinTarName' -C '$WslLinBuildDir' . && " +
          "tar -tvzf '$WslWinPath/Releases/$LinTarName' | grep -E 'rwxr-xr-x.* \./?qBittorrentCompanion$'"
wsl bash -c "$TarCmd"
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to create Linux archive, or binary is missing the executable bit!"; exit 1 }

# Package portable Windows build (unzip and use)
$WinZipPath = "Releases\qBittorrentCompanion-v$Version-win-x64.zip"
Compress-Archive -Path "$WinBuildDir\*" -DestinationPath $WinZipPath -Force

# Package Windows .exe installer via Velopack
$FullWinInstallerWorkDir = Join-Path $PSScriptRoot $WinInstallerWorkDir
$ExpectedInstallerPath = "Releases\qBittorrentCompanion-v$Version-win-installer-x64.exe"
$VpkSetupName = "qBittorrentCompanion-v$Version-win-installer-x64.exe"

New-Item -ItemType Directory -Path $FullWinInstallerWorkDir -Force | Out-Null

# Fetch the previous release so vpk can generate delta packages.
# Failure is fine (e.g. the very first release): vpk then just builds full packages.
vpk download github --repoUrl $RepoUrl -o "$FullWinInstallerWorkDir"
if ($LASTEXITCODE -ne 0) {
    Write-Warning "Could not download previous release, continuing without delta packages."
}

vpk pack `
    -u qBittorrentCompanion `
    -v $Version `
    -p "$FullWinOutputPath" `
    -e $WinMainExe `
    -o "$FullWinInstallerWorkDir" `
    --setupName "$VpkSetupName"

if ($LASTEXITCODE -ne 0) { Write-Error "Velopack packaging failed!"; exit 1 }

# Installer exe
$BuiltSetup = Get-ChildItem -Path $FullWinInstallerWorkDir -Filter "*installer*x64.exe" | Select-Object -First 1
if (-not $BuiltSetup) {
    $BuiltSetup = Get-ChildItem -Path $FullWinInstallerWorkDir -Filter "*Setup*.exe" | Select-Object -First 1
}
if ($BuiltSetup) {
    Copy-Item -Path $BuiltSetup.FullName -Destination $ExpectedInstallerPath -Force
} else {
    Write-Error "Could not locate built Windows .exe installer in $FullWinInstallerWorkDir!"; exit 1
}

# Update feed files the in-app updater needs:
#  - this version's full/delta .nupkg files (not the old ones vpk downloaded)
#  - the releases/assets JSON feeds (these list all versions)
Get-ChildItem -Path $FullWinInstallerWorkDir -File |
    Where-Object { ($_.Extension -eq ".nupkg" -and $_.Name -like "*-$Version-*") -or $_.Extension -eq ".json" } |
    Copy-Item -Destination "Releases" -Force

if (-not (Get-ChildItem "Releases" -Filter "*-$Version-*-full.nupkg")) {
    Write-Error "Velopack full package for v$Version is missing from Releases\!"; exit 1
}

Write-Host "[4/6] Creating Git Tag and GitHub Release..." -ForegroundColor Magenta
git tag "v$Version"
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to create git tag v$Version"; exit 1 }

git push origin "v$Version"
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to push tag v$Version"; exit 1 }

gh release create "v$Version" (Get-ChildItem "Releases\*") --title "v$Version" --notes "Release v$Version"
if ($LASTEXITCODE -ne 0) { Write-Error "Failed to create GitHub release v$Version"; exit 1 }

# --- 6. Automated Flathub PR Update ---
#if (Test-Path $SubmissionYamlPath) {
#    $FlathubConfirm = Read-Host "Submit update to Flathub PR? (y/n)"
#    if ($FlathubConfirm -eq "y") {
#        Write-Host "[5/6] Updating Flathub Fork..." -ForegroundColor Cyan
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
Write-Host "[6/6] Updating WinGet..." -ForegroundColor Yellow
$Url = "$RepoUrl/releases/download/v$Version/qBittorrentCompanion-v$Version-win-installer-x64.exe"
wingetcreate update qBittorrentCompanion.qBittorrentCompanion --version $Version --urls $Url --submit

Write-Host "`n--- Pipeline Complete! ---" -ForegroundColor Cyan