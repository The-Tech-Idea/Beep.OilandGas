param([string]$PackageSource = "$env:USERPROFILE/.nuget/packages", [string]$Filter = 'FullyQualifiedName~SetupDriverBeepLoaderTests', [switch]$SourceBuild,
    [string]$IsolatedDriverFeed, [string]$IsolatedDriverVersion, [switch]$BundledSqlServer, [switch]$PublicDriverDownload, [switch]$VerifySqlServerRestore,
    [string]$HseLifecycleTestAssembly)
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
function XmlPath([string]$path) { [System.Security.SecurityElement]::Escape($path) }
if ($SourceBuild) {
    $dataProject = Join-Path $root 'Beep.OilandGas.Models/Beep.OilandGas.Models.csproj'
    Update-OilGasRestoredFixtureProjectFile -ProjectPath $dataProject -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
    dotnet restore $dataProject -p:UseBeepDMSource=true --ignore-failed-sources --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    dotnet build $dataProject --no-restore -p:UseBeepDMSource=true -p:BuildProjectReferences=false --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    $repositoryProject = Join-Path $root 'Beep.OilandGas.Repository/Beep.OilandGas.Repository.csproj'
    Update-OilGasRestoredFixtureProjectFile -ProjectPath $repositoryProject -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
    dotnet restore $repositoryProject -p:UseBeepDMSource=true --ignore-failed-sources --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    dotnet build $repositoryProject --no-restore -p:UseBeepDMSource=true -p:BuildProjectReferences=false --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    $modelsProject = Join-Path $root 'Beep.OilandGas.Models/Beep.OilandGas.Models.csproj'
    Update-OilGasRestoredFixtureProjectFile -ProjectPath $modelsProject -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
    dotnet restore $modelsProject -p:UseBeepDMSource=true --ignore-failed-sources --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    dotnet build $modelsProject --no-restore -p:UseBeepDMSource=true -p:BuildProjectReferences=false -p:GeneratePackageOnBuild=false --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    $sqlServerProject = Join-Path (Split-Path $root -Parent) 'BeepDataSources/DataSourcesPluginsCore/SQlServerDataSourceCore/SqlServerDataSourceCore.csproj'
    Update-OilGasRestoredFixtureProjectFile -ProjectPath $sqlServerProject -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
    dotnet restore $sqlServerProject --ignore-failed-sources --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    dotnet build $sqlServerProject -f net10.0 --no-restore -p:GeneratePackageOnBuild=false --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    $ppdmProject = Join-Path $root 'Beep.OilandGas.PPDM39/Beep.OilandGas.PPDM39.csproj'
    Update-OilGasRestoredFixtureProjectFile -ProjectPath $ppdmProject -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
    dotnet restore $ppdmProject -p:UseBeepDMSource=true --ignore-failed-sources --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    dotnet build $ppdmProject --no-restore -p:UseBeepDMSource=true -p:BuildProjectReferences=false -p:GeneratePackageOnBuild=false --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    $applicationProject = Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Beep.OilandGas.PPDM39.DataManagement.csproj'
    Update-OilGasRestoredFixtureProjectFile -ProjectPath $applicationProject -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
    dotnet restore $applicationProject -p:UseBeepDMSource=true --ignore-failed-sources --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    dotnet build $applicationProject --no-restore -p:UseBeepDMSource=true -p:BuildProjectReferences=false --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
$work = Join-Path $env:TEMP ("oilgas-beep-loader-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work | Out-Null
$test = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.ApiService.Tests/SetupDriverBeepLoaderTests.cs'))
$unitTest = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.ApiService.Tests/SetupDriverServiceTests.cs'))
$service = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.ApiService/Services/SetupDriverService.cs'))
$item = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Models/SetupDriverItem.cs'))
$refusal = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.Models/Core/Refusals/RefusalException.cs'))
$kind = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.Models/Core/Refusals/RefusalKind.cs'))
$probe = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Services/ConnectionProbe.cs'))
$target = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/MigrationConnectionTarget.cs'))
$selection = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.ApiService/Services/SystemSetupConnectionSelectionService.cs'))
$selectionContract = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Models/SystemSetupConnectionSelection.cs'))
$modelBase = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.Models/Data/ModelEntityBase.cs'))
$modelInterface = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.PPDM.Models/IPPDMEntity.cs'))
$systemRecord = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Models/SYSTEM_SETUP_RECORD.cs'))
$companyEnvironment = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Models/COMPANY_OILGAS_ENVIRONMENT.cs'))
$ghgSelection = [System.Security.SecurityElement]::Escape((Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Models/COMPANY_GHG_CATALOG_SELECTION.cs'))
$beepReferences = if ($SourceBuild) {
    $beepRoot = Join-Path (Split-Path $root -Parent) 'BeepDM'
    $sourcesRoot = Join-Path (Split-Path $root -Parent) 'BeepDataSources'
    $bundledReference = if ($BundledSqlServer) { '<ProjectReference Include="' + (XmlPath (Join-Path $sourcesRoot 'DataSourcesPluginsCore/SQlServerDataSourceCore/SqlServerDataSourceCore.csproj')) + '" AdditionalProperties="GeneratePackageOnBuild=false" />' } else { '' }
    @"
<ProjectReference Include="$(XmlPath (Join-Path $beepRoot 'DataManagementEngineStandard/DataManagementEngine.csproj'))" AdditionalProperties="GeneratePackageOnBuild=false" />
<ProjectReference Include="$(XmlPath (Join-Path $beepRoot 'DataManagementModelsStandard/DataManagementModels.csproj'))" AdditionalProperties="GeneratePackageOnBuild=false" />
<ProjectReference Include="$(XmlPath (Join-Path $sourcesRoot 'DataSourcesPlugins/RDBMSDataSource/RDBDataSource.csproj'))" AdditionalProperties="GeneratePackageOnBuild=false" />
<ProjectReference Include="$(XmlPath (Join-Path $sourcesRoot 'DataSourcesPluginsCore/SqliteDatasourceCore/SqliteDatasourceCore.csproj'))" AdditionalProperties="GeneratePackageOnBuild=false" />
$bundledReference
<ProjectReference Include="$(XmlPath (Join-Path (Split-Path $root -Parent) 'OilandGasTools/src/TheTechIdea.OilGas.Core/TheTechIdea.OilGas.Core.csproj'))" />
<ProjectReference Include="$(XmlPath (Join-Path (Split-Path $root -Parent) 'OilandGasTools/src/TheTechIdea.OilGas.Loaders/TheTechIdea.OilGas.Loaders.csproj'))" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.6" />
<PackageReference Include="System.Data.SqlClient" Version="4.9.1" />
<PackageReference Include="Microsoft.Data.Sqlite" Version="10.0.9" />
<PackageReference Include="Npgsql" Version="10.0.3" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.9" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.9" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.3" />
<PackageReference Include="Oracle.EntityFrameworkCore" Version="10.23.26300" />
<Reference Include="Beep.OilandGas.Repository"><HintPath>$(XmlPath (Join-Path $root 'Beep.OilandGas.Repository/bin/Debug/net10.0/Beep.OilandGas.Repository.dll'))</HintPath></Reference>
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/ModuleDatabaseTarget.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/SystemSetupTarget.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/SystemSetupRecordStore.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/ISystemSetupRecordReader.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/ICompanyEnvironmentStore.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/CompanyEnvironmentStore.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/CompanyEnvironmentPreviewer.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Controllers/CompanyEnvironmentController.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/Fixtures/CompanyEnvironmentPreviewBoundary.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/CompanyEnvironmentPreviewControllerTests.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/SystemSetupToolsDefaults.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/SystemSetupToolsDefaultsTests.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/CompanyEnvironmentEffectiveResolver.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/CompanyEnvironmentSources.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.Web/Services/CompanyEnvironmentOverrideEditor.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/CompanySourceOverlayTests.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/CompanyPPDMEvidenceReader.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/CompanyFieldObligationWriter.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/IncidentReportSchema.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/PPDMIncidentReportWriter.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/CompanyFieldObligationSqliteTests.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/CompanyOrganizationVerifier.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/Fixtures/CompanyOrganizationNativeBoundary.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/BeepRepositoryConnectionSource.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/SetupRepositoryConnectionCreationService.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/SetupRepositoryConnectionSelectionService.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/BeepCredentialKeyProvider.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/SetupBootstrapAccess.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/SetupRepositoryMigrationService.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Middleware/SetupBootstrapPaths.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Middleware/SetupGateMiddleware.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Controllers/SetupBootstrapRepositoryController.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Controllers/SetupBootstrapDriverController.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/SetupProtectedBeepConnectionTests.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/SetupFirstRunHttpIntegrationTests.cs'))" />
"@
} else {
    '<PackageReference Include="TheTechIdea.Beep.DataManagementEngine" Version="3.1.1" /><PackageReference Include="TheTechIdea.Beep.DataManagementModels" Version="3.1.1" /><PackageReference Include="TheTechIdea.Beep.RDBDataSource" Version="2.1.1" /><PackageReference Include="Microsoft.Data.Sqlite" Version="10.0.9" /><PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.6" /><PackageReference Include="Npgsql" Version="10.0.3" /><Compile Include="' + (XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/ModuleDatabaseTarget.cs')) + '" />'
}
$modelReferences = if ($SourceBuild) {
    $bin = Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/bin/Debug/net10.0'
    @('Beep.OilandGas.Models', 'Beep.OilandGas.Models', 'Beep.OilandGas.PPDM.Models', 'Beep.OilandGas.PPDM39', 'Beep.OilandGas.PPDM39.DataManagement', 'TheTechIdeaWeb.Diagnostics.Abstractions') | ForEach-Object {
        '<Reference Include="' + $_ + '"><HintPath>' + (XmlPath (Join-Path $bin ($_.ToString() + '.dll'))) + '</HintPath></Reference>'
    }
} else {
    @('<Compile Include="' + $modelBase + '" />', '<Compile Include="' + $modelInterface + '" />', '<Compile Include="' + $systemRecord + '" />', '<Compile Include="' + $companyEnvironment + '" />', '<Compile Include="' + $ghgSelection + '" />', '<Compile Include="' + (XmlPath (Join-Path $root 'Beep.OilandGas.Models/Data/DataManagement/BeepConnectionName.cs')) + '" />',
      '<Compile Include="' + (XmlPath (Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Models/SystemSetupConfiguration.cs')) + '" />', '<Compile Include="' + (XmlPath (Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Models/CompanyEnvironmentIdentity.cs')) + '" />', '<Compile Include="' + (XmlPath (Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Models/CompanyEnvironmentPreview.cs')) + '" />', '<Compile Include="' + (XmlPath (Join-Path $root 'Beep.OilandGas.Models/Data/AccessControl/CompanyAccessModels.cs')) + '" />',
      '<Compile Include="' + (XmlPath (Join-Path $root 'Beep.OilandGas.PPDM39/Core/PPDMDataSourceTransaction.cs')) + '" />', '<Compile Include="' + (XmlPath (Join-Path $root 'Beep.OilandGas.PPDM39/Core/BeepOperationResult.cs')) + '" />')
}
$constants = if ($SourceBuild) { 'WORKFLOW_INBOX_FOCUSED;BEEP_SOURCE_TEST;BEEP_SECURE_CONNECTIONS' } else { 'WORKFLOW_INBOX_FOCUSED' }
$hseReferences = ''
if ($HseLifecycleTestAssembly) {
    if (-not $SourceBuild) { throw 'The HSE SQLite check requires SourceBuild.' }
    $hseAssembly = XmlPath (Resolve-Path -LiteralPath $HseLifecycleTestAssembly).Path
    $hseTest = XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/CompanyHseWorkflowSqliteTests.cs')
    $constants += ';HSE_SQLITE_FOCUSED'
    $hseReferences = '<Reference Include="HseLifecycleTestAssembly"><Aliases>purchase</Aliases><HintPath>' + $hseAssembly + '</HintPath></Reference>' +
        '<PackageReference Include="Stub.System.Data.SQLite.Core.NetStandard" Version="1.0.119" /><Compile Include="' + $hseTest + '" />'
    $env:OILGAS_SQLITE_CONNECTOR = (Resolve-Path -LiteralPath (Join-Path (Split-Path $root -Parent) 'BeepDataSources/DataSourcesPluginsCore/SqliteDatasourceCore/bin/Debug/net10.0/SqliteDatasourceCore.dll')).Path
}
$project = Join-Path $work 'BeepLoader.csproj'
@"
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>$constants</DefineConstants><GeneratePackageOnBuild>false</GeneratePackageOnBuild></PropertyGroup>
<ItemGroup>
<FrameworkReference Include="Microsoft.AspNetCore.App" />
<Using Include="Microsoft.Extensions.Logging" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" />
<PackageReference Include="Moq" Version="4.20.72" />
$beepReferences
$hseReferences
<Compile Include="$test" /><Compile Include="$unitTest" /><Compile Include="$service" /><Compile Include="$item" /><Compile Include="$refusal" /><Compile Include="$kind" /><Compile Include="$probe" /><Compile Include="$target" /><Compile Include="$selection" /><Compile Include="$selectionContract" />
$($modelReferences -join "`n")
</ItemGroup>
</Project>
"@ | Set-Content -LiteralPath $project -Encoding utf8
$env:OILGAS_TEST_NUGET_FEED = (Resolve-Path -LiteralPath $PackageSource).Path
$env:OILGAS_TEST_APPSETTINGS = (Resolve-Path -LiteralPath (Join-Path $root 'Beep.OilandGas.ApiService/appsettings.json')).Path
if ($SourceBuild) { $env:OILGAS_TEST_SQL_SOURCE = (Resolve-Path -LiteralPath (Join-Path (Split-Path $root -Parent) 'BeepDataSources/DataSourcesPluginsCore/SQlServerDataSourceCore/bin/Debug/net10.0/SqlServerDataSourceCore.dll')).Path }
if ($BundledSqlServer) { $env:OILGAS_TEST_BUNDLED_DRIVER = '1' }
Update-OilGasRestoredFixtureProjectFile -ProjectPath $project -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
dotnet restore $project --source $PackageSource --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet build $project --no-restore --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
if ($PublicDriverDownload -and $IsolatedDriverFeed) { throw 'Choose either a public download or an isolated local driver feed.' }
if ($VerifySqlServerRestore -and (-not $IsolatedDriverFeed -or $BundledSqlServer -or $Filter -ne 'FullyQualifiedName~WizardInstallsPackagedSqlServerConnectorWithSerializableOwnership')) {
    throw 'Fresh-process restore requires the isolated SQL Server install test and no bundled SQL Server connector.'
}
if ($PublicDriverDownload) {
    if (-not $SourceBuild) { throw 'The public driver download check requires SourceBuild.' }
    $env:OILGAS_TEST_NETWORK_DOWNLOAD = '1'
    $env:NUGET_PACKAGES = Join-Path $env:TEMP ('oilgas-public-packages-' + [Guid]::NewGuid().ToString('N'))
    $env:NUGET_HTTP_CACHE_PATH = Join-Path $env:TEMP ('oilgas-public-http-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $env:NUGET_PACKAGES | Out-Null
}
if ($IsolatedDriverFeed) {
    if (-not $SourceBuild -or -not $IsolatedDriverVersion) { throw 'The isolated driver test requires SourceBuild and an explicit package version.' }
    $env:OILGAS_TEST_ISOLATED_DRIVER_FEED = (Resolve-Path -LiteralPath $IsolatedDriverFeed).Path
    $env:OILGAS_TEST_PACKAGED_VERSION = $IsolatedDriverVersion
    $env:NUGET_PACKAGES = Join-Path $env:TEMP ('oilgas-package-consumer-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $env:NUGET_PACKAGES | Out-Null
}
dotnet test $project --no-build --no-restore --filter $Filter --logger 'console;verbosity=normal'
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
if ($VerifySqlServerRestore) {
    # A separate test host ensures restore starts with a fresh assembly catalog and process.
    dotnet test $project --no-build --no-restore --filter 'FullyQualifiedName~FreshProcessRestoresDownloadedSqlServerConnectorFromCache' --logger 'console;verbosity=normal'
}
exit $LASTEXITCODE
