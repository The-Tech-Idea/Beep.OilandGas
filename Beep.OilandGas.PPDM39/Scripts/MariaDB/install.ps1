# PPDM39 MariaDB Installation Script for Windows
# This script creates the PPDM39 database and runs all required scripts

param(
    [string]$DatabaseName = "ppdm39",
    [string]$DbUser = "root",
    [string]$DbHost = "localhost",
    [int]$DbPort = 3306
)

$ErrorActionPreference = "Stop"

Write-Host "PPDM39 MariaDB Installation Script" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host "Database: $DatabaseName"
Write-Host "User: $DbUser"
Write-Host "Host: $DbHost"
Write-Host "Port: $DbPort"
Write-Host ""

# Get the directory where the script is located
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Check if mysql is available
$mysqlPath = Get-Command mysql -ErrorAction SilentlyContinue
if (-not $mysqlPath) {
    Write-Host "Error: mysql command not found. Please install MariaDB/MySQL client tools." -ForegroundColor Red
    exit 1
}

# Prompt for password if not set
if (-not $env:MYSQL_PWD) {
    $securePassword = Read-Host "Enter MySQL password for $DbUser" -AsSecureString
    $env:MYSQL_PWD = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    )
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
    Get-Content $File | & mysql -h $DbHost -P $DbPort -u $DbUser $DatabaseName
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error executing $File" -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}

# Create database if it doesn't exist
Write-Host "Checking if database exists..."
$dbExists = & mysql -h $DbHost -P $DbPort -u $DbUser -e "SHOW DATABASES LIKE '$DatabaseName';" | Select-String -Pattern "\b$DatabaseName\b"

if (-not $dbExists) {
    Write-Host "Creating database: $DatabaseName"
    & mysql -h $DbHost -P $DbPort -u $DbUser -e "CREATE DATABASE $DatabaseName CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
    Write-Host ""
}

# Run scripts in order
Run-SqlFile "$ScriptDir\TAB.sql" "Creating Tables and Columns" $false
Run-SqlFile "$ScriptDir\PK.sql" "Creating Primary Keys" $false
Run-SqlFile "$ScriptDir\CK.sql" "Creating Check Constraints" $false
Run-SqlFile "$ScriptDir\FK.sql" "Creating Foreign Key Constraints" $false
Run-SqlFile "$ScriptDir\OUOM.sql" "Creating Original Units of Measure Foreign Keys" $false
Run-SqlFile "$ScriptDir\UOM.sql" "Creating Units of Measure Foreign Keys" $false
Run-SqlFile "$ScriptDir\RQUAL.sql" "Creating ROW_QUALITY Foreign Keys" $false
Run-SqlFile "$ScriptDir\RSRC.sql" "Creating SOURCE Foreign Keys" $false
Run-SqlFile "$ScriptDir\TCM.sql" "Creating Table Comments" $true
Run-SqlFile "$ScriptDir\CCM.sql" "Creating Column Comments" $true
Run-SqlFile "$ScriptDir\SYN.sql" "Creating Synonyms" $true
Run-SqlFile "$ScriptDir\GUID.sql" "Creating GUID Constraints" $true

Write-Host "Installation completed successfully!" -ForegroundColor Green
