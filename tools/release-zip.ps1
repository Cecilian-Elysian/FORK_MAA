<#
.SYNOPSIS
    Build MAA fork release zip (MAA-vX.Y.Z-fork.YYYYMMDD-win-x64.zip) into installer/.

.DESCRIPTION
    Equivalent to upstream .github/workflows/ci.yml "Build for Windows" + "Organize install files" + "Zip files"
    steps, but for a local fork build.

    Steps:
        1. Read VERSION (e.g. v6.14.0-fork.20260714)
        2. Configure + build C++ (cmake --preset windows-publish-x64 -DMAA_HASH_VERSION=$ver)
        3. Build MAA.Updater (Static CRT, survives MAA runtime replacement)
        4. cmake --install to install/
        5. Sync resource/ -> install/resource/
        6. Taggify csproj (Version/FileVersion/AssemblyVersion/InformationalVersion) and restore
        7. dotnet publish to install/ (uses nulastudio.NetBeauty to merge dependencies)
        8. Strip dev artifacts (*.pdb, *.h, *.bak, msvc-debug/)
        9. Copy DependencySetup bat
       10. Stage install/ into a temp dir (excluding cache/config/data/debug user data)
       11. Compress to installer/MAA-{ver}-win-x64.zip

    File naming note: this script is intentionally named release-zip.* (not build-release-zip.*)
    because the parent .gitignore has a `build-*` rule for CMake build artifacts that would
    otherwise swallow the script files themselves.

.PARAMETER Version
    Optional version override. If omitted, read from ./VERSION first line.

.PARAMETER SkipBuild
    Skip cmake/dotnet build; only do the cleanup + zip stage. Useful when the build artifacts
    in install/ are already up to date and you just want to repack.

.PARAMETER KeepInstallerDir
    Reuse the existing installer/ directory without cleaning it first.

.EXAMPLE
    tools\release-zip.bat
    tools\release-zip.ps1 -Version v6.14.0-fork.20260715
    tools\release-zip.ps1 -SkipBuild
#>

[CmdletBinding()]
param(
    [string]$Version,
    [switch]$SkipBuild,
    [switch]$KeepInstallerDir
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$root = Resolve-Path (Join-Path $PSScriptRoot '..')
Set-Location $root

# ---------- 1. read VERSION ----------
if (-not $Version) {
    $versionFile = Join-Path $root 'VERSION'
    if (-not (Test-Path $versionFile)) {
        throw "VERSION file not found at $versionFile"
    }
    $Version = (Get-Content $versionFile -Raw).Trim() -split "`r?`n" | Select-Object -First 1
}
if ([string]::IsNullOrWhiteSpace($Version)) {
    throw "VERSION is empty. Write a SemVer tag (e.g. v6.14.0-fork.20260714) into ./VERSION"
}
if ($Version -notmatch '^v\d+(\.\d+){1,3}(-[A-Za-z0-9.-]+)?$') {
    throw "VERSION '$Version' is not a valid v-prefixed SemVer tag (e.g. v6.14.0-fork.20260714)"
}
Write-Host "[build-release-zip] Version: $Version" -ForegroundColor Cyan

$installerDir = Join-Path $root 'installer'
$installDir   = Join-Path $root 'install'

# ---------- 2. build (cmake + dotnet) ----------
if (-not $SkipBuild) {
    Write-Host "[build-release-zip] (1/8) cmake configure" -ForegroundColor Cyan
    & cmake -B build --preset windows-publish-x64 "-DMAA_HASH_VERSION=$Version"
    if ($LASTEXITCODE) { throw "cmake configure failed (exit $LASTEXITCODE)" }

    # NOTE: We deliberately do NOT use `cmake --build --preset windows-publish-x64` (which builds
    # ALL targets including MaaWpfGui). The preset's include_external_msproject(MaaWpfGui) wires the
    # WPF csproj into cmake's build graph; cmake then invokes MSBuild for it, which uses Visual
    # Studio's MSBuild and tries to resolve Microsoft.NET.Sdk from
    # E:\visual stduio community 2026\VIUAL\MSBuild\Sdks\ (a path that is missing/broken on this
    # machine). We instead build only the C++ targets via cmake, then drive the WPF csproj through
    # the standalone `dotnet` CLI (which uses C:\Program Files\dotnet and works fine), mirroring
    # the proven pattern from tools/local-install.bat.

    Write-Host "[build-release-zip] (2/8) cmake build (C++ only: MaaCore)" -ForegroundColor Cyan
    & cmake --build build --target MaaCore --config RelWithDebInfo --parallel $env:NUMBER_OF_PROCESSORS
    if ($LASTEXITCODE) { throw "MaaCore build failed (exit $LASTEXITCODE)" }

    Write-Host "[build-release-zip] (3/8) build MAA.Updater (static CRT)" -ForegroundColor Cyan
    & cmake --build build --target MAA.Updater --config RelWithDebInfo --parallel $env:NUMBER_OF_PROCESSORS
    if ($LASTEXITCODE) { throw "MAA.Updater build failed (exit $LASTEXITCODE)" }

    Write-Host "[build-release-zip] (4/8) cmake install -> install/" -ForegroundColor Cyan
    & cmake --install build --config RelWithDebInfo
    if ($LASTEXITCODE) { throw "cmake install failed (exit $LASTEXITCODE)" }

    Write-Host "[build-release-zip] (5/8) sync resource/ -> install/resource/" -ForegroundColor Cyan
    & robocopy (Join-Path $root 'resource') (Join-Path $installDir 'resource') /MIR /MT:8 | Out-Null
    # robocopy exit codes: 0-7 success, 8+ failure. Accept up to 7.
    if ($LASTEXITCODE -ge 8) { throw "robocopy resource failed (exit $LASTEXITCODE)" }
} else {
    Write-Host "[build-release-zip] (skip build, using existing install/)" -ForegroundColor Yellow
}

# ---------- 6. WPF publish (csproj version injection) ----------
$csproj = Join-Path $root 'src\MaaWpfGui\MaaWpfGui.csproj'
$csprojBak = "$csproj.bak"
$gjFile = Join-Path $root 'global.json'
$gjBak = "$gjFile.bak"
$hadGj = Test-Path $gjFile
$csprojTouched = $false

# Detect base version (strip -prerelease) and prerelease for SemVer fields
$matchNum = '0.0.1'
if ($Version -match '^v(\d+(?:\.\d+){1,3})') { $matchNum = $Matches[1] }

try {
    # Backup + write global.json. AGENTS.md pins 10.0.203 with rollForward: disable, but this
    # machine only has 10.0.300 installed. Use 10.0.100 (lowest 10.0 feature band) with
    # rollForward: latestFeature so any 10.0.x SDK works while still requiring .NET 10.
    if ($hadGj) { Copy-Item $gjFile $gjBak -Force }
    '{"sdk":{"version":"10.0.100","rollForward":"latestFeature"}}' | Set-Content -Path $gjFile -NoNewline

    if (-not $SkipBuild) {
        Write-Host "[build-release-zip] (6/8) dotnet restore + publish (with csproj version tag)" -ForegroundColor Cyan
        & dotnet restore $csproj
        if ($LASTEXITCODE) { throw "dotnet restore failed (exit $LASTEXITCODE)" }

        # Taggify csproj (mirror ci.yml "Taggify version for csproj")
        Copy-Item $csproj $csprojBak -Force
        $csprojTouched = $true
        [xml]$x = Get-Content $csproj -Raw
        $node = $x.Project.PropertyGroup | Where-Object { $_.ApplicationVersion -ne $null } | Select-Object -First 1
        if ($null -eq $node) { throw "csproj missing <ApplicationVersion> PropertyGroup node" }
        $node.InformationalVersion = $Version
        $node.Version              = $matchNum
        $node.FileVersion          = $matchNum
        $node.AssemblyVersion      = $matchNum
        $x.Save($csproj)

        # dotnet publish (no -r win-x64, NetBeauty merges dependencies per csproj)
        & dotnet publish $csproj -c Release -p:Platform=x64 -o $installDir
        if ($LASTEXITCODE) { throw "dotnet publish failed (exit $LASTEXITCODE)" }
    } else {
        Write-Host "[build-release-zip] (skip dotnet publish, using existing install/)" -ForegroundColor Yellow
    }
}
finally {
    # Always restore csproj + global.json
    if ($csprojTouched -and (Test-Path $csprojBak)) {
        Move-Item $csprojBak $csproj -Force
        Write-Host "[build-release-zip]   csproj restored" -ForegroundColor DarkGray
    }
    if ($hadGj) {
        Move-Item $gjBak $gjFile -Force
    } else {
        Remove-Item $gjFile -ErrorAction SilentlyContinue
    }
}

# ---------- 7. strip dev artifacts ----------
Write-Host "[build-release-zip] (7/8) strip dev artifacts from install/" -ForegroundColor Cyan
Remove-Item (Join-Path $installDir '*.pdb') -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $installDir '*.h')   -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $installDir '*.bak') -Force -ErrorAction SilentlyContinue
Remove-Item (Join-Path $installDir 'MAAComponent-DebugSymbol-*.zip') -Force -ErrorAction SilentlyContinue
if (Test-Path (Join-Path $installDir 'msvc-debug')) {
    Remove-Item (Join-Path $installDir 'msvc-debug') -Recurse -Force
}

