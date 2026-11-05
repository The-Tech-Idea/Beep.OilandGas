param([string]$PackageSource = "$env:USERPROFILE/.nuget/packages")
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$work = Join-Path $env:TEMP ('oilgas-migration-dispatch-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work | Out-Null
function XmlPath([string]$path) { [System.Security.SecurityElement]::Escape($path) }
# This focused host links the current migration action sources to already-built
# Data, Models, Repository and Beep model assemblies; it does not build the API.
foreach ($dependency in @(
    (Join-Path $root 'Beep.OilandGas.Models/bin/Debug/net10.0/Beep.OilandGas.Models.dll'),
    (Join-Path $root 'Beep.OilandGas.Repository/bin/Debug/net10.0/Beep.OilandGas.Repository.dll'),
    (Join-Path $root 'Beep.OilandGas.Models/bin/Debug/net10.0/Beep.OilandGas.Models.dll'),
    (Join-Path (Split-Path $root -Parent) 'BeepDM/DataManagementModelsStandard/bin/Debug/net10.0/DataManagementModels.dll')
)) { if (-not (Test-Path -LiteralPath $dependency -PathType Leaf)) { throw "Build the focused dependency first: $dependency" } }
$stub = Join-Path $work 'TestBoundaries.cs'
@'
using System.Security.Claims;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Beep.OilandGas.ApiService.Services
{
    public static class TestResolvedActor
    {
        public static string ActingUserId(this ClaimsPrincipal user) => user.FindFirst("party_id")?.Value
            ?? throw RefusalException.Forbidden("No resolved test account.");
    }
}
namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    [ApiController, Authorize(Roles = "Administrator"), Route("api/ppdm39/setup")]
    public partial class PPDM39SetupController : ControllerBase
    {
        private readonly IPPDM39SchemaMigrationService _schemaMigrationService;
        public PPDM39SetupController(IPPDM39SchemaMigrationService service) => _schemaMigrationService = service;
    }
}
'@ | Set-Content -LiteralPath $stub -Encoding utf8
$project = Join-Path $work 'MigrationDispatch.csproj'
@"
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>WORKFLOW_INBOX_FOCUSED;MIGRATION_CONTROLLER_FOCUSED</DefineConstants></PropertyGroup>
<ItemGroup>
<FrameworkReference Include="Microsoft.AspNetCore.App" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" />
<PackageReference Include="Moq" Version="4.20.72" />
<Reference Include="Beep.OilandGas.Models"><HintPath>$(XmlPath (Join-Path $root 'Beep.OilandGas.Models/bin/Debug/net10.0/Beep.OilandGas.Models.dll'))</HintPath></Reference>
<Reference Include="Beep.OilandGas.Repository"><HintPath>$(XmlPath (Join-Path $root 'Beep.OilandGas.Repository/bin/Debug/net10.0/Beep.OilandGas.Repository.dll'))</HintPath></Reference>
<Reference Include="Beep.OilandGas.Models"><HintPath>$(XmlPath (Join-Path $root 'Beep.OilandGas.Models/bin/Debug/net10.0/Beep.OilandGas.Models.dll'))</HintPath></Reference>
<Reference Include="DataManagementModels"><HintPath>$(XmlPath (Join-Path (Split-Path $root -Parent) 'BeepDM/DataManagementModelsStandard/bin/Debug/net10.0/DataManagementModels.dll'))</HintPath></Reference>
<Compile Include="$(XmlPath $stub)" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/MigrationAdministratorAuthority.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Controllers/PPDM39/PPDM39SetupController.Migrations.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/MigrationAdministratorAuthorityTests.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/MigrationDispatchAuthorityTests.cs'))" />
<Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/MigrationDispatchHttpTests.cs'))" />
</ItemGroup></Project>
"@ | Set-Content -LiteralPath $project -Encoding utf8
Update-OilGasRestoredFixtureProjectFile -ProjectPath $project -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
dotnet restore $project --source $PackageSource --ignore-failed-sources --verbosity quiet
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet test $project --no-restore --nologo --verbosity minimal
exit $LASTEXITCODE
