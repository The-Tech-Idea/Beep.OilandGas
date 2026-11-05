param([Parameter(Mandatory)][string]$ProviderDirectory)
$ErrorActionPreference='Stop'
$provider=(Resolve-Path -LiteralPath $ProviderDirectory).Path
$taskDirectory=Join-Path ([IO.Path]::GetTempPath()) ("oilgas-sqlite-probe-"+[Guid]::NewGuid().ToString('N'))
[void](New-Item -ItemType Directory -Path $taskDirectory)
$project='<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup><ItemGroup><PackageReference Include="System.Data.SQLite.Core" Version="1.0.119"/>'
foreach($name in @('DataManagementModels','DataManagementEngine','RDBDataSource','SqliteDatasourceCore')){
    $assembly=(Resolve-Path -LiteralPath (Join-Path $provider ($name+'.dll'))).Path
    Write-Host "$name sha256 $((Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash)"
    $project+='<Reference Include="'+$name+'"><HintPath>'+[Security.SecurityElement]::Escape($assembly)+'</HintPath></Reference>'
}
foreach($name in @('PPDMDataSourceTransaction.cs','BeepOperationResult.cs')){
    $source=(Resolve-Path -LiteralPath (Join-Path $PSScriptRoot ('../Beep.OilandGas.PPDM39/Core/'+$name))).Path
    $project+='<Compile Include="'+[Security.SecurityElement]::Escape($source)+'"/>'
}
$project+='</ItemGroup></Project>'
[IO.File]::WriteAllText((Join-Path $taskDirectory 'Probe.csproj'),$project)
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'BeepSqliteTransactionProbe.cs') -Destination (Join-Path $taskDirectory 'Program.cs')
dotnet run --project (Join-Path $taskDirectory 'Probe.csproj') -v:q
if($LASTEXITCODE -ne 0){throw 'The isolated Beep SQLite transaction probe failed.'}
