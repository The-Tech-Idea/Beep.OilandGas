param([Parameter(Mandatory=$true)][string]$FixtureAssemblyPath)
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference='Stop'
$repositoryRoot=Split-Path $PSScriptRoot -Parent
$fixture=(Resolve-Path -LiteralPath $FixtureAssemblyPath).Path
$runtime=Split-Path $fixture -Parent
$directory=Join-Path ([IO.Path]::GetTempPath()) "oilgas-protected-tab-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $directory)
[xml]$project='<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><RootNamespace>WorkflowProtectedTab</RootNamespace><StartupObject>WorkflowProtectedTab.Host</StartupObject></PropertyGroup><ItemGroup/></Project>'
$group=$project.SelectSingleNode('/Project/ItemGroup')
$canonicalData=Join-Path $repositoryRoot 'Beep.OilandGas.Models/bin/Debug/net10.0/Beep.OilandGas.Models.dll'
$fixtureData=Join-Path $runtime 'Beep.OilandGas.Models.dll'
if((Get-FileHash -LiteralPath $canonicalData).Hash -ne (Get-FileHash -LiteralPath $fixtureData).Hash){throw 'Build a focused runtime using the current canonical Data assembly before starting its browser host.'}
$dataReferences=@{}
foreach($reference in [Reflection.Assembly]::LoadFile($canonicalData).GetReferencedAssemblies()){$dataReferences[$reference.Name]=$reference}
foreach($dll in Get-ChildItem -LiteralPath $runtime -Filter '*.dll'){
    $referencePath=$dll.FullName
    if($dataReferences.ContainsKey($dll.BaseName)){
        $required=$dataReferences[$dll.BaseName];$observed=[Reflection.AssemblyName]::GetAssemblyName($referencePath)
        if($observed.Version -lt $required.Version){
            $candidate=Join-Path (Split-Path $canonicalData) $dll.Name
            if(-not (Test-Path -LiteralPath $candidate) -or [Reflection.AssemblyName]::GetAssemblyName($candidate).FullName -ne $required.FullName){throw "The canonical Data dependency is unavailable: $($required.FullName)"}
            $referencePath=$candidate
            Write-Output "Canonical Data dependency: $($required.FullName) at $candidate"
        }
    }
    $node=$project.CreateElement('Reference');$node.SetAttribute('Include',$dll.BaseName)
    $hint=$project.CreateElement('HintPath');$hint.InnerText=$referencePath;[void]$node.AppendChild($hint);[void]$group.AppendChild($node)
}
[xml]$web=Get-Content -LiteralPath (Join-Path $repositoryRoot 'Beep.OilandGas.Web/Beep.OilandGas.Web.csproj')
$mud=$project.CreateElement('PackageReference');$mud.SetAttribute('Include','MudBlazor');$mud.SetAttribute('Version',$web.SelectSingleNode("/Project/ItemGroup/PackageReference[@Include='MudBlazor']").GetAttribute('Version'));[void]$group.AppendChild($mud)
# The focused fixture assembly already compiles the production registration owner and pages.
# Consume that single compiled owner here; compiling it again creates ambiguous extension methods.
foreach($file in Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'WorkflowProtectedTab') -File){Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $directory $file.Name)}
$styleDirectory=Join-Path $directory 'wwwroot/css';[void](New-Item -ItemType Directory -Path $styleDirectory -Force)
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'Beep.OilandGas.Web/wwwroot/css/app.css') -Destination (Join-Path $styleDirectory 'app.css')
$projectPath=Join-Path $directory 'WorkflowProtectedTab.csproj';Add-OilGasRestoredOwnerReferences -Project $project -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
$project.Save($projectPath)
Write-Output "Isolated browser host: $directory"
Write-Output "Fixture runtime: $runtime"
dotnet run --project $projectPath
if($LASTEXITCODE -ne 0){throw "Workflow browser host failed with exit $LASTEXITCODE."}
