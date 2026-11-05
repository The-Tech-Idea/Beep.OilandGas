param([string]$DataAssemblyPath)
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
if (-not $DataAssemblyPath -or -not (Test-Path -LiteralPath $DataAssemblyPath)) { throw 'Provide the actual compiled current Data assembly.' }
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) "oilgas-journal-render-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $checkDirectory)
# Actual Razor component, actual MudBlazor version, explicit static-render JS fixture. No production dependency replacements.
[xml]$project = '<Project Sdk="Microsoft.NET.Sdk.Razor"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable><NoWarn>$(NoWarn);CS0436</NoWarn></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="MudBlazor" Version="9.7.0"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup><ItemGroup/></Project>'
[xml]$webProject = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Beep.OilandGas.Web/Beep.OilandGas.Web.csproj')
$mudVersion = $webProject.SelectSingleNode("/Project/ItemGroup/PackageReference[@Include='MudBlazor']").GetAttribute('Version')
if ([string]::IsNullOrWhiteSpace($mudVersion)) { throw 'The Web project must declare its actual MudBlazor version.' }
$project.SelectSingleNode("/Project/ItemGroup/PackageReference[@Include='MudBlazor']").SetAttribute('Version',$mudVersion)
$group = $project.SelectNodes('/Project/ItemGroup')[1]
$test = $project.CreateElement('Compile'); $test.SetAttribute('Include',(Join-Path $repositoryRoot 'Beep.OilandGas.Web.Auth.Tests/JournalSourceReviewPanelTests.cs')); [void]$group.AppendChild($test)
$razor = $project.CreateElement('RazorComponent'); $razor.SetAttribute('Include',(Join-Path $repositoryRoot 'Beep.OilandGas.Web/Components/BusinessProcess/JournalSourceReviewPanel.razor')); $razor.SetAttribute('Link','Components/JournalSourceReviewPanel.razor'); [void]$group.AppendChild($razor)
$reference = $project.CreateElement('Reference'); $reference.SetAttribute('Include','TheTechIdea.Data')
$hint = $project.CreateElement('HintPath'); $hint.InnerText = (Resolve-Path -LiteralPath $DataAssemblyPath).Path
[void]$reference.AppendChild($hint); [void]$group.AppendChild($reference)
$maintenanceTest = $project.CreateElement('Compile'); $maintenanceTest.SetAttribute('Include',(Join-Path $repositoryRoot 'Beep.OilandGas.Web.Auth.Tests/WorkOrderSourceReviewPanelTests.cs')); [void]$group.AppendChild($maintenanceTest)
$maintenanceRazor = $project.CreateElement('RazorComponent'); $maintenanceRazor.SetAttribute('Include',(Join-Path $repositoryRoot 'Beep.OilandGas.Web/Components/BusinessProcess/WorkOrderSourceReviewPanel.razor')); $maintenanceRazor.SetAttribute('Link','Components/WorkOrderSourceReviewPanel.razor'); [void]$group.AppendChild($maintenanceRazor)
foreach ($component in @('Beep.OilandGas.Web/Components/BusinessProcess/ProcessStartSourcePanel.razor','Beep.OilandGas.Web/Components/BusinessProcess/ProcessDefinitionReviewPanel.razor','Beep.OilandGas.Web/Components/BusinessProcess/FinancialProcessCatalogPanel.razor','Beep.OilandGas.Web.Auth.Tests/Fixtures/FinancialCatalogRenderHost.razor')) {
    $financialRazor = $project.CreateElement('RazorComponent'); $financialRazor.SetAttribute('Include',(Join-Path $repositoryRoot $component)); $financialRazor.SetAttribute('Link',('Components/'+[IO.Path]::GetFileName($component))); [void]$group.AppendChild($financialRazor)
}
foreach ($source in @('TheTechIdea.Data/OilGas/ProcessStartSourceReview.cs','TheTechIdea.Data/OilGas/ProcessInstanceRequest.cs','Beep.OilandGas.Web/Services/ProcessDefinitionDetailState.cs','Beep.OilandGas.Web.Auth.Tests/ProcessDefinitionRenderingTests.cs','Beep.OilandGas.Web/Services/FinancialProcessCatalogState.cs','Beep.OilandGas.Client/App/CompanyClientContext.cs','Beep.OilandGas.Web.Auth.Tests/FinancialCatalogRenderingTests.cs')) {
    $financialSource=$project.CreateElement('Compile');$financialSource.SetAttribute('Include',(Join-Path $repositoryRoot $source));[void]$group.AppendChild($financialSource)
}
$financialModels=$project.CreateElement('Reference');$financialModels.SetAttribute('Include','Beep.OilandGas.Models')
$financialHint=$project.CreateElement('HintPath');$financialHint.InnerText=Join-Path $repositoryRoot 'Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/Beep.OilandGas.Models.dll';[void]$financialModels.AppendChild($financialHint);[void]$group.AppendChild($financialModels)
$projectPath = Join-Path $checkDirectory 'JournalRendering.csproj'; $project.Save($projectPath)
dotnet test $projectPath --logger 'console;verbosity=normal'
if ($LASTEXITCODE -ne 0) { throw "Journal source rendering check failed with exit $LASTEXITCODE." }
