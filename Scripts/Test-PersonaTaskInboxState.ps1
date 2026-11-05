param([string]$ModelsAssemblyPath)
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
if (-not $ModelsAssemblyPath) {
    $ModelsAssemblyPath = Join-Path $repositoryRoot 'Beep.OilandGas.Models/bin/Debug/net10.0/Beep.OilandGas.Models.dll'
}
if (-not (Test-Path -LiteralPath $ModelsAssemblyPath)) { throw 'Provide an actual compiled restored Models assembly; this check does not replace missing projects.' }
# Compile actual state/context and the shared assigned-work panel, with explicit typed-client/auth/static-JS fixtures.
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) "oilgas-task-inbox-state-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $checkDirectory)
[xml]$project = '<Project Sdk="Microsoft.NET.Sdk.Razor"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup><ItemGroup/></Project>'
[xml]$webProject=Get-Content -LiteralPath (Join-Path $repositoryRoot 'Beep.OilandGas.Web/Beep.OilandGas.Web.csproj')
$mudVersion=$webProject.SelectSingleNode("/Project/ItemGroup/PackageReference[@Include='MudBlazor']").GetAttribute('Version')
if([string]::IsNullOrWhiteSpace($mudVersion)){throw 'Provide the declared Web MudBlazor version.'}
$mud=$project.CreateElement('PackageReference');$mud.SetAttribute('Include','MudBlazor');$mud.SetAttribute('Version',$mudVersion);[void]$project.SelectNodes('/Project/ItemGroup')[0].AppendChild($mud)
$group = $project.SelectNodes("/Project/ItemGroup")[1]
foreach($component in @('Beep.OilandGas.Web/Components/Workflow/PersonaAssignedWorkPanel.razor','Beep.OilandGas.Web.Auth.Tests/Fixtures/PersonaAssignedWorkRenderHost.razor')){
    $razor=$project.CreateElement('RazorComponent');$razor.SetAttribute('Include',(Join-Path $repositoryRoot $component));$razor.SetAttribute('Link',('Components/'+[IO.Path]::GetFileName($component)));[void]$group.AppendChild($razor)
}
foreach ($source in @('Beep.OilandGas.Web/Services/PersonaContextService.cs', 'Beep.OilandGas.Client/App/CompanyClientContext.cs', 'Beep.OilandGas.Web/Services/IUnifiedTaskInboxService.cs', 'Beep.OilandGas.Web/Services/PersonaTaskInboxState.cs', 'Beep.OilandGas.Web.Auth.Tests/PersonaTaskInboxStateTests.cs')) {
    $node = $project.CreateElement('Compile'); $node.SetAttribute('Include', (Join-Path $repositoryRoot $source)); [void]$group.AppendChild($node)
}
$reference = $project.CreateElement('Reference'); $reference.SetAttribute('Include', 'Beep.OilandGas.Models')
$hint = $project.CreateElement('HintPath'); $hint.InnerText = (Resolve-Path -LiteralPath $ModelsAssemblyPath).Path
[void]$reference.AppendChild($hint); [void]$group.AppendChild($reference)
$projectPath = Join-Path $checkDirectory 'PersonaTaskInboxTests.csproj'; Add-OilGasRestoredOwnerReferences -Project $project -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
$project.Save($projectPath)
dotnet test $projectPath --logger 'console;verbosity=normal'
if ($LASTEXITCODE -ne 0) { throw "Persona task-inbox check failed with exit $LASTEXITCODE." }
