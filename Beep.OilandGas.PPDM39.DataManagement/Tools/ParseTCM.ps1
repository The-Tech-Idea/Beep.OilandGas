# PowerShell script to parse TCM.sql and create ppdm38tabledescr.json

param(
    [string]$TcmSqlPath = "",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
$solutionRoot = Split-Path -Parent $projectRoot

# Default paths
if ([string]::IsNullOrWhiteSpace($TcmSqlPath)) {
    $TcmSqlPath = Join-Path $env:USERPROFILE "OneDrive\SimpleInfoapps\PPDM\html\TCM.sql"
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $projectRoot "Core\Metadata\ppdm38tabledescr.json"
}

Write-Host "Parsing TCM.sql to extract table descriptions..."
Write-Host "Input: $TcmSqlPath"
Write-Host "Output: $OutputPath"

if (-not (Test-Path $TcmSqlPath)) {
    Write-Error "TCM.sql file not found: $TcmSqlPath"
    exit 1
}

# Create output directory if it doesn't exist
$outputDir = Split-Path -Parent $OutputPath
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# Read TCM.sql and parse descriptions
$descriptions = @{}
$content = Get-Content $TcmSqlPath -Raw -Encoding UTF8

# Pattern: execute sp_addextendedproperty 'Description','[Description]','USER','dbo','TABLE','[TABLE_NAME]';
$pattern = "execute\s+sp_addextendedproperty\s+'Description',\s*'([^']*(?:''[^']*)*)',\s*'USER',\s*'dbo',\s*'TABLE',\s*'([^']+)';"

$matches = [regex]::Matches($content, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

foreach ($match in $matches) {
    if ($match.Groups.Count -ge 3) {
        $description = $match.Groups[1].Value
        $tableName = $match.Groups[2].Value
        
        # Replace SQL escaped single quotes
        $description = $description -replace "''", "'"
        
        if (-not $descriptions.ContainsKey($tableName)) {
            $descriptions[$tableName] = $description
        } else {
            Write-Warning "Duplicate table name found: $tableName"
        }
    }
}

# Create JSON object
$jsonObject = @{}
foreach ($key in $descriptions.Keys) {
    $jsonObject[$key] = $descriptions[$key]
}

# Convert to JSON with indentation
$json = $jsonObject | ConvertTo-Json -Depth 10

# Write to file
$json | Out-File -FilePath $OutputPath -Encoding UTF8 -NoNewline

Write-Host "Successfully parsed $($descriptions.Count) table descriptions"
Write-Host "Output written to: $OutputPath"

