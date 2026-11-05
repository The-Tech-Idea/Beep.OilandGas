param([string]$ModelsAssemblyPath)
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
if (-not $ModelsAssemblyPath) {
    $ModelsAssemblyPath = Join-Path $repositoryRoot 'Beep.OilandGas.Models/bin/Debug/net10.0/Beep.OilandGas.Models.dll'
}
if (-not (Test-Path -LiteralPath $ModelsAssemblyPath)) { throw 'Provide an actual compiled restored Models assembly; this check does not replace missing projects.' }
# Compile actual state/context sources with an explicit typed-client/auth fixture. Actual check-projection and state sources; no provider, transport, Razor, host or OIDC claim.
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) "oilgas-period-checks-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $checkDirectory)
[xml]$project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup><ItemGroup/></Project>'
$group = $project.SelectNodes("/Project/ItemGroup")[1]
foreach ($source in @('Beep.OilandGas.Web/Services/AccountingPeriodCheckState.cs', 'Beep.OilandGas.Client/App/CompanyClientContext.cs', 'Beep.OilandGas.ApiService/Services/AccountingPeriodCheckReviews.cs', 'Beep.OilandGas.Accounting/Models/PeriodCloseChecklist.cs', 'Beep.OilandGas.Web.Auth.Tests/AccountingPeriodCheckTests.cs')) {
    $node = $project.CreateElement('Compile'); $node.SetAttribute('Include', (Join-Path $repositoryRoot $source)); [void]$group.AppendChild($node)
}
$reference = $project.CreateElement('Reference'); $reference.SetAttribute('Include', 'Beep.OilandGas.Models')
$hint = $project.CreateElement('HintPath'); $hint.InnerText = (Resolve-Path -LiteralPath $ModelsAssemblyPath).Path
[void]$reference.AppendChild($hint); [void]$group.AppendChild($reference)
$projectPath = Join-Path $checkDirectory 'PeriodCheckTests.csproj'; Add-OilGasRestoredOwnerReferences -Project $project -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
$project.Save($projectPath)
dotnet test $projectPath --logger 'console;verbosity=normal'
if ($LASTEXITCODE -ne 0) { throw "Accounting period check failed with exit $LASTEXITCODE." }
