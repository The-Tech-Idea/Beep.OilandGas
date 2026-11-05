param([Parameter(Mandatory)][string]$ModelsAssemblyPath)
. (Join-Path $PSScriptRoot 'RestoredOwnerFixtureReferences.ps1')
$ErrorActionPreference='Stop'
$repositoryRoot=Split-Path $PSScriptRoot -Parent
$checkDirectory=Join-Path ([IO.Path]::GetTempPath()) ('oilgas-maintenance-capabilities-'+[guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $checkDirectory)
# Compile the actual seed/catalog sources against explicit existing dependencies; no seed or live grant runs.
[xml]$project='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable><NoWarn>$(NoWarn);CS0436</NoWarn></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup><ItemGroup/></Project>'
$group=$project.SelectNodes('/Project/ItemGroup')[1]
foreach($path in @('Beep.OilandGas.UserManagement/Services/DefaultSecuritySeedService.cs','Beep.OilandGas.Models/Data/Security/PermissionConstants.cs')){
    $entry=$project.CreateElement('Compile');$entry.SetAttribute('Include',(Join-Path $repositoryRoot $path));[void]$group.AppendChild($entry)
}
foreach($dependency in Get-ChildItem (Join-Path $repositoryRoot 'Beep.OilandGas.UserManagement/bin/Debug/net10.0') -Filter '*.dll'){
    if($dependency.Name -eq 'Beep.OilandGas.Models.dll'){continue}
    $entry=$project.CreateElement('Reference');$entry.SetAttribute('Include',$dependency.BaseName);$hint=$project.CreateElement('HintPath');$hint.InnerText=$dependency.FullName;[void]$entry.AppendChild($hint);[void]$group.AppendChild($entry)
}
$entry=$project.CreateElement('Reference');$entry.SetAttribute('Include','Beep.OilandGas.Models');$hint=$project.CreateElement('HintPath');$hint.InnerText=(Resolve-Path -LiteralPath $ModelsAssemblyPath).Path;[void]$entry.AppendChild($hint);$aliases=$project.CreateElement('Aliases');$aliases.InnerText='actualOwners';[void]$entry.AppendChild($aliases);[void]$group.AppendChild($entry)
Add-OilGasRestoredOwnerReferences -Project $project -RepositoryRoot (Split-Path $PSScriptRoot -Parent)
$project.Save((Join-Path $checkDirectory 'MaintenanceCapabilities.csproj'))
[IO.File]::WriteAllText((Join-Path $checkDirectory 'CapabilitySeedTests.cs'),@'
using System.Reflection;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.UserManagement.Services;
using Xunit;
public sealed class CapabilitySeedTests
{
    [Fact] public void DedicatedMaintenanceCapabilitiesAreDiscoverableButNoDefaultRoleReceivesThem()
    {
        var capabilities=new[]{PermissionConstants.Maintenance.ViewApprovedPermit,PermissionConstants.Maintenance.ExecuteWork,PermissionConstants.Maintenance.VerifyCompletion,PermissionConstants.Maintenance.ReturnToService};
        var type=typeof(DefaultSecuritySeedService);
        var catalog=(IEnumerable<string>)type.GetMethod("GetAllPermissionCodes",BindingFlags.NonPublic|BindingFlags.Static)!.Invoke(null,null)!;
        foreach(var capability in capabilities)Assert.Contains(capability,catalog);
        var roles=(Dictionary<string,string[]>)type.GetMethod("GetRolePermissionMap",BindingFlags.NonPublic|BindingFlags.Static)!.Invoke(null,null)!;
        Assert.NotEmpty(roles);foreach(var permissions in roles.Values)foreach(var capability in capabilities)Assert.DoesNotContain(capability,permissions);
        Assert.Contains(roles.Values,permissions=>permissions.Contains(PermissionConstants.Maintenance.AssignPersonnel));
    }
}
'@,[Text.UTF8Encoding]::new($false))
dotnet test (Join-Path $checkDirectory 'MaintenanceCapabilities.csproj') --logger 'console;verbosity=normal'
if($LASTEXITCODE -ne 0){throw "Actual maintenance capability seed gate failed: $checkDirectory"}