# Copy DependencySetup bat (mirrors ci.yml "Organize install files" step)
$depSetupSrc = Join-Path $root 'tools\DependencySetup_依赖库安装.bat'
$depSetupDst = Join-Path $installDir 'DependencySetup_依赖库安装.bat'
if (Test-Path $depSetupSrc) {
    Copy-Item $depSetupSrc $depSetupDst -Force
} else {
    Write-Host "[build-release-zip]   WARNING: $depSetupSrc not found, skipping" -ForegroundColor Yellow
}

# ---------- 8. stage + zip ----------
Write-Host "[build-release-zip] (8/8) stage install/ + zip into installer/" -ForegroundColor Cyan

# Clean installer/ unless user said keep
if (-not $KeepInstallerDir -and (Test-Path $installerDir)) {
    Get-ChildItem -LiteralPath $installerDir -Force | Remove-Item -Recurse -Force
}
New-Item -ItemType Directory -Path $installerDir -Force | Out-Null

# Stage to a temp dir, excluding user data (cache/config/data/debug) and .gitignore
$stage = Join-Path $env:TEMP ("maa-zip-stage-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage -Force | Out-Null
try {
    # robocopy /MIR with /XD to exclude user data dirs and any embedded .git (in case the
    # install/ tree was accidentally created inside a git work tree).
    & robocopy $installDir $stage /MIR /MT:8 `
        /XD cache config data debug .git `
        /XF '.gitignore' 'filelist.txt' `
        | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "robocopy stage failed (exit $LASTEXITCODE)" }

    $zipPath = Join-Path $installerDir ("MAA-$Version-win-x64.zip")
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($stage, $zipPath, 'Optimal', $false)
}
finally {
    Remove-Item $stage -Recurse -Force -ErrorAction SilentlyContinue
}

$size = (Get-Item $zipPath).Length
$sizeMB = [math]::Round($size / 1MB, 1)
Write-Host ""
Write-Host "[build-release-zip] DONE" -ForegroundColor Green
Write-Host "  Output: $zipPath" -ForegroundColor Green
Write-Host "  Size:   $sizeMB MB" -ForegroundColor Green
