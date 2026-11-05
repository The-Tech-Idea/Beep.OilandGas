param([string]$PackageSource = "$env:USERPROFILE/.nuget/packages",
    [string]$Filter = 'FullyQualifiedName~WizardInstallsPackagedSqlServerConnectorWithSerializableOwnership')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$parent = Split-Path $root -Parent
$feed = Join-Path $env:TEMP ('oilgas-setup-test-feed-' + [Guid]::NewGuid().ToString('N'))
$version = '99.0.0-oilgas.test.' + [Guid]::NewGuid().ToString('N')
New-Item -ItemType Directory -Path $feed | Out-Null
$projects = @(
    (Join-Path $parent 'BeepDM/DataManagementModelsStandard/DataManagementModels.csproj'),
    (Join-Path $parent 'BeepDM/DataManagementEngineStandard/DataManagementEngine.csproj'),
    (Join-Path $parent 'BeepDataSources/DataSourcesPlugins/RDBMSDataSource/RDBDataSource.csproj'),
    (Join-Path $parent 'BeepDataSources/DataSourcesPluginsCore/SQlServerDataSourceCore/SqlServerDataSourceCore.csproj')
)
# Build the actual source dependencies before packing. The generated package version
# is unique to this test; it is not a production release or a shared-feed update.
dotnet restore $projects[3] -p:TargetFrameworks=net10.0 --ignore-failed-sources --verbosity quiet
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet build $projects[3] -f net10.0 -p:TargetFrameworks=net10.0 --no-restore -p:GeneratePackageOnBuild=false --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
foreach ($project in $projects) {
    dotnet pack $project --no-build --no-restore -c Debug -p:TargetFrameworks=net10.0 "-p:Version=$version" "-p:PackageVersion=$version" -p:SkipLocalPackageCopy=true -o $feed --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
& (Join-Path $PSScriptRoot 'Test-SetupDriverBeepLoader.ps1') -SourceBuild -PackageSource $PackageSource `
    -Filter $Filter `
    -IsolatedDriverFeed $feed -IsolatedDriverVersion $version
exit $LASTEXITCODE
