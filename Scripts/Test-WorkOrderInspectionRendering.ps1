param([string]$DataAssemblyPath)
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
if (-not $DataAssemblyPath) {
    $DataAssemblyPath = Join-Path $repositoryRoot 'Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/TheTechIdea.Data.dll'
}
if (-not (Test-Path -LiteralPath $DataAssemblyPath)) { throw 'Provide an actual compiled TheTechIdea.Data assembly; this check does not replace missing projects.' }
# Actual inspection component, current Data sources and typed-client read fixture; no HTTP, parent-page or browser/authorization claim.
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) "oilgas-inspection-render-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $checkDirectory)
[xml]$project = '<Project Sdk="Microsoft.NET.Sdk.Razor"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable><NoWarn>$(NoWarn);CS0436</NoWarn></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="MudBlazor" Version="9.7.0"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup><ItemGroup/></Project>'
[xml]$webProject = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Beep.OilandGas.Web/Beep.OilandGas.Web.csproj')
$mudVersion = $webProject.SelectSingleNode("/Project/ItemGroup/PackageReference[@Include='MudBlazor']").GetAttribute('Version')
$project.SelectSingleNode("/Project/ItemGroup/PackageReference[@Include='MudBlazor']").SetAttribute('Version',$mudVersion)
$group = $project.SelectNodes("/Project/ItemGroup")[1]
$navigation = $project.CreateElement('Compile'); $navigation.SetAttribute('Include',(Join-Path $repositoryRoot 'Beep.OilandGas.Web/Services/WorkOrderNavigation.cs')); [void]$group.AppendChild($navigation)
foreach ($component in @('Beep.OilandGas.Web.Auth.Tests/Fixtures/WorkOrderQueueRenderHost.razor','Beep.OilandGas.Web/Components/BusinessProcess/WorkOrderQueuePanel.razor','Beep.OilandGas.Web/Components/BusinessProcess/WorkOrderInspectionPanel.razor','Beep.OilandGas.Web.Auth.Tests/Fixtures/WorkOrderInspectionRenderHost.razor')) {
    $razor = $project.CreateElement('RazorComponent'); $razor.SetAttribute('Include',(Join-Path $repositoryRoot $component)); $razor.SetAttribute('Link',('Components/'+[IO.Path]::GetFileName($component))); [void]$group.AppendChild($razor)
}

foreach ($source in @('Beep.OilandGas.Web/Services/WorkOrderListState.cs','Beep.OilandGas.Web.Auth.Tests/WorkOrderQueueRenderingTests.cs','Beep.OilandGas.Web.Auth.Tests/WorkOrderInspectionRenderingTests.cs','Beep.OilandGas.Web/Services/IWorkOrderServiceClient.cs','Beep.OilandGas.Web/Services/WorkOrderInspectionState.cs','Beep.OilandGas.Client/App/CompanyClientContext.cs')) {
    $node = $project.CreateElement('Compile'); $node.SetAttribute('Include', (Join-Path $repositoryRoot $source)); [void]$group.AppendChild($node)
}
$reference = $project.CreateElement('Reference'); $reference.SetAttribute('Include', 'TheTechIdea.Data')
$hint = $project.CreateElement('HintPath'); $hint.InnerText = (Resolve-Path -LiteralPath $DataAssemblyPath).Path
[void]$reference.AppendChild($hint); [void]$group.AppendChild($reference)
foreach ($modelName in @('Beep.OilandGas.Models', 'Beep.OilandGas.PPDM.Models', 'DataManagementModels', 'DataManagementEngine', 'Beep.OilandGas.PPDM39')) {
    $actualModelPath = Join-Path $repositoryRoot "Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/$modelName.dll"
    if (-not (Test-Path -LiteralPath $actualModelPath)) { throw "Actual model dependency is missing: $modelName" }
    $modelReference = $project.CreateElement('Reference'); $modelReference.SetAttribute('Include', $modelName)
    $modelHint = $project.CreateElement('HintPath'); $modelHint.InnerText = $actualModelPath
    [void]$modelReference.AppendChild($modelHint);
    if($modelName -eq "Beep.OilandGas.Models"){$aliases=$project.CreateElement("Aliases");$aliases.InnerText="legacyModels";[void]$modelReference.AppendChild($aliases)}
    [void]$group.AppendChild($modelReference)
}
$projectPath = Join-Path $checkDirectory 'InspectionRendering.csproj'; $project.Save($projectPath)
dotnet test $projectPath --logger 'console;verbosity=normal'
if ($LASTEXITCODE -ne 0) { throw "Inspection rendering check failed with exit $LASTEXITCODE." }
