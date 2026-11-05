param([string]$DataAssemblyPath)
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
if (-not $DataAssemblyPath) {
    $DataAssemblyPath = Join-Path $repositoryRoot 'Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/TheTechIdea.Data.dll'
}
if (-not (Test-Path -LiteralPath $DataAssemblyPath)) { throw 'Provide an actual compiled TheTechIdea.Data assembly; this check does not replace missing projects.' }
# Compile actual state/context sources with an explicit typed-client/auth fixture. No transport, Razor, host or OIDC claim.
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) "oilgas-process-transition-reviews-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $checkDirectory)
[xml]$project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable><NoWarn>$(NoWarn);CS0436</NoWarn></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup><ItemGroup/></Project>'
$group = $project.SelectNodes("/Project/ItemGroup")[1]
foreach ($source in @('TheTechIdea.Data/OilGas/ProcessStartSourceReview.cs','TheTechIdea.Data/OilGas/ProcessInstanceRequest.cs','TheTechIdea.Data/OilGas/ProcessDefinitionRevision.cs','Beep.OilandGas.ApiService/Services/ProcessStartSources.cs','Beep.OilandGas.ApiService.Tests/ProcessStartSourcesTests.cs','Beep.OilandGas.ApiService/Services/ProcessDefinitionStartReview.cs','Beep.OilandGas.ApiService.Tests/ProcessDefinitionStartReviewTests.cs','Beep.OilandGas.ApiService/Services/ProcessDefinitionCatalog.cs','Beep.OilandGas.ApiService.Tests/ProcessDefinitionCatalogTests.cs','TheTechIdea.Data/OilGas/ProcessTransitionReview.cs', 'TheTechIdea.Data/OilGas/WorkOrderParentAccess.cs', 'TheTechIdea.Data/OilGas/WorkOrderDTOs.cs', 'TheTechIdea.Data/OilGas/WorkOrderInspectionRules.cs', 'TheTechIdea.Data/OilGas/WorkOrderWorkflow.cs', 'TheTechIdea.Data/OilGas/WorkOrderSourceReview.cs', 'Beep.OilandGas.ApiService/Services/ProcessStepRoleChecks.cs', 'Beep.OilandGas.ApiService/Services/ProcessTransitionReviews.cs', 'Beep.OilandGas.ApiService.Tests/ProcessTransitionReviewTests.cs')) {
    $node = $project.CreateElement('Compile'); $node.SetAttribute('Include', (Join-Path $repositoryRoot $source)); [void]$group.AppendChild($node)
}
$reference = $project.CreateElement('Reference'); $reference.SetAttribute('Include', 'TheTechIdea.Data')
$hint = $project.CreateElement('HintPath'); $hint.InnerText = (Resolve-Path -LiteralPath $DataAssemblyPath).Path
[void]$reference.AppendChild($hint); [void]$group.AppendChild($reference)
foreach ($modelName in @('Beep.OilandGas.Models', 'Beep.OilandGas.PPDM.Models', 'DataManagementModels')) {
    $actualModelPath = Join-Path $repositoryRoot "Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/$modelName.dll"
    if (-not (Test-Path -LiteralPath $actualModelPath)) { throw "Actual model dependency is missing: $modelName" }
    $modelReference = $project.CreateElement('Reference'); $modelReference.SetAttribute('Include', $modelName)
    $modelHint = $project.CreateElement('HintPath'); $modelHint.InnerText = $actualModelPath
    [void]$modelReference.AppendChild($modelHint); [void]$group.AppendChild($modelReference)
}
$projectPath = Join-Path $checkDirectory 'ProcessTransitionReviews.csproj'; $project.Save($projectPath)
dotnet test $projectPath --logger 'console;verbosity=normal'
if ($LASTEXITCODE -ne 0) { throw "Process detail-state check failed with exit $LASTEXITCODE." }
