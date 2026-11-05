param([string]$PackageSource = "$env:USERPROFILE/.nuget/packages")
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$work = Join-Path $env:TEMP ('oilgas-connection-edit-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work | Out-Null
function XmlPath([string]$path) { [System.Security.SecurityElement]::Escape($path) }
Copy-Item -LiteralPath (Join-Path $root 'Beep.OilandGas.Web/Components/PPDM39DatabaseWizard.razor') -Destination (Join-Path $work 'PPDM39DatabaseWizard.razor')
@'
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using MudBlazor
@using Beep.OilandGas.Web.Services
'@ | Set-Content -LiteralPath (Join-Path $work '_Imports.razor') -Encoding utf8
@'
using System.Net.Http.Json;
namespace TheTechIdeaWeb.Diagnostics { public interface IFailureReporter {} }
namespace TheTechIdeaWeb.Diagnostics.Notifications { public sealed class FailureWording {} }
namespace Beep.OilandGas.Client.App { public sealed class CompanyClientContext : IDisposable { public void Dispose() {} } }
namespace Beep.OilandGas.Web.Services {
 public sealed class OilGasApiFailures {}
 public sealed class OilGasCallFailures { public string Explain(Exception error, string operation, string sentence) => sentence; }
 public sealed class ApiClient(HttpClient http, TheTechIdeaWeb.Diagnostics.IFailureReporter failures, Beep.OilandGas.Client.App.CompanyClientContext company) {
  public async Task<TOut?> PostAsync<TIn,TOut>(string path,TIn value) { using var response = await http.PostAsJsonAsync(path,value); response.EnsureSuccessStatusCode(); return await response.Content.ReadFromJsonAsync<TOut>(); }
  public async Task<TOut?> PutAsync<TIn,TOut>(string path,TIn value) { using var response = await http.PutAsJsonAsync(path,value); response.EnsureSuccessStatusCode(); return await response.Content.ReadFromJsonAsync<TOut>(); }
 }
}
'@ | Set-Content -LiteralPath (Join-Path $work 'stubs.cs') -Encoding utf8
$models = Join-Path $root 'Beep.OilandGas.Models/bin/Debug/net10.0/Beep.OilandGas.Models.dll'
$beep = Join-Path (Split-Path $root -Parent) 'BeepDM/DataManagementModelsStandard/bin/Debug/net10.0/DataManagementModels.dll'
$data = Join-Path $root 'Beep.OilandGas.Models/bin/Debug/net10.0/Beep.OilandGas.Models.dll'
foreach ($path in @($models,$beep,$data)) { if (!(Test-Path -LiteralPath $path)) { throw "Build focused dependency: $path" } }
@"
<Project Sdk="Microsoft.NET.Sdk.Razor"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><RootNamespace>Beep.OilandGas.Web.Components</RootNamespace><DefineConstants>WORKFLOW_INBOX_FOCUSED</DefineConstants></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/><PackageReference Include="Moq" Version="4.20.72"/><PackageReference Include="bunit" Version="1.40.0"/><PackageReference Include="MudBlazor" Version="9.7.0"/><Reference Include="Beep.OilandGas.Models"><HintPath>$(XmlPath $models)</HintPath></Reference><Reference Include="DataManagementModels"><HintPath>$(XmlPath $beep)</HintPath></Reference><Reference Include="Beep.OilandGas.Models"><HintPath>$(XmlPath $data)</HintPath></Reference><Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.Web.Auth.Tests/ConnectionEditTests.cs'))"/></ItemGroup></Project>
"@ | Set-Content -LiteralPath (Join-Path $work 'Focused.csproj') -Encoding utf8
Update-OilGasRestoredFixtureProjectFile -ProjectPath (Join-Path $work 'Focused.csproj') -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
dotnet restore (Join-Path $work 'Focused.csproj') --source $PackageSource --ignore-failed-sources --verbosity quiet
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet test (Join-Path $work 'Focused.csproj') --no-restore --filter 'FullyQualifiedName~ConnectionEditTests' --nologo -clp:ErrorsOnly --logger 'console;verbosity=normal'
exit $LASTEXITCODE
