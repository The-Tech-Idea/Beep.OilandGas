param([Parameter(Mandatory)][string]$DataAssemblyPath,[string]$Filter='FullyQualifiedName~ActualOriginalMaintenanceRecoveryHttp')
$ErrorActionPreference='Stop'
$repositoryRoot=Split-Path $PSScriptRoot -Parent
$dataAssembly=(Resolve-Path -LiteralPath $DataAssemblyPath).Path
Push-Location $repositoryRoot
try {
    # The real repository transaction owner must match the current source-linked lifecycle callers.
    # Keep unrelated project builds skipped; never certify a stale runtime dependency as current.
    dotnet build Beep.OilandGas.PPDM39/Beep.OilandGas.PPDM39.csproj -p:BuildProjectReferences=false -p:GeneratePackageOnBuild=false -p:UseBeepDMSource=false
    if($LASTEXITCODE -ne 0){throw 'The current PPDM39 transaction dependency did not build.'}
    dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true -p:FocusedDataAssemblyPath=$dataAssembly -p:BuildProjectReferences=false -p:GeneratePackageOnBuild=false -p:UseBeepDMSource=false --filter $Filter --logger 'console;verbosity=normal'
    if($LASTEXITCODE -ne 0){throw 'The actual maintenance handoff HTTP gate failed.'}
}
finally {Pop-Location}
