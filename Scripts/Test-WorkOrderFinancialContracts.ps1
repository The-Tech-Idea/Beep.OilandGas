param([Parameter(Mandatory)][string]$DataAssemblyPath)
$ErrorActionPreference='Stop'
$repositoryRoot=Split-Path $PSScriptRoot -Parent
$dataAssembly=(Resolve-Path -LiteralPath $DataAssemblyPath).Path
$checkDirectory=Join-Path ([IO.Path]::GetTempPath()) ('oilgas-financial-contracts-'+[guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $checkDirectory)
foreach($assembly in @('Beep.OilandGas.Models','Beep.OilandGas.ProductionAccounting')){
    $directory=Join-Path $checkDirectory $assembly;[void](New-Item -ItemType Directory -Path $directory)
    [xml]$project='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Version>1.0.2</Version></PropertyGroup><ItemGroup/></Project>'
    $group=$project.SelectSingleNode('/Project/ItemGroup')
    $reference=$project.CreateElement('Reference');$reference.SetAttribute('Include','TheTechIdea.Data');$hint=$project.CreateElement('HintPath');$hint.InnerText=$dataAssembly;[void]$reference.AppendChild($hint);[void]$group.AppendChild($reference)
    $files=if($assembly -eq 'Beep.OilandGas.Models'){@('Beep.OilandGas.Models/Data/ProductionAccounting/ACCOUNTING_COST.cs')}else{@('Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AFE.cs','Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/AFE_LINE_ITEM.cs','Beep.OilandGas.ProductionAccounting/Data/ProductionAccounting/APPROVAL_WORKFLOW.cs')}
    foreach($file in $files){$compile=$project.CreateElement('Compile');$compile.SetAttribute('Include',(Join-Path $repositoryRoot $file));[void]$group.AppendChild($compile)}
    $project.Save((Join-Path $directory ($assembly+'.csproj')))
}
[xml]$test='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>WORK_ORDER_FINANCIAL_CONTRACTS_FOCUSED</DefineConstants></PropertyGroup><ItemGroup><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup><ItemGroup/></Project>'
$group=$test.SelectNodes('/Project/ItemGroup')[1]
$reference=$test.CreateElement('Reference');$reference.SetAttribute('Include','TheTechIdea.Data');$hint=$test.CreateElement('HintPath');$hint.InnerText=$dataAssembly;[void]$reference.AppendChild($hint);[void]$group.AppendChild($reference)
foreach($assembly in @('Beep.OilandGas.Models','Beep.OilandGas.ProductionAccounting')){$reference=$test.CreateElement('ProjectReference');$reference.SetAttribute('Include',(Join-Path $checkDirectory ($assembly+'/'+$assembly+'.csproj')));[void]$group.AppendChild($reference)}
$compile=$test.CreateElement('Compile');$compile.SetAttribute('Include',(Join-Path $repositoryRoot 'Beep.OilandGas.Web.Auth.Tests/WorkOrderFinancialContractOwnershipTests.cs'));[void]$group.AppendChild($compile)
$runtimeReference=$test.CreateElement('Reference');$runtimeReference.SetAttribute('Include','DataManagementModels')
$runtimeHint=$test.CreateElement('HintPath');$runtimeHint.InnerText=Join-Path $repositoryRoot 'Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/DataManagementModels.dll'
[void]$runtimeReference.AppendChild($runtimeHint);[void]$group.AppendChild($runtimeReference)
$testPath=Join-Path $checkDirectory 'FinancialContracts.csproj';$test.Save($testPath)
dotnet test $testPath --logger 'console;verbosity=normal'
if($LASTEXITCODE -ne 0){throw "Canonical financial contract/forwarder check failed: $checkDirectory"}
