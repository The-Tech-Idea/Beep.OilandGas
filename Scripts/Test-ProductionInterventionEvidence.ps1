param([string]$DataAssemblyPath)
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
if (-not $DataAssemblyPath -or -not (Test-Path -LiteralPath $DataAssemblyPath)) { throw 'Provide the actual compiled current Data assembly.' }
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) "oilgas-intervention-evidence-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $checkDirectory)
# Actual Razor component, actual MudBlazor version, explicit static-render JS fixture. No production dependency replacements.
[xml]$project = '<Project Sdk="Microsoft.NET.Sdk.Razor"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="MudBlazor" Version="9.7.0"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup><ItemGroup/></Project>'
[xml]$webProject = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Beep.OilandGas.Web/Beep.OilandGas.Web.csproj')
$mudVersion = $webProject.SelectSingleNode("/Project/ItemGroup/PackageReference[@Include='MudBlazor']").GetAttribute('Version')
if ([string]::IsNullOrWhiteSpace($mudVersion)) { throw 'The Web project must declare its actual MudBlazor version.' }
$project.SelectSingleNode("/Project/ItemGroup/PackageReference[@Include='MudBlazor']").SetAttribute('Version',$mudVersion)
$group = $project.SelectNodes('/Project/ItemGroup')[1]
$test = $project.CreateElement('Compile'); $test.SetAttribute('Include',(Join-Path $repositoryRoot 'Beep.OilandGas.Web.Auth.Tests/ProductionInterventionEvidenceTests.cs')); [void]$group.AppendChild($test)
foreach($source in @('Beep.OilandGas.Web/Services/ProductionInterventionDecisionState.cs','Beep.OilandGas.Client/App/CompanyClientContext.cs')){
    $node=$project.CreateElement('Compile');$node.SetAttribute('Include',(Join-Path $repositoryRoot $source));[void]$group.AppendChild($node)
}
$razor = $project.CreateElement('RazorComponent'); $razor.SetAttribute('Include',(Join-Path $repositoryRoot 'Beep.OilandGas.Web/Components/BusinessProcess/ProductionInterventionEvidence.razor')); $razor.SetAttribute('Link','Components/ProductionInterventionEvidence.razor'); [void]$group.AppendChild($razor)
$reference = $project.CreateElement('Reference'); $reference.SetAttribute('Include','TheTechIdea.Data')
$hint = $project.CreateElement('HintPath'); $hint.InnerText = (Resolve-Path -LiteralPath $DataAssemblyPath).Path
[void]$reference.AppendChild($hint); [void]$group.AppendChild($reference)
foreach($dependency in @('DataManagementModels','Beep.OilandGas.PPDM.Models')){
    $dependencyPath=Join-Path (Split-Path (Resolve-Path -LiteralPath $DataAssemblyPath).Path -Parent) "$dependency.dll"
    if(-not (Test-Path -LiteralPath $dependencyPath)){throw "Actual native contract dependency is unavailable: $dependencyPath"}
    $node=$project.CreateElement('Reference');$node.SetAttribute('Include',$dependency)
    $dependencyHint=$project.CreateElement('HintPath');$dependencyHint.InnerText=$dependencyPath;[void]$node.AppendChild($dependencyHint);[void]$group.AppendChild($node)
}
$projectPath = Join-Path $checkDirectory 'InterventionEvidenceRendering.csproj'; $project.Save($projectPath)
dotnet test $projectPath --logger 'console;verbosity=normal'
if ($LASTEXITCODE -ne 0) { throw "Intervention evidence rendering check failed with exit $LASTEXITCODE." }
