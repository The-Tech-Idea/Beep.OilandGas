param([Parameter(Mandatory)][string]$ProviderDirectory,[Parameter(Mandatory)][string]$AppDependencyDirectory)
$ErrorActionPreference='Stop'
$provider=(Resolve-Path -LiteralPath $ProviderDirectory).Path
$app=(Resolve-Path -LiteralPath $AppDependencyDirectory).Path
$taskDirectory=Join-Path ([IO.Path]::GetTempPath()) ("oilgas-sqlite-migration-"+[Guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $taskDirectory)
$project='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><PackageReference Include="System.Data.SQLite.Core" Version="1.0.119"/><PackageReference Include="Moq" Version="4.20.72"/><PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="5.6.0"/>'
foreach($set in @(@($provider,@('DataManagementModels','DataManagementEngine','RDBDataSource','SqliteDatasourceCore')),@($app,@('TheTechIdea.Data','Beep.OilandGas.Models','Beep.OilandGas.PPDM.Models')))){
    foreach($name in $set[1]){
        $assembly=(Resolve-Path -LiteralPath (Join-Path $set[0] ($name+'.dll'))).Path
        Write-Host "$name sha256 $((Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash)"
        $project+='<Reference Include="'+$name+'"><HintPath>'+[Security.SecurityElement]::Escape($assembly)+'</HintPath></Reference>'
    }
}
$project+='</ItemGroup></Project>'
[IO.File]::WriteAllText((Join-Path $taskDirectory 'Probe.csproj'),$project)
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'BeepSqliteMigrationProbe.cs') -Destination (Join-Path $taskDirectory 'Program.cs')
dotnet run --project (Join-Path $taskDirectory 'Probe.csproj') -v:q
if($LASTEXITCODE -ne 0){throw 'The isolated Beep SQLite migration and exact audit probe failed.'}
