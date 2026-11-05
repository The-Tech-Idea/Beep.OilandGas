param([string]$DataAssemblyPath)
$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
if (-not $DataAssemblyPath) {
    $DataAssemblyPath = Join-Path $repositoryRoot 'Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/TheTechIdea.Data.dll'
}
if (-not (Test-Path -LiteralPath $DataAssemblyPath)) { throw 'Provide an actual compiled TheTechIdea.Data assembly; this check does not replace missing projects.' }
# Compile actual repository owners and mappings. SQLite tests use an explicit authority fixture; no HTTP, provider or OIDC claim.
$checkDirectory = Join-Path ([IO.Path]::GetTempPath()) "oilgas-workspace-store-$([Guid]::NewGuid().ToString('N'))"
[void](New-Item -ItemType Directory -Path $checkDirectory)
[xml]$project = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable></PropertyGroup><ItemGroup><FrameworkReference Include="Microsoft.AspNetCore.App"/><PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.9"/><PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.9"/><PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0"/><PackageReference Include="xunit" Version="2.9.3"/><PackageReference Include="xunit.runner.visualstudio" Version="3.1.5"/></ItemGroup><ItemGroup/></Project>'
$group = $project.SelectNodes("/Project/ItemGroup")[1]
foreach ($source in @('Beep.OilandGas.Repository/RepositoryDbContext.cs', 'Beep.OilandGas.Repository/RepositoryAccessService.cs', 'Beep.OilandGas.Repository/RepositoryRoleScope.cs', 'Beep.OilandGas.Repository/RepositoryPersonaService.cs', 'Beep.OilandGas.Repository/RepositoryPersonaService.Workspaces.cs', 'Beep.OilandGas.Repository/*Mapping.cs', 'Beep.OilandGas.Repository.Tests/RoleWorkspaceTests.cs', 'Beep.OilandGas.Repository.Tests/RepositoryPersonaTests.cs')) {
    $node = $project.CreateElement('Compile'); $node.SetAttribute('Include', (Join-Path $repositoryRoot $source)); [void]$group.AppendChild($node)
}
$reference = $project.CreateElement('Reference'); $reference.SetAttribute('Include', 'TheTechIdea.Data')
$hint = $project.CreateElement('HintPath'); $hint.InnerText = (Resolve-Path -LiteralPath $DataAssemblyPath).Path
[void]$reference.AppendChild($hint); [void]$group.AppendChild($reference)
$projectPath = Join-Path $checkDirectory 'PersonaWorkspaceStoreTests.csproj'; $project.Save($projectPath)
dotnet test $projectPath --logger 'console;verbosity=normal'
if ($LASTEXITCODE -ne 0) { throw "Persona workspace-store check failed with exit $LASTEXITCODE." }
