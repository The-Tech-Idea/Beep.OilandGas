<#
.SYNOPSIS
    CI guard: verifies that feature module files do NOT reference
    Beep.OilandGas.PPDM39.Models types directly.

.DESCRIPTION
    Feature modules (files matching *Module*.cs outside of
    Beep.OilandGas.PPDM39.DataManagement) must never register or
    instantiate PPDM39.Models table classes directly.  Those types
    are owned exclusively by the PPDM39 core and shared-reference
    modules.

    Permitted projects that may reference PPDM39.Models:
      - Beep.OilandGas.PPDM39.DataManagement  (core modules — allowed)
      - Beep.OilandGas.PPDM39                 (model definitions — allowed)
      - Beep.OilandGas.ApiService              (controllers/program — allowed)
      - Beep.OilandGas.Web                     (Blazor pages — allowed)
      - Beep.OilandGas.Client                  (API client — allowed)
      - *.Tests projects                       (test fixtures — allowed)

    Any *Module*.cs file in a FEATURE project that contains patterns
    matching forbidden PPDM39.Models usage will cause this script to
    exit with code 1.

.PARAMETER RootPath
    The root of the solution.  Defaults to the directory containing this script.

.EXAMPLE
    .\ci-guard-module-ownership.ps1
    .\ci-guard-module-ownership.ps1 -RootPath "C:\repos\Beep.OilandGas"
#>

[CmdletBinding()]
param(
    [string]$RootPath = (Split-Path -Parent $PSScriptRoot)
)

# ──────────────────────────────────────────────────────────────────────────────
# Configuration
# ──────────────────────────────────────────────────────────────────────────────

# Projects that are ALLOWED to reference PPDM39.Models inside module files.
$AllowedProjectFragments = @(
    "Beep.OilandGas.PPDM39.DataManagement",
    [System.IO.Path]::Combine("Beep.OilandGas.PPDM39", ""),  # exact project, not DataManagement
    "Beep.OilandGas.ApiService",
    "Beep.OilandGas.Web",
    "Beep.OilandGas.Client",
    ".Tests"
)

# Patterns that constitute a forbidden direct reference to PPDM39.Models
# inside a *Module*.cs file.
$ForbiddenPatterns = @(
    # typeof() with a PPDM39 model type
    'typeof\s*\(\s*Beep\.OilandGas\.PPDM39\.Models\.',
    # new() instantiation
    'new\s+Beep\.OilandGas\.PPDM39\.Models\.',
    # using static / using alias
    'using\s+(static\s+)?Beep\.OilandGas\.PPDM39\.Models\.',
    # EntityTypes.Add / list literal with PPDM39.Models type
    'EntityTypes.*Beep\.OilandGas\.PPDM39\.Models\.'
)

# ──────────────────────────────────────────────────────────────────────────────
# Helpers
# ──────────────────────────────────────────────────────────────────────────────

function IsAllowedProject([string]$filePath) {
    foreach ($fragment in $AllowedProjectFragments) {
        if ($filePath -like "*$fragment*") {
            return $true
        }
    }
    return $false
}

function FindViolations([string]$filePath) {
    $violations = @()
    $lines = Get-Content $filePath

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        foreach ($pattern in $ForbiddenPatterns) {
            if ($line -match $pattern) {
                $violations += [PSCustomObject]@{
                    File    = $filePath
                    Line    = $i + 1
                    Pattern = $pattern
                    Content = $line.Trim()
                }
            }
        }
    }
    return $violations
}

# ──────────────────────────────────────────────────────────────────────────────
# Main scan
# ──────────────────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "=== Module Ownership CI Guard ===" -ForegroundColor Cyan
Write-Host "Root: $RootPath"
Write-Host ""

# Collect all *Module*.cs files in the solution
$moduleFiles = Get-ChildItem -Path $RootPath -Recurse -Filter "*Module*.cs" |
    Where-Object { $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*" }

Write-Host "Scanning $($moduleFiles.Count) module file(s) for forbidden PPDM39.Models references..." -ForegroundColor Gray
Write-Host ""

$allViolations = @()

foreach ($file in $moduleFiles) {
    if (IsAllowedProject $file.FullName) {
        Write-Verbose "ALLOWED  $($file.FullName)"
        continue
    }

    $violations = FindViolations $file.FullName
    if ($violations.Count -gt 0) {
        $allViolations += $violations
    }
}

# ──────────────────────────────────────────────────────────────────────────────
# Report
# ──────────────────────────────────────────────────────────────────────────────

if ($allViolations.Count -eq 0) {
    Write-Host "[PASS] No forbidden PPDM39.Models references found in feature module files." -ForegroundColor Green
    Write-Host ""
    exit 0
}

Write-Host "[FAIL] Found $($allViolations.Count) forbidden PPDM39.Models reference(s) in feature modules:" -ForegroundColor Red
Write-Host ""

# Group by file for readable output
$byFile = $allViolations | Group-Object -Property File

foreach ($group in $byFile) {
    $relativePath = $group.Name.Replace($RootPath, "").TrimStart('\').TrimStart('/')
    Write-Host "  FILE: $relativePath" -ForegroundColor Yellow

    foreach ($v in $group.Group) {
        Write-Host "    Line $($v.Line): $($v.Content)" -ForegroundColor Red
        Write-Host "    Matched pattern: $($v.Pattern)" -ForegroundColor DarkGray
    }
    Write-Host ""
}

Write-Host "ACTION REQUIRED:" -ForegroundColor Yellow
Write-Host "  Feature modules must NOT register or instantiate Beep.OilandGas.PPDM39.Models types."
Write-Host "  Replace direct type references with project-owned table classes."
Write-Host "  See: Plans/module-setup-phase-plan/00_Master_Phased_Plan.md — Module Ownership Rules"
Write-Host ""

exit 1
