param([string]$ModelsAssemblyPath)
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
if ($ModelsAssemblyPath -and -not (Test-Path -LiteralPath $ModelsAssemblyPath)) { throw 'The requested Models assembly is missing.' }
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) ('oilgas-financial-contracts-' + [guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $checkDirectory)
[xml]$project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>WORK_ORDER_FINANCIAL_CONTRACTS_FOCUSED</DefineConstants></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup></Project>'
$compile = $project.CreateElement('Compile')
$compile.SetAttribute('Include', (Join-Path $repositoryRoot 'Beep.OilandGas.Web.Auth.Tests/WorkOrderFinancialContractOwnershipTests.cs'))
[void]$project.SelectSingleNode('/Project/ItemGroup').AppendChild($compile)
Add-OilGasRestoredOwnerReferences -Project $project -RepositoryRoot $repositoryRoot
$projectPath = Join-Path $checkDirectory 'FinancialContracts.csproj'
$project.Save($projectPath)
dotnet test $projectPath --logger 'console;verbosity=normal'
if ($LASTEXITCODE -ne 0) { throw "Module financial contract check failed: $checkDirectory" }
