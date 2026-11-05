param([string]$PackageSource = "$env:USERPROFILE/.nuget/packages")
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$work = Join-Path $env:TEMP ("oilgas-live-connection-probe-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work | Out-Null
function XmlPath([string]$path) { [System.Security.SecurityElement]::Escape($path) }
$probe = XmlPath (Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Services/ConnectionProbe.cs')
$target = XmlPath (Join-Path $root 'Beep.OilandGas.PPDM39.DataManagement/Core/ModuleSetup/MigrationConnectionTarget.cs')
$test = XmlPath (Join-Path $root 'Beep.OilandGas.ApiService.Tests/ConnectionProbeSqliteTests.cs')
$project = Join-Path $work 'LiveProbe.csproj'
@"
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup><TargetFramework>net10.0</TargetFramework><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><IsPackable>false</IsPackable><EnableDefaultCompileItems>false</EnableDefaultCompileItems><DefineConstants>WORKFLOW_INBOX_FOCUSED</DefineConstants></PropertyGroup>
<ItemGroup>
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.4.0" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="TheTechIdea.Beep.DataManagementEngine" Version="3.1.1" />
<PackageReference Include="TheTechIdea.Beep.DataManagementModels" Version="3.1.1" />
<PackageReference Include="TheTechIdea.Beep.RDBDataSource" Version="2.1.1" />
<PackageReference Include="TheTechIdea.Beep.SqliteDatasourceCore" Version="1.0.58" />
<Compile Include="$probe" /><Compile Include="$target" /><Compile Include="$test" />
</ItemGroup>
</Project>
"@ | Set-Content -LiteralPath $project -Encoding utf8
dotnet restore $project --source $PackageSource --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet test $project --no-restore --filter 'FullyQualifiedName~ConnectionProbeSqliteTests' --logger 'console;verbosity=normal'
exit $LASTEXITCODE
