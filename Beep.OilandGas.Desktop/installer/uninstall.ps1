<#
.SYNOPSIS
    Uninstalls Beep Oil & Gas for the current user. Program files go; the data root stays by default.

.DESCRIPTION
    REL-01 AC-4. Removing the program files is the normal uninstall. The per-user data root - the
    failure store, the logs, the local database, the environment and the exports - is removed only when
    -RemoveDataRoot is given together with the exact confirmation sentence, which names the folder:

        .\uninstall.ps1 -RemoveDataRoot -Confirmation "REMOVE C:\Users\you\AppData\Local\TheTechIdea\BeepOilGas"

    Without -RemoveDataRoot the data root is kept, so reinstalling finds the same data and an uninstall
    never loses a customer's work by accident.

.PARAMETER InstallDirectory
    The program folder to remove. Defaults to %LOCALAPPDATA%\Programs\TheTechIdea\BeepOilGas.

.PARAMETER RemoveDataRoot
    Also remove the per-user data root. Requires -Confirmation.

.PARAMETER Confirmation
    The exact sentence 'REMOVE <data root>'; the data root's path is printed when it is missing or wrong.

.PARAMETER Yes
    Do not ask before removing the program files.
#>
[CmdletBinding()]
param(
    [string] $InstallDirectory = (Join-Path $env:LOCALAPPDATA 'Programs\TheTechIdea\BeepOilGas'),
    [switch] $RemoveDataRoot,
    [string] $Confirmation = '',
    [switch] $Yes
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$exeName = 'TheTechIdea.Beep.OilandGas.Desktop.exe'
$dataRoot = Join-Path $env:LOCALAPPDATA 'TheTechIdea\BeepOilGas'
$startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\TheTechIdea'
$failureStore = Join-Path $dataRoot 'Diagnostics\failures.db'
$logDirectory = Join-Path $dataRoot 'Logs'

Write-Host "Beep Oil & Gas - uninstall"
Write-Host ""
Write-Host "  program files $InstallDirectory"
Write-Host "  data root    $dataRoot"
Write-Host ""

if ($RemoveDataRoot) {
    # AC-4: the choice that removes the failure store and the logs must name them, and must be exact.
    $expected = "REMOVE $dataRoot"

    if ($Confirmation -ne $expected) {
        throw "The data root was not removed. It holds the failure store ($failureStore), the logs ($logDirectory), the local database, the environment and the exports.`nTo remove it, confirm with exactly:`n  -Confirmation `"$expected`""
    }

    Write-Host "  the data root WILL be removed after the program files: $dataRoot"
} else {
    Write-Host "  the data root will be KEPT: the failure store ($failureStore) and the logs ($logDirectory) stay."
}

if (-not $Yes) {
    $answer = Read-Host 'Remove the program files now? [y/N]'

    if ($answer -notmatch '^(y|yes)$') {
        Write-Host 'Nothing was removed.'
        return
    }
}

# Refuse while the application is running: a half-removed install is worse than a refused uninstall.
$running = Get-Process -Name ([System.IO.Path]::GetFileNameWithoutExtension($exeName)) -ErrorAction SilentlyContinue

if ($running) {
    throw "The application is running (process id $($running[0].Id)). Close it and run this again; nothing was removed."
}

if (Test-Path -LiteralPath $InstallDirectory) {
    Remove-Item -LiteralPath $InstallDirectory -Recurse -Force
    Write-Host "  removed the program files"
} else {
    Write-Host "  the program files were already gone"
}

$link = Join-Path $startMenu 'Beep Oil & Gas.lnk'

if (Test-Path -LiteralPath $link) {
    Remove-Item -LiteralPath $link -Force
    Write-Host "  removed the shortcut"
}

if ($RemoveDataRoot) {
    if (Test-Path -LiteralPath $dataRoot) {
        Remove-Item -LiteralPath $dataRoot -Recurse -Force
        Write-Host "  removed the data root"
    } else {
        Write-Host "  the data root was already gone"
    }
} else {
    Write-Host "  kept the data root - reinstall to find the same data"
}

Write-Host ""
Write-Host "Done."
