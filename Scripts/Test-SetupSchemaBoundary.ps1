$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$controller = Get-Content -LiteralPath (Join-Path $root 'Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39SetupController.cs') -Raw
$service = Get-Content -LiteralPath (Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Services/PPDM39SetupService.cs') -Raw
$migration = Get-Content -LiteralPath (Join-Path $root 'Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39SetupController.Migrations.cs') -Raw
$editor = Get-Content -LiteralPath (Join-Path $root 'Beep.OilandGas.Web/Components/PPDM39DatabaseWizard.razor') -Raw
foreach ($route in @('scripts/generate','execute-script','execute-all-scripts','create-database','discover-scripts/{databaseType}',
    'check-schema-privileges','create-schema','create-schema-from-migration','drop-database','recreate-database')) {
    if ($controller -match ('\[Http(?:Get|Post|Put|Delete)\("' + [regex]::Escape($route) + '"\)')) { throw "Retired setup route is present: $route" }
}
foreach ($route in @('schema/plan','schema/approve','schema/execute')) {
    if ($migration -notmatch ('\[HttpPost\("' + [regex]::Escape($route) + '"\)')) { throw "Migration route is missing: $route" }
}
if ($service -match '\bExecuteSql\s*\(') { throw 'The active PPDM setup service contains a direct SQL execution call.' }
if ($controller -match '\bPPDMScriptGenerator\b') { throw 'The retired schema script generator is present.' }
if ($editor -match '/api/ppdm39/setup/(?:execute|scripts|create-schema)') { throw 'The connection editor still calls a retired schema endpoint.' }
Write-Host 'PASS: PPDM setup exposes migration review routes and omits the retired script and direct DDL paths.'
