# PowerShell script to run the metadata generator
# This parses SQL scripts ONCE and generates C# class/JSON

$scriptFolder = "C:\Users\f_ald\OneDrive\SimpleInfoapps\PPDM\ppdm39_SqlServerScript\ms"
$tabScript = Join-Path $scriptFolder "TAB.sql"
$pkScript = Join-Path $scriptFolder "PK.sql"
$fkScript = Join-Path $scriptFolder "FK.sql"

$outputFolder = Join-Path $PSScriptRoot ".." "Core" "Metadata"
$jsonOutput = Join-Path $outputFolder "PPDM39Metadata.json"
$csharpOutput = Join-Path $outputFolder "PPDM39Metadata.Generated.cs"

Write-Host "PPDM39 Metadata Generator" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green
Write-Host "TAB.sql: $tabScript"
Write-Host "PK.sql: $pkScript"
Write-Host "FK.sql: $fkScript"
Write-Host ""

# Check if scripts exist
if (-not (Test-Path $tabScript)) {
    Write-Host "ERROR: TAB.sql not found at $tabScript" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $pkScript)) {
    Write-Host "ERROR: PK.sql not found at $pkScript" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $fkScript)) {
    Write-Host "ERROR: FK.sql not found at $fkScript" -ForegroundColor Red
    exit 1
}

Write-Host "All SQL scripts found. Ready to generate metadata." -ForegroundColor Green
Write-Host ""
Write-Host "To generate metadata, run the GenerateMetadata tool or use:" -ForegroundColor Yellow
Write-Host "  var generator = new PPDMMetadataGenerator(tabScript, pkScript, fkScript);" -ForegroundColor Cyan
Write-Host "  generator.GenerateCSharpMetadata(outputPath);" -ForegroundColor Cyan
Write-Host "  generator.GenerateJsonMetadata(outputPath);" -ForegroundColor Cyan

