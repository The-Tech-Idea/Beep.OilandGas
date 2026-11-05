param([string]$Filter = 'FullyQualifiedName~SetupRepositorySqlServerTests|FullyQualifiedName~RepositoryRegistrationTests')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
dotnet build (Join-Path $root 'Beep.OilandGas.Repository/Beep.OilandGas.Repository.csproj') --no-restore -p:BuildProjectReferences=false -p:UseBeepDMSource=true --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
function XmlPath([string]$path) { [System.Security.SecurityElement]::Escape($path) }
$work = Join-Path $env:TEMP ('oilgas-identity-setup-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work | Out-Null
$repository = Join-Path $root 'Beep.OilandGas.Repository/bin/Debug/net10.0'
$data = Join-Path $root 'Beep.OilandGas.Models/bin/Debug/net10.0/Beep.OilandGas.Models.dll'
$diagnostics = Join-Path (Split-Path $root -Parent) 'Beep.Web/TheTechIdeaWeb.Diagnostics.Abstractions/bin/Debug/net10.0/TheTechIdeaWeb.Diagnostics.Abstractions.dll'
$project = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsTestProject>true</IsTestProject><DefineConstants>SETUP_SQLSERVER_FOCUSED</DefineConstants></PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.9" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.3" />
    <PackageReference Include="Oracle.EntityFrameworkCore" Version="10.23.26300" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.9" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" />
    <Reference Include="Beep.OilandGas.Repository"><HintPath>$(XmlPath (Join-Path $repository 'Beep.OilandGas.Repository.dll'))</HintPath></Reference>
    <Reference Include="Beep.OilandGas.Models"><HintPath>$(XmlPath $data)</HintPath></Reference>
    <Reference Include="TheTechIdeaWeb.Diagnostics.Abstractions"><HintPath>$(XmlPath $diagnostics)</HintPath></Reference>
    <Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService/Services/SetupRepositoryMigrationService.cs'))" />
    <Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Models/SetupRepositoryMigration.cs'))" />
    <Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/SetupRepositorySqlServerTests.cs'))" />
    <Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.Repository.Tests/RepositoryRegistrationTests.cs'))" />
    <Compile Include="$(XmlPath (Join-Path $root 'Beep.OilandGas.Repository.Tests/RecordingFailureReporter.cs'))" />
  </ItemGroup>
</Project>
"@
[IO.File]::WriteAllText((Join-Path $work 'IdentitySetup.csproj'), $project)
dotnet test (Join-Path $work 'IdentitySetup.csproj') --filter $Filter --nologo --verbosity normal
exit $LASTEXITCODE
