param([Parameter(Mandatory)][string]$StateAssemblyPath, [Parameter(Mandatory)][string]$ModelsAssemblyPath)
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
# The state assembly must come from the current Test-WorkOrderListState gate, not a substituted host.
foreach ($path in @($StateAssemblyPath, $ModelsAssemblyPath)) {
    if (-not (Test-Path -LiteralPath $path)) { throw 'Provide the actual current focused-state and Data assemblies.' }
}
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) "oilgas-order-grant-page-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $checkDirectory)
@'
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Authorization
@using MudBlazor
'@ | Set-Content -LiteralPath (Join-Path $checkDirectory '_Imports.razor') -Encoding utf8
[xml]$project = '<Project Sdk="Microsoft.NET.Sdk.Razor"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><RootNamespace>Beep.OilandGas.Web</RootNamespace></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="MudBlazor" Version="9.7.0"/></ItemGroup></Project>'
[xml]$web = Get-Content -LiteralPath (Join-Path $repositoryRoot 'Beep.OilandGas.Web/Beep.OilandGas.Web.csproj')
$project.SelectSingleNode("/Project/ItemGroup/PackageReference").SetAttribute('Version', $web.SelectSingleNode("/Project/ItemGroup/PackageReference[@Include='MudBlazor']").GetAttribute('Version'))
$group = $project.SelectSingleNode('/Project/ItemGroup')
$source = Join-Path $repositoryRoot 'Beep.OilandGas.Web/Pages/Admin/AccessControl/AssetAccess.razor'
$hash = (Get-FileHash -LiteralPath $source).Hash
$razor = $project.CreateElement('RazorComponent'); $razor.SetAttribute('Include', $source); $razor.SetAttribute('Link', 'Pages/Admin/AccessControl/AssetAccess.razor'); [void]$group.AppendChild($razor)
$references = @{
    'WorkOrderListTests' = (Resolve-Path -LiteralPath $StateAssemblyPath).Path
    'Beep.OilandGas.Models' = (Resolve-Path -LiteralPath $ModelsAssemblyPath).Path
    'Beep.OilandGas.PPDM.Models' = Join-Path $repositoryRoot 'Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/Beep.OilandGas.PPDM.Models.dll'
    'DataManagementModels' = Join-Path $repositoryRoot 'Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/DataManagementModels.dll'
}
foreach ($name in $references.Keys) {
    $node = $project.CreateElement('Reference'); $node.SetAttribute('Include', $name)
    $hint = $project.CreateElement('HintPath'); $hint.InnerText = $references[$name]; [void]$node.AppendChild($hint); [void]$group.AppendChild($node)
}
$projectPath = Join-Path $checkDirectory 'WorkOrderGrantPage.csproj'; Add-OilGasRestoredOwnerReferences -Project $project -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
$project.Save($projectPath)
dotnet build $projectPath --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw "Actual grant page compilation failed: $checkDirectory" }
if ($hash -ne (Get-FileHash -LiteralPath $source).Hash) { throw 'The grant page changed during verification.' }
Write-Output "Actual company-admin grant page compiled against current focused state and Data: $checkDirectory. No host, HTTP, role or browser execution claim."
