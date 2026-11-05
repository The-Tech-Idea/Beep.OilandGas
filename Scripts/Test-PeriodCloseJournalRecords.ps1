param([string]$ModelsAssemblyPath)
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
if (-not $ModelsAssemblyPath) {
    $ModelsAssemblyPath = Join-Path $repositoryRoot 'Beep.OilandGas.Models/bin/Debug/net10.0/Beep.OilandGas.Models.dll'
}
if (-not (Test-Path -LiteralPath $ModelsAssemblyPath)) { throw 'Provide an actual compiled restored Models assembly; this check does not replace missing projects.' }
# Compile actual state/context sources with an explicit typed-client/auth fixture. No transport, Razor, host or OIDC claim.
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) "oilgas-close-record-status-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $checkDirectory)
[xml]$project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup><ItemGroup/></Project>'
$group = $project.SelectNodes("/Project/ItemGroup")[1]
foreach ($source in @('Beep.OilandGas.ProductionAccounting/Services/PeriodCloseJournalRecords.cs', 'Beep.OilandGas.ProductionAccounting/Constants/AccountingSourceModuleCodes.cs', 'Beep.OilandGas.ProductionAccounting/Constants/WorkflowReferenceCodes.cs', 'Beep.OilandGas.ApiService.Tests/PeriodCloseJournalRecordsTests.cs')) {
    $node = $project.CreateElement('Compile'); $node.SetAttribute('Include', (Join-Path $repositoryRoot $source)); [void]$group.AppendChild($node)
}
$reference = $project.CreateElement('Reference'); $reference.SetAttribute('Include', 'Beep.OilandGas.Models')
$hint = $project.CreateElement('HintPath'); $hint.InnerText = (Resolve-Path -LiteralPath $ModelsAssemblyPath).Path
[void]$reference.AppendChild($hint); [void]$group.AppendChild($reference)
foreach ($modelName in @('Beep.OilandGas.Models', 'Beep.OilandGas.PPDM.Models', 'DataManagementModels')) {
    $actualModelPath = Join-Path $repositoryRoot "Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/$modelName.dll"
    if (-not (Test-Path -LiteralPath $actualModelPath)) { throw "Actual model dependency is missing: $modelName" }
    $modelReference = $project.CreateElement('Reference'); $modelReference.SetAttribute('Include', $modelName)
    $modelHint = $project.CreateElement('HintPath'); $modelHint.InnerText = $actualModelPath
    [void]$modelReference.AppendChild($modelHint); [void]$group.AppendChild($modelReference)
}
$projectPath = Join-Path $checkDirectory 'PeriodCloseRecords.csproj'; Add-OilGasRestoredOwnerReferences -Project $project -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
$project.Save($projectPath)
dotnet test $projectPath --logger 'console;verbosity=normal'
if ($LASTEXITCODE -ne 0) { throw "Period close record check failed with exit $LASTEXITCODE." }
