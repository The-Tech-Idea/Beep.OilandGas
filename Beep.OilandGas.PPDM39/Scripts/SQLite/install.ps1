# PPDM39 SQLite Installation Script for Windows
# This script creates the PPDM39 database and runs all required scripts

param(
    [string]$DatabaseName = "ppdm39.db"
)

$ErrorActionPreference = "Stop"

Write-Host "PPDM39 SQLite Installation Script" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host "Database: $DatabaseName"
Write-Host ""

# Get the directory where the script is located
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DbPath = Join-Path $ScriptDir $DatabaseName

# Check if sqlite3 is available
$sqlitePath = Get-Command sqlite3 -ErrorAction SilentlyContinue
if (-not $sqlitePath) {
    Write-Host "Error: sqlite3 command not found. Please install SQLite." -ForegroundColor Red
    Write-Host "You can download it from: https://www.sqlite.org/download.html" -ForegroundColor Yellow
    exit 1
}

# Function to run a SQL file
function Run-SqlFile {
    param(
        [string]$File,
        [string]$Description,
        [bool]$Optional = $false
    )
    
    if (-not (Test-Path $File)) {
        if ($Optional) {
            Write-Host "Skipping optional file: $File (not found)" -ForegroundColor Yellow
            return
        } else {
            Write-Host "Error: Required file not found: $File" -ForegroundColor Red
            exit 1
        }
    }
    
    Write-Host "Running: $Description [$File]" -ForegroundColor Green
    Get-Content $File | & sqlite3 $DbPath
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error executing $File" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}

# Enable foreign keys
Write-Host "Enabling foreign key constraints..."
& sqlite3 $DbPath "PRAGMA foreign_keys = ON;"

# Run scripts in order
Run-SqlFile "$ScriptDir\TAB.sql" "Creating Tables and Columns" $false
Run-SqlFile "$ScriptDir\PK.sql" "Creating Primary Keys" $false
Run-SqlFile "$ScriptDir\CK.sql" "Creating Check Constraints" $false
Run-SqlFile "$ScriptDir\FK.sql" "Creating Foreign Key Constraints" $false
Run-SqlFile "$ScriptDir\OUOM.sql" "Creating Original Units of Measure Foreign Keys" $false
Run-SqlFile "$ScriptDir\UOM.sql" "Creating Units of Measure Foreign Keys" $false
Run-SqlFile "$ScriptDir\RQUAL.sql" "Creating ROW_QUALITY Foreign Keys" $false
Run-SqlFile "$ScriptDir\RSRC.sql" "Creating SOURCE Foreign Keys" $false
Run-SqlFile "$ScriptDir\GUID.sql" "Creating GUID Constraints" $true

# Note: TCM, CCM, and SYN are not applicable for SQLite
Write-Host "Skipping TCM.sql (SQLite doesn't support table comments)" -ForegroundColor Yellow
Write-Host "Skipping CCM.sql (SQLite doesn't support column comments)" -ForegroundColor Yellow
Write-Host "Skipping SYN.sql (SQLite doesn't support synonyms)" -ForegroundColor Yellow
Write-Host ""

Write-Host "Installation completed successfully!" -ForegroundColor Green
Write-Host "Database created at: $DbPath" -ForegroundColor Cyan
