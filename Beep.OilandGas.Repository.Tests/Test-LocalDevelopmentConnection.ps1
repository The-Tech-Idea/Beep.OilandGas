#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '../Beep.OilandGas.Repository/LocalDevelopmentConnection.ps1')

$valid = @(
    'Server=(localdb)\MSSQLLocalDB;Database=BeepOilGasRepository;Integrated Security=true',
    'Data Source=(localdb)\TestInstance;Initial Catalog=TestRepository;Integrated Security=true'
)
foreach ($connection in $valid) {
    Assert-LocalDevelopmentConnection -Provider SqlServer -ConnectionString $connection
}

$invalid = @(
    'Server=remote.example;Database=(localdb);Integrated Security=true',
    'Server=remote.example;Database=Test;Application Name=(localdb)',
    'Server=(localdb)\MSSQLLocalDB;Data Source=remote.example;Database=Test',
    'Server=(localdb)\MSSQLLocalDB;Database=Test;Initial Catalog=Other',
    'Server=(localdb)\MSSQLLocalDB',
    'Server=(localdb)\MSSQLLocalDB;Database=',
    'Server=(localdb);Database=Test',
    'Database=Test',
    'not a connection string'
)
foreach ($connection in $invalid) {
    $rejected = $false
    try { Assert-LocalDevelopmentConnection -Provider SqlServer -ConnectionString $connection }
    catch { $rejected = $true }
    if (-not $rejected) { throw 'An invalid LocalDB target was accepted.' }
}
foreach ($provider in @('PostgreSql', 'Oracle')) {
    $rejected = $false
    try { Assert-LocalDevelopmentConnection -Provider $provider -ConnectionString $valid[0] }
    catch { $rejected = $true }
    if (-not $rejected) { throw 'A non-SQL Server provider was accepted for local development.' }
}
Write-Output 'Passed 13 LocalDB connection guard cases. No database connections were opened.'
