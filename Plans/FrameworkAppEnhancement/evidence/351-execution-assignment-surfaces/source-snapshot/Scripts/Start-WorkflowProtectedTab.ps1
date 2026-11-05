param([Parameter(Mandatory=$true)][string]$FixtureAssemblyPath)
$ErrorActionPreference='Stop'
$repositoryRoot=Split-Path $PSScriptRoot -Parent
$fixture=(Resolve-Path -LiteralPath $FixtureAssemblyPath).Path
$runtime=Split-Path $fixture -Parent
$directory=Join-Path ([IO.Path]::GetTempPath()) "oilgas-protected-tab-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $directory)
[xml]$project='<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><RootNamespace>WorkflowProtectedTab</RootNamespace><StartupObject>WorkflowProtectedTab.Host</StartupObject></PropertyGroup><ItemGroup/></Project>'
$group=$project.SelectSingleNode('/Project/ItemGroup')
foreach($dll in Get-ChildItem -LiteralPath $runtime -Filter '*.dll'){
    $node=$project.CreateElement('Reference');$node.SetAttribute('Include',$dll.BaseName)
    $hint=$project.CreateElement('HintPath');$hint.InnerText=$dll.FullName;[void]$node.AppendChild($hint);[void]$group.AppendChild($node)
}
[xml]$web=Get-Content -LiteralPath (Join-Path $repositoryRoot 'Beep.OilandGas.Web/Beep.OilandGas.Web.csproj')
$mud=$project.CreateElement('PackageReference');$mud.SetAttribute('Include','MudBlazor');$mud.SetAttribute('Version',$web.SelectSingleNode("/Project/ItemGroup/PackageReference[@Include='MudBlazor']").GetAttribute('Version'));[void]$group.AppendChild($mud)
# The focused fixture assembly already compiles the production registration owner and pages.
# Consume that single compiled owner here; compiling it again creates ambiguous extension methods.
foreach($file in Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'WorkflowProtectedTab') -File){Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $directory $file.Name)}
$projectPath=Join-Path $directory 'WorkflowProtectedTab.csproj';$project.Save($projectPath)
Write-Output "Isolated browser host: $directory"
Write-Output "Fixture runtime: $runtime"
dotnet run --project $projectPath
if($LASTEXITCODE -ne 0){throw "Workflow browser host failed with exit $LASTEXITCODE."}
