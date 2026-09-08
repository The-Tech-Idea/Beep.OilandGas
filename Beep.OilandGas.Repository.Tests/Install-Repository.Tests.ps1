#Requires -Version 7.0
$ErrorActionPreference = 'Stop'
$installer = Join-Path $PSScriptRoot '../Beep.OilandGas.Repository/Install-Repository.ps1'
$original = $env:OILGAS_REPOSITORY_CONNECTION
$global:OilGasInstallerTestCalls = @()
$global:OilGasInstallerTestExitCode = 0

function dotnet {
    $global:OilGasInstallerTestCalls += ,@($args)
    $global:LASTEXITCODE = $global:OilGasInstallerTestExitCode
}
function Assert-True([bool] $condition, [string] $message) {
    if (-not $condition) { throw $message }
}
function Assert-Rejected([scriptblock] $action) {
    $rejected = $false
    try { & $action } catch { $rejected = $true }
    Assert-True $rejected 'Expected installer to reject invalid input.'
}

try {
    foreach ($provider in @('SqlServer', 'PostgreSql', 'Oracle')) {
        $env:OILGAS_REPOSITORY_CONNECTION = 'test-connection-never-sent-to-a-database'
        & $installer -Provider $provider -Mode Apply
        Assert-True ($global:OilGasInstallerTestCalls[-1] -contains ($provider + 'RepositoryDbContext')) 'Incorrect provider context.'
        Assert-True ($global:OilGasInstallerTestCalls[-1] -notcontains '--connection') 'Connection secret must not be passed on the command line.'
        Assert-True ($env:OILGAS_REPOSITORY_CONNECTION -eq 'test-connection-never-sent-to-a-database') 'Environment changed after success.'
    }
    $count = $global:OilGasInstallerTestCalls.Count
    & $installer -Mode Apply -WhatIf
    Assert-True ($global:OilGasInstallerTestCalls.Count -eq $count) 'WhatIf invoked dotnet.'
    Assert-Rejected { & $installer -LocalDevelopment -Mode Apply }
    Assert-Rejected { & $installer -Mode Script -OutputPath $installer }
    Assert-Rejected { & $installer -Mode Script }
    Assert-True ($global:OilGasInstallerTestCalls.Count -eq $count) 'Invalid input invoked dotnet.'

    $global:OilGasInstallerTestExitCode = 1
    Assert-Rejected { & $installer -Mode Apply }
    Assert-True ($env:OILGAS_REPOSITORY_CONNECTION -eq 'test-connection-never-sent-to-a-database') 'Environment changed after failure.'
    $global:OilGasInstallerTestExitCode = 0
    $env:OILGAS_REPOSITORY_CONNECTION = $null
    Assert-Rejected { & $installer -Mode Apply }
    if ($IsWindows) {
        & $installer -LocalDevelopment -Mode Apply
        Assert-True ([string]::IsNullOrEmpty($env:OILGAS_REPOSITORY_CONNECTION)) 'LocalDB temporary environment was not cleared.'
    }
    Write-Host 'Installer guardrail checks passed; dotnet was mocked and no database was accessed.'
} finally {
    $env:OILGAS_REPOSITORY_CONNECTION = $original
}
