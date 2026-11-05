# Source-bound conflict admission

Owner: [implementation 351](../../Implementation-351-Approved-Program-Drilling-Execution.md). Goal active, phase partial; 350 remains the last promoted journey. No staging/commit/push.

This changes the existing API admission helper to accept the canonical DrillingProgramBasis. Draft submission supplies its original basis; conflict inspection supplies the original approved-program basis. Admission checks selected company/account, core and execution binding, exact well and current original FIELD rights immediately. The eager funding/well Access.Read is removed from conflict inspection. Both complete opening/closing execution-basis callbacks, native exact-original absence and competing-population checks, final current authority and binding checks remain. No cache, alternate read owner, SQL, business EF, domain/schema/UI change or increased timeout.

Run the affected focused gates from the repository root against the unchanged current Data DLL:

```powershell
./Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath ./TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~DrillingExecutionConflictNativeTests|FullyQualifiedName~DrillingProgramDraftNativeTests|FullyQualifiedName~DrillingExecutionStartNativeTests|FullyQualifiedName~DrillingExecutionConflictStateTests'
./Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath ./TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~FocusedConflictUsesCurrentOriginalFieldReadAuthorityAndPreservesClosingSourceChecks'
./Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath ./TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~ProtectedTypedBasisCreationAndReadOnlyRecoveryUseTheSameApprovedSourceAndNativeProgram'
```

The captured draft HTTP gate reuses the completed 78-case runtime with `dotnet test <project> --no-build --no-restore --filter <draft-case> --logger console;verbosity=normal`; production admission sources are the same. That runtime precedes only the stronger test-only during-native-read program mutation. Runtime paths, logs and hashes are recorded in observed-evidence.json. A first targeted conflict runtime was intentionally stopped to strengthen its program-change check from pre-opening denial to mutation after opening source review; its partial log is construction evidence only.

The focused conflict case shares the existing actual configured approval/start setup but returns before assignment/report work. It uses an original readonly unmapped account and inactive well, inspects/confirms a different recorded original, refuses already-recorded original/changed proof, denies a revoked FIELD before inspection, revokes FIELD during a native read and mutates program approval evidence during a later native read. Native execution write counts and finance/WELL invariants are checked. The existing draft case separately verifies actual typed creation/list, readonly recovery, forbidden repeat write, retired route refusal and anonymous denial.

Read built-in request completion timings from the captured logs. Historical 81.805/71.437-second inspection/confirmation timings are from the earlier expanded journey. The focused fixture omits later assignment/report data and runs with its own setup, so the comparison is not a controlled causal benchmark or production performance qualification. The unchanged 100-second client timeout, before/after freshness checks and failure assertions must not be relaxed to obtain a pass. Current timings must come from terminal captured results, not these reproduction notes.

External Beep providers, resource/authentication seams are controlled; app RBAC/registry SQLite, canonical source readers, typed HTTP and native command owners are real within the fixture. Full Web/OIDC, provider isolation/concurrency/LOB/migration, browser/RTL/a11y, multinational/load/notification and production performance remain separate open gates. No browser rerun is credited for this backend-only change; prior actual UI checks retain their original UI/runtime scope.
