# Current reporting generation

Owner: [implementation 351](../../Implementation-351-Approved-Program-Drilling-Execution.md). Full 351 and goal remain partial; 350 is last promoted. No staging, commit or push before the full goal and plan finish.

The canonical Data descriptor supplies current manager, crew, stage and task IDs while retaining the immutable initial assignment. Native closure and recovery replay the transfer chain. Actual management/receiver pages distinguish current responsibility from initial assignment history. Subsequent public transfer catalog/API/client/journal/form implementation and qualification are tracked in the [transfer app package](../351-reporting-transfer-app/REPRODUCE.md).

Run the affected focused checks using the current Data assembly:

```powershell
dotnet build TheTechIdea.Data/TheTechIdea.Data.csproj --no-restore
./Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath ./TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~DrillingReportingReassignmentNativeTests.ChangedManagementRoleCanTakeOverUnchangedCrewWithoutInventingAnotherHandoff|FullyQualifiedName~DrillingReportingReassignmentNativeTests.CurrentAdmittedManagerMayDifferFromInitialSenderAndKeepsBothAccountsAudit|FullyQualifiedName~DrillingReportingClosureNativeTests.ClosureAfterTransferClosesOnlyCurrentGenerationAndEarlierTransferStillRecovers|FullyQualifiedName~DrillingReportingClosureNativeTests.ClosureCompletesOnlyOriginalReportingTasksAndPreservesAcceptanceAndAssignmentRecovery'
./Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath ./TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~DrillingReportingClosureStateTests|FullyQualifiedName~DrillingExecutionAssignedStateTests|FullyQualifiedName~DrillingAssignedCompletionStateTests'
./Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath ./TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~DrillingExecutionStartApiTests.FocusedReportingClosureUsesManagerAuthorityAndReadOnlyRecoveryThenRemovesOnlyOwnedPendingWork'
```

[Observed evidence](observed-evidence.json) records terminal results and running handles separately. The first 40-case screen gate failed one obsolete heading assertion, with 39 passes. The exact corrected case has a separate passing run. A later three-case actual-page gate covers the final current-task wording. These are overlapping gates, not a combined current-source full-filter pass. Portable PDB checksums identify actual compiled source and subsequent differences. Native and API status must be read from their recorded terminal results, not inferred from partial passing output.

The API lost-ack fixture keeps its injected fault armed until the actual commit hook fires before retry. Caller timeout does not prove server completion. The client request timeout remains 100 seconds and the outside-transaction resource assertion remains enforced. Earlier API runtime results do not qualify current contracts.

Controlled Beep/provider/auth/resource seams and cached helper assemblies qualify local behavior only. No current provider/schema/migration/isolation/concurrency/LOB, cross-source atomicity, real physical certificate, transferred-recipient protected HTTP/browser/full host/OIDC/notification/multinational or production-performance acceptance is inferred. No SQL, new domain EF, schema/table, parallel ledger or compatibility path was added.
