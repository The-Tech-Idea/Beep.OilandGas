param([string]$ModelsAssemblyPath)
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference='Stop'
$taskRoot=Split-Path $PSScriptRoot -Parent
$taskDependencies=Join-Path $taskRoot 'Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0'
if(!$ModelsAssemblyPath){$ModelsAssemblyPath=Join-Path $taskDependencies 'Beep.OilandGas.Models.dll'}
$taskDirectory=Join-Path $env:TEMP ('oilgas-commercial-land-'+[Guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $taskDirectory)
Copy-Item -LiteralPath (Join-Path $taskRoot 'Beep.OilandGas.Web/Components/Workflow/CommercialLandPortfolio.razor') -Destination (Join-Path $taskDirectory 'CommercialLandPortfolio.razor')
@'
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using MudBlazor
'@ | Set-Content -LiteralPath (Join-Path $taskDirectory '_Imports.razor') -Encoding utf8
[xml]$taskProject='<Project Sdk="Microsoft.NET.Sdk.Razor"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><IsTestProject>true</IsTestProject><DefineConstants>COMMERCIAL_LAND_FOCUSED</DefineConstants><RootNamespace>Beep.OilandGas.Web</RootNamespace></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="MudBlazor" Version="8.15.0"/><PackageReference Include="Moq" Version="4.20.72"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup></Project>'
$taskItems=$taskProject.SelectSingleNode('/Project/ItemGroup')
[xml]$taskWeb=Get-Content -LiteralPath (Join-Path $taskRoot 'Beep.OilandGas.Web/Beep.OilandGas.Web.csproj') -Raw
$taskProject.SelectSingleNode('/Project/ItemGroup/PackageReference[@Include="MudBlazor"]').SetAttribute('Version',(@($taskWeb.Project.ItemGroup.PackageReference | Where-Object Include -eq 'MudBlazor')[0].Version))
foreach($taskAssembly in @('Beep.OilandGas.Models','DataManagementModels','Beep.OilandGas.PPDM.Models','Beep.OilandGas.Repository.Tests')){
 $taskReference=$taskProject.CreateElement('Reference');$taskReference.SetAttribute('Include',$taskAssembly)
 $taskHint=$taskProject.CreateElement('HintPath');$taskHint.InnerText=if($taskAssembly -eq 'Beep.OilandGas.Models'){(Resolve-Path -LiteralPath $ModelsAssemblyPath).Path}else{Join-Path $taskDependencies ($taskAssembly+'.dll')}
 [void]$taskReference.AppendChild($taskHint);[void]$taskItems.AppendChild($taskReference)
}
foreach($taskSource in @('Beep.OilandGas.Web/Services/LeasePortfolioState.cs','Beep.OilandGas.Repository.Tests/LeasePortfolioUiTests.cs','Beep.OilandGas.Web.Auth.Tests/CommercialLandPortfolioTests.cs')){
 $taskCompile=$taskProject.CreateElement('Compile');$taskCompile.SetAttribute('Include',(Join-Path $taskRoot $taskSource));[void]$taskItems.AppendChild($taskCompile)
}
$taskProjectPath=Join-Path $taskDirectory 'CommercialLand.csproj';Add-OilGasRestoredOwnerReferences -Project $taskProject -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
$taskProject.Save($taskProjectPath)
dotnet test $taskProjectPath --verbosity minimal
if($LASTEXITCODE -ne 0){throw "Commercial land focused checks failed: $taskDirectory"}
if((Get-FileHash -LiteralPath (Join-Path $taskDirectory 'CommercialLandPortfolio.razor')).Hash -ne (Get-FileHash -LiteralPath (Join-Path $taskRoot 'Beep.OilandGas.Web/Components/Workflow/CommercialLandPortfolio.razor')).Hash){throw 'Component changed during focused checks.'}
Write-Output "Exact component and current lease portfolio state checked: $taskDirectory. Browser/OIDC/live API/provider acceptance remains separate."
