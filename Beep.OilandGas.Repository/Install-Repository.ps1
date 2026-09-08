#Requires -Version 7.0
[CmdletBinding(SupportsShouldProcess)]
param(
    [ValidateSet('SqlServer', 'PostgreSql', 'Oracle')]
    [string] $Provider = 'SqlServer',
    [ValidateSet('Script', 'Apply')]
    [string] $Mode = 'Script',
    [string] $OutputPath,
    [switch] $LocalDevelopment
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'Beep.OilandGas.Repository.csproj'
$contexts = @{
    SqlServer = 'SqlServerRepositoryDbContext'
    PostgreSql = 'PostgreSqlRepositoryDbContext'
    Oracle = 'OracleRepositoryDbContext'
}
$originalConnection = [Environment]::GetEnvironmentVariable('OILGAS_REPOSITORY_CONNECTION', 'Process')
$connection = $originalConnection

if ($LocalDevelopment) {
    if ($Provider -ne 'SqlServer' -or -not $IsWindows) {
        throw 'LocalDevelopment requires SQL Server LocalDB on Windows.'
    }
    if (-not [string]::IsNullOrWhiteSpace($originalConnection)) {
        throw 'Unset OILGAS_REPOSITORY_CONNECTION before selecting LocalDevelopment to avoid an ambiguous target.'
    }
    $configPath = Join-Path $PSScriptRoot '../Beep.OilandGas.ApiService/appsettings.Development.json'
    $repository = (Get-Content -LiteralPath $configPath -Raw | ConvertFrom-Json).Repository
    if ($repository.Provider -ne 'SqlServer' -or $repository.ConnectionString -notmatch '\(localdb\)') {
        throw 'Development repository configuration must select SQL Server LocalDB.'
    }
    $connection = $repository.ConnectionString
}
if ([string]::IsNullOrWhiteSpace($connection)) {
    throw 'Set OILGAS_REPOSITORY_CONNECTION for the selected provider, or select -LocalDevelopment.'
}

if ($Mode -eq 'Script') {
    if ([string]::IsNullOrWhiteSpace($OutputPath)) {
        throw 'Specify a new -OutputPath for the idempotent SQL preview.'
    }
    $OutputPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputPath)
    if (Test-Path -LiteralPath $OutputPath) { throw 'OutputPath already exists; choose a new file.' }
    if (-not (Test-Path -LiteralPath (Split-Path -Parent $OutputPath) -PathType Container)) {
        throw 'The OutputPath parent directory must exist.'
    }
} elseif (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    throw 'OutputPath is only supported in Script mode.'
}

$arguments = if ($Mode -eq 'Script') {
    @('ef', 'migrations', 'script', '--idempotent', '--output', $OutputPath)
} else {
    @('ef', 'database', 'update')
}
$arguments += @('--project', $project, '--context', $contexts[$Provider])
$target = if ($LocalDevelopment) { 'configured development LocalDB repository' } else { "$Provider repository from OILGAS_REPOSITORY_CONNECTION" }
$operation = if ($Mode -eq 'Script') { "Generate idempotent SQL at $OutputPath" } else { 'Apply pending repository EF migrations' }
if (-not $PSCmdlet.ShouldProcess($target, $operation)) { return }

try {
    [Environment]::SetEnvironmentVariable('OILGAS_REPOSITORY_CONNECTION', $connection, 'Process')
    & dotnet @arguments
    if ($LASTEXITCODE -ne 0) { throw "Repository migration command failed (exit $LASTEXITCODE)." }
    Write-Host 'Repository operation completed. No users were seeded and no BeepDM module migration was requested.'
} finally {
    [Environment]::SetEnvironmentVariable('OILGAS_REPOSITORY_CONNECTION', $originalConnection, 'Process')
}
