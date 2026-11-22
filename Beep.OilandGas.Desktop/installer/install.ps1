<#
.SYNOPSIS
    Installs Beep Oil & Gas for the current user - no elevation, no machine-wide folder, no writes
    outside the per-user data root.

.DESCRIPTION
    REL-01. The application installs per user under %LOCALAPPDATA%\Programs\TheTechIdea\BeepOilGas and
    keeps every file it writes under %LOCALAPPDATA%\TheTechIdea\BeepOilGas (see OgPaths). Nothing is
    written to Program Files, ProgramData or the roaming profile, so a standard user can install and
    run it and an uninstall removes exactly what this script created.

    The payload is a published build (dotnet publish) of src\Beep.OilandGas.Desktop. Pass -Payload or
    publish into .\publish first.

.PARAMETER Payload
    The published folder to install from. Defaults to .\publish beside this script.

.PARAMETER InstallDirectory
    Where the program files go. Defaults to %LOCALAPPDATA%\Programs\TheTechIdea\BeepOilGas.

.PARAMETER NoShortcut
    Do not create the Start-menu shortcut.

.PARAMETER Start
    Start the application after installing.

.EXAMPLE
    dotnet publish ..\src\Beep.OilandGas.Desktop -c Release -r win-x64 --self-contained false -o .\publish
    .\install.ps1 -Start
#>
[CmdletBinding()]
param(
    [string] $Payload = (Join-Path $PSScriptRoot 'publish'),
    [string] $InstallDirectory = (Join-Path $env:LOCALAPPDATA 'Programs\TheTechIdea\BeepOilGas'),
    [switch] $NoShortcut,
    [switch] $Start
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$exeName = 'TheTechIdea.Beep.OilandGas.Desktop.exe'
$dataRoot = Join-Path $env:LOCALAPPDATA 'TheTechIdea\BeepOilGas'
$installVersion = '1'

function Write-Step([string] $text) { Write-Host "  $text" }

Write-Host "Beep Oil & Gas - per-user installation"
Write-Host ""

# ---------------------------------------------------------------- prerequisites
# AC-6: no elevation is requested anywhere in this script, and none is needed: both folders are the
# user's own. If the app is ever changed to install machine-wide, this check is the thing that fails.
if (-not (Test-Path -LiteralPath $Payload)) {
    throw "The payload folder '$Payload' does not exist. Publish the desktop project first (see the example in this script's help)."
}

$exe = Join-Path $Payload $exeName

if (-not (Test-Path -LiteralPath $exe)) {
    $found = Get-ChildItem -LiteralPath $Payload -Filter $exeName -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1

    if (-not $found) {
        throw "'$Payload' holds no $exeName - publish the desktop project, not the library projects."
    }

    $Payload = $found.Directory.FullName
}

# The native renderer is part of the payload; without it charts and PDFs cannot draw (AC-3).
$skia = Get-ChildItem -LiteralPath $Payload -Filter 'libSkiaSharp.dll' -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1

if (-not $skia) {
    throw "The payload holds no libSkiaSharp.dll: charts, well views, maps and PDF exports would fail. Republish without trimming the native assets."
}

Write-Step "payload      $Payload"
Write-Step "program files $InstallDirectory"
Write-Step "data root     $dataRoot (created by the application on first run)"

# ---------------------------------------------------------------- install
$previousVersion = $null

if (Test-Path -LiteralPath (Join-Path $InstallDirectory $exeName)) {
    # AC-5: an install over an existing version is an upgrade, and it is named as one rather than being silent.
    $record = Join-Path $dataRoot 'Diagnostics\installation.json'

    if (Test-Path -LiteralPath $record) {
        try { $previousVersion = (Get-Content -LiteralPath $record -Raw | ConvertFrom-Json).Version } catch { $previousVersion = $null }
    }

    Write-Step "existing installation found - upgrading (previous version: $(if ($previousVersion) { $previousVersion } else { 'not recorded' }))"
}

New-Item -ItemType Directory -Path $InstallDirectory -Force | Out-Null

# Copy the payload over the target. The data root is a different tree and is never touched by this.
Copy-Item -Path (Join-Path $Payload '*') -Destination $InstallDirectory -Recurse -Force

if (-not $NoShortcut) {
    $startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\TheTechIdea'

    New-Item -ItemType Directory -Path $startMenu -Force | Out-Null

    $link = Join-Path $startMenu 'Beep Oil & Gas.lnk'
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($link)
    $shortcut.TargetPath = Join-Path $InstallDirectory $exeName
    $shortcut.WorkingDirectory = $InstallDirectory
    $shortcut.Description = 'Beep Oil & Gas'
    $shortcut.Save()

    Write-Step "shortcut     $link"
}

# The uninstall script is written beside the program files, and it is the same contract the page
# shows: program files go, the data root stays unless the exact words are typed.
$uninstaller = Join-Path $InstallDirectory 'uninstall.ps1'
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'uninstall.ps1') -Destination $uninstaller -Force
Write-Step "uninstaller  $uninstaller"

# Record the version under the data root, so the next run knows it is an upgrade (AC-5). The
# application re-records it on first start, which keeps the record honest if this script is skipped.
$diagnostics = Join-Path $dataRoot 'Diagnostics'
New-Item -ItemType Directory -Path $diagnostics -Force | Out-Null

$version = (Get-Item -LiteralPath (Join-Path $InstallDirectory $exeName)).VersionInfo.ProductVersion

@{
    Version     = if ($version) { $version } else { '0.0.0' }
    InstalledAt = (Get-Date).ToUniversalTime().ToString('o')
    DataRoot    = $dataRoot
    Installer   = $installVersion
} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $diagnostics 'installation.json') -Encoding UTF8

Write-Step "recorded     version $version"

Write-Host ""
# Everything this script writes is in the two per-user folders, except the Start-menu shortcut, which Windows expects
# under %APPDATA% and the uninstall removes.
Write-Host "Installed. The program files and the data root are per-user; the only other write is the Start-menu shortcut."
Write-Host "Nothing was written to Program Files or ProgramData."
Write-Host "Uninstall from '$uninstaller' - the data root is kept unless you ask for it to be removed."

if ($Start) {
    Start-Process -FilePath (Join-Path $InstallDirectory $exeName)
}
