param(
    [Parameter(Mandatory)][string]$FeedPath,
    [Parameter(Mandatory)][string]$Version
)

$ErrorActionPreference = 'Stop'
if ($Version -notmatch '^\d+\.\d+\.\d+(?:-[0-9A-Za-z]+(?:[.-][0-9A-Za-z]+)*)?$') {
    throw 'Provide an explicit NuGet package version, for example 1.2.3 or 1.2.3-setup.1.'
}
$root = Split-Path $PSScriptRoot -Parent
$parent = Split-Path $root -Parent
$feed = [IO.Path]::GetFullPath($FeedPath)
$projects = @(
    (Join-Path $parent 'BeepDM/DataManagementModelsStandard/DataManagementModels.csproj'),
    (Join-Path $parent 'BeepDM/DataManagementEngineStandard/DataManagementEngine.csproj'),
    (Join-Path $parent 'BeepDataSources/DataSourcesPlugins/RDBMSDataSource/RDBDataSource.csproj'),
    (Join-Path $parent 'BeepDataSources/DataSourcesPluginsCore/SQlServerDataSourceCore/SqlServerDataSourceCore.csproj')
)
foreach ($project in $projects) {
    if (-not (Test-Path -LiteralPath $project -PathType Leaf)) { throw "Required source project is missing: $project" }
}
$packages = @('TheTechIdea.Beep.DataManagementModels', 'TheTechIdea.Beep.DataManagementEngine',
    'TheTechIdea.Beep.RDBDataSource', 'TheTechIdea.Beep.SqlServerDataSourceCore')
foreach ($package in $packages) {
    $target = Join-Path $feed "$package.$Version.nupkg"
    if (Test-Path -LiteralPath $target) { throw "Package already exists; choose a new version or feed: $target" }
}
New-Item -ItemType Directory -Path $feed -Force | Out-Null

# The four packages are one source-built set: the connector depends on these
# Beep runtime fixes. Never substitute the older public runtime packages.
dotnet restore $projects[3] -p:TargetFrameworks=net10.0 --ignore-failed-sources --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Connector restore failed.' }
dotnet build $projects[3] -f net10.0 -p:TargetFrameworks=net10.0 --no-restore -p:GeneratePackageOnBuild=false --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Connector build failed.' }
foreach ($project in $projects) {
    dotnet pack $project --no-build --no-restore -c Debug -p:TargetFrameworks=net10.0 "-p:Version=$Version" "-p:PackageVersion=$Version" -p:SkipLocalPackageCopy=true -o $feed --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { throw "Package creation failed: $project" }
}
foreach ($package in $packages) {
    if (-not (Test-Path -LiteralPath (Join-Path $feed "$package.$Version.nupkg") -PathType Leaf)) {
        throw "The expected package was not created: $package $Version"
    }
}
Write-Host "Source-built connector feed: $feed"
Write-Host "Configure SetupDrivers:Connectors:SQLServerDataSource:Version as $Version and include this directory in SetupDrivers:PackageSources."
Write-Host 'Include a trusted source for third-party dependencies and validate the deployed provider before use.'
