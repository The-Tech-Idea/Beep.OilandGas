# Permit provider process qualification

This executable supports the exact Plan 354 restart and contention tests. It runs the
current native lifecycle and provider fixture from the focused test assembly.
It is a verification tool, with controlled company authority, not a server entry
point. The parent test starts three fresh processes against one isolated SQLite
database: commit without an acknowledged outcome, read-only recovery, then
read-only replay. Exit 73 is intentional for the commit stage.

The contention test prepares one frozen source, starts two independent native
writers behind a pre-transaction barrier, and audits from a fresh read-only
process. Its two inputs cover duplicate delivery of one original and distinct
originals for the same source. Verified serializable guard windows must not
overlap; duplicate delivery returns one shared outcome, while distinct originals
produce one result and one source-conflict refusal. Both retain one history and
receipt. This is a bounded submission check, not general stress certification.

Run from the repository root after reviewing the master tracker and active
[permit plan](../../Plans/FrameworkAppEnhancement/Implementation-354-Permit-Application-Workflow.md).
The reviewed Data/native/BeepDM assemblies must be built. Set
`OILGAS_TEST_PERMIT_DRIVER_FEED` to the local feed containing the reviewed SQLite
connector 1.0.59 and provider 1.0.119 packages. The recorded qualification uses the
local package artifact, not a verified public release.

```powershell
$dataPath = (Resolve-Path TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll).Path
$focusedArgs = @(
    '-p:FocusedCompanyTests=true',
    '-p:FocusedObligationPolicyTests=true',
    "-p:FocusedDataAssemblyPath=$dataPath",
    '-p:BuildProjectReferences=false'
)
dotnet build Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj --no-restore @focusedArgs
dotnet restore tools/PermitProviderWorker/PermitProviderWorker.csproj -p:NuGetAudit=false --ignore-failed-sources
dotnet build tools/PermitProviderWorker/PermitProviderWorker.csproj --no-restore -p:BuildProjectReferences=false
Copy-Item tools/PermitProviderWorker/bin/Debug/net10.0/Beep.OilandGas.PermitProviderWorker.dll Beep.OilandGas.Repository.Tests/bin/company-focused/Debug/net10.0/
Get-Content Plans/FrameworkAppEnhancement/Company-Plan-Index.md -TotalCount 3
Get-Content Plans/FrameworkAppEnhancement/Implementation-354-Permit-Application-Workflow.md -Tail 8
dotnet test Beep.OilandGas.Repository.Tests/Beep.OilandGas.Repository.Tests.csproj --no-build --no-restore @focusedArgs --filter 'FullyQualifiedName~InstalledPermitProviderSerializesConcurrentOriginalsAcrossProcesses|FullyQualifiedName~InstalledPermitProviderRecoversUnacknowledgedSubmissionAcrossProcesses|FullyQualifiedName~InstalledPermitProviderRollsBackThenRecoversExactNativeSubmission'
```

The test retains its isolated `oilgas-permit-restart-*` directory under the
temporary folder, including the database, original, process reports and worker
output. Contention runs similarly retain `oilgas-permit-race-*` directories with
both originals, barrier markers, owned windows and each worker report. Assertions cover exact original bytes, separate process IDs,
matching confirmed outcome hashes, one history/receipt and unchanged database
bytes during recovery/replay. It does not certify an HTTP network failure,
abrupt process termination, power loss, other providers/actions, stress load or production RBAC.
