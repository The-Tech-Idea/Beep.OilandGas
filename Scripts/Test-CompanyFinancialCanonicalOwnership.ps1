param([Parameter(Mandatory=$true)][string]$DataAssemblyPath)
$ErrorActionPreference='Stop'
$taskRepository=Split-Path $PSScriptRoot -Parent
$taskDependencies=Join-Path $taskRepository 'Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0'
$taskData=(Resolve-Path -LiteralPath $DataAssemblyPath).Path
$taskLegacy=Join-Path $taskDependencies 'Beep.OilandGas.Models.dll'
if(-not (Test-Path -LiteralPath $taskLegacy)){throw 'The pre-move model compatibility assembly is unavailable.'}
$taskCheckDirectory=Join-Path ([IO.Path]::GetTempPath()) "oilgas-financial-ownership-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $taskCheckDirectory)
[xml]$taskProject='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable><DefineConstants>WORKFLOW_INBOX_FOCUSED</DefineConstants></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup><ItemGroup/></Project>'
$taskGroup=$taskProject.SelectNodes('/Project/ItemGroup')[1]
$taskSource=$taskProject.CreateElement('Compile');$taskSource.SetAttribute('Include',(Join-Path $taskRepository 'Beep.OilandGas.ApiService.Tests/CompanyFinancialCanonicalOwnershipTests.cs'));[void]$taskGroup.AppendChild($taskSource)
$taskBoundary=$taskProject.CreateElement('Compile');$taskBoundary.SetAttribute('Include',(Join-Path $taskRepository 'Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/ModuleSchemaBoundary.cs'));[void]$taskGroup.AppendChild($taskBoundary)
foreach($taskReference in @(@{Name='TheTechIdea.Data';Path=$taskData},@{Name='Beep.OilandGas.Models';Path=$taskLegacy;Alias='legacyModels'},@{Name='Beep.OilandGas.PPDM.Models';Path=(Join-Path $taskDependencies 'Beep.OilandGas.PPDM.Models.dll')},@{Name='DataManagementModels';Path=(Join-Path $taskDependencies 'DataManagementModels.dll')})){
    if(-not (Test-Path -LiteralPath $taskReference.Path)){throw "Required compatibility dependency unavailable: $($taskReference.Name)"}
    $taskNode=$taskProject.CreateElement('Reference');$taskNode.SetAttribute('Include',$taskReference.Name)
    $taskHint=$taskProject.CreateElement('HintPath');$taskHint.InnerText=$taskReference.Path;[void]$taskNode.AppendChild($taskHint)
    if($taskReference.Alias){$taskAlias=$taskProject.CreateElement('Aliases');$taskAlias.InnerText=$taskReference.Alias;[void]$taskNode.AppendChild($taskAlias)}
    [void]$taskGroup.AppendChild($taskNode)
}
$taskProjectPath=Join-Path $taskCheckDirectory 'FinancialOwnership.csproj'
$taskProject.Save($taskProjectPath)
Write-Output "Financial ownership composition: $taskProjectPath"
dotnet test $taskProjectPath --filter CompanyFinancialCanonicalOwnershipTests --logger 'console;verbosity=normal'
exit $LASTEXITCODE
