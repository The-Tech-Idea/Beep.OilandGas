param([Parameter(Mandatory)][string]$DataAssemblyPath)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$dataAssembly = (Resolve-Path -LiteralPath $DataAssemblyPath).Path
Push-Location $root
try {
    # Match the focused repository harness's package-mode Beep transaction boundary.
    dotnet build Beep.OilandGas.PPDM39/Beep.OilandGas.PPDM39.csproj -p:BuildProjectReferences=false -p:GeneratePackageOnBuild=false -p:UseBeepDMSource=false
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj -p:FocusedCompanyTests=true -p:FocusedObligationPolicyTests=true -p:FocusedDataAssemblyPath=$dataAssembly -p:BuildProjectReferences=false -p:GeneratePackageOnBuild=false -p:UseBeepDMSource=false --filter 'FullyQualifiedName~ObligationWriterUsesActualCompanyFieldPolicyWithinTheOwnedTransaction' --logger 'console;verbosity=normal'
    exit $LASTEXITCODE
}
finally { Pop-Location }
