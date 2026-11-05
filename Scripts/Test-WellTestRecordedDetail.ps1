param([string]$ModelsAssemblyPath)
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
if (-not $ModelsAssemblyPath -or -not (Test-Path -LiteralPath $ModelsAssemblyPath)) { throw 'Provide the actual compiled current Data assembly.' }
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) "oilgas-well-test-detail-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $checkDirectory)
# Actual Razor component, actual MudBlazor version, explicit static-render JS fixture. No production dependency replacements.
[xml]$project = '<Project Sdk="Microsoft.NET.Sdk.Razor"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable><RootNamespace>Beep.OilandGas.Web</RootNamespace><NoWarn>$(NoWarn);CS0436</NoWarn></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="MudBlazor" Version="9.7.0"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup><ItemGroup/></Project>'
[xml]$webProject = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Beep.OilandGas.Web/Beep.OilandGas.Web.csproj')
$mudVersion = $webProject.SelectSingleNode("/Project/ItemGroup/PackageReference[@Include='MudBlazor']").GetAttribute('Version')
if ([string]::IsNullOrWhiteSpace($mudVersion)) { throw 'The Web project must declare its actual MudBlazor version.' }
$project.SelectSingleNode("/Project/ItemGroup/PackageReference[@Include='MudBlazor']").SetAttribute('Version',$mudVersion)
$group = $project.SelectNodes('/Project/ItemGroup')[1]
$test = $project.CreateElement('Compile'); $test.SetAttribute('Include',(Join-Path $repositoryRoot 'Beep.OilandGas.Web.Auth.Tests/WellTestRecordedDetailTests.cs')); [void]$group.AppendChild($test)
$razor = $project.CreateElement('RazorComponent'); $razor.SetAttribute('Include',(Join-Path $repositoryRoot 'Beep.OilandGas.Web/Components/BusinessProcess/WellTestRecordedDetail.razor')); $razor.SetAttribute('Link','Components/WellTestRecordedDetail.razor'); [void]$group.AppendChild($razor)
$kpi = $project.CreateElement('RazorComponent'); $kpi.SetAttribute('Include',(Join-Path $repositoryRoot 'Beep.OilandGas.Web/Components/Shared/KpiCard.razor')); $kpi.SetAttribute('Link','Components/Shared/KpiCard.razor'); [void]$group.AppendChild($kpi)
$dto = $project.CreateElement('Compile'); $dto.SetAttribute('Include',(Join-Path $repositoryRoot 'Beep.OilandGas.Models/Data/Production/WellTestResponse.cs')); [void]$group.AppendChild($dto)
Set-Content -LiteralPath (Join-Path $checkDirectory '_Imports.razor') -Value '@using MudBlazor'
foreach ($name in @('Beep.OilandGas.Models','Beep.OilandGas.PPDM.Models','DataManagementModels')) {
    $node=$project.CreateElement('Reference');$node.SetAttribute('Include',$name);$path=$project.CreateElement('HintPath');$path.InnerText=Join-Path $repositoryRoot "Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/$name.dll";[void]$node.AppendChild($path);[void]$group.AppendChild($node)
}
$reference = $project.CreateElement('Reference'); $reference.SetAttribute('Include','Beep.OilandGas.Models')
$hint = $project.CreateElement('HintPath'); $hint.InnerText = (Resolve-Path -LiteralPath $ModelsAssemblyPath).Path
[void]$reference.AppendChild($hint); [void]$group.AppendChild($reference)
$projectPath = Join-Path $checkDirectory 'WellTestDetailRendering.csproj'; Add-OilGasRestoredOwnerReferences -Project $project -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
$project.Save($projectPath)
dotnet test $projectPath --logger 'console;verbosity=normal'
if ($LASTEXITCODE -ne 0) { throw "Well test detail rendering check failed with exit $LASTEXITCODE." }
