# Shared execution and report source admission

Owner: [implementation 351](../../Implementation-351-Approved-Program-Drilling-Execution.md). Full 351/goal remain partial; 350 remains last promoted. No staging/commit/push.

`CaptureExecutionReadAsync` now keeps one existing paired program-facts owner across opening and closing phases. Each phase still reads the complete fresh native program graph and current funding/well/program facts. Closing compares original/native/projected revisions and the exact execution basis. Source-bound capture avoids an eager duplicate funding/well read. Current account/company/standard roles/permissions/FIELD, core/planning/lifecycle/execution physical bindings and resource checks remain independent and fresh. No current source or authority is cached across requests. Native persistence, Data contracts/DLL, UI/typed clients, schema/Beep Migration and the 100-second HTTP timeout are unchanged.

Affected readers are execution status, report history/basis/pages, sender reporting-assignment review, and recipient assigned-work/inbox. Every affected return still performs the closing source confirmation. Report pages also perform their closing complete native history scan.

Run from repository root:

```powershell
./Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath ./TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~DrillingExecutionStartNativeTests|FullyQualifiedName~DrillingProgramReviewProgressTests|FullyQualifiedName~DrillingProgramReviewProgressStateTests|FullyQualifiedName~DrillingDailyReportPage|FullyQualifiedName~DrillingExecutionAssignmentNativeTests|FullyQualifiedName~DrillingExecutionAssignedStateTests'
./Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath ./TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~FocusedReportPagesBindCurrentReadAuthorityAndRejectChangedHistoryWithoutMutations'
./Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath ./TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~FocusedReportingSenderAndRecipientUsePairedCurrentExecutionReadAuthority'
```

The affected linked-source/native/state gate passes 144 cases in 324.780 seconds, zero failures/skips, exit 0. It includes complete 537-report qualification, hidden/corrupt histories, closing changes, fresh program facts, assignment and recipient/state checks. Its runtime compiles the production change and expanded report-page journey; the later added sender/recipient Fact and branch are only credited to their own HTTP runtime.

The protected page journey retains actual independent program approval and execution start, one real API-recorded report and canonical later persisted-provider fixtures for page population. Later reports are not claimed as HTTP-recorded writes. It checks readonly/unmapped/inactive-well pages, checked cursor/history, FIELD revocation before/during reads, a program approval changed during a report range, appended history, and current view/company/anonymous denial. The altered approval status is restored by its exact current native key after refusal; native/WELL/finance/lifecycle invariants remain required.

The separate protected sender/recipient journey reuses the actual existing assignment journey: configured sender and recipient roles/personas, lost-ack assignment recovery, readonly/unmapped sender recovery, exact recipient assigned route and qualified inbox task, recipient FIELD READ without drilling edit rights, other-account recovery denial and revoked-recipient denial. Native execution and finance/WELL effects remain checked.

Both protected HTTP journeys pass, one case each, zero failures/skips, exit 0. The report-page journey totals 926.268 seconds; sender/recipient/inbox totals 872.040 seconds. Successful page requests take 37.541/37.377/37.347/37.780 seconds with the unchanged 100-second timeout. These latencies still leave responsive-screen performance open. Each original tool session reached terminal success; no run was restarted or deliberately stopped in this slice. See observed-evidence.json and the permanent logs for exact statuses, timings and source/runtime capture.

These are linked-source tests, actual app RBAC/registry SQLite and controlled local Kestrel/typed-client fixtures. External Beep driver, authentication/resource and provider seams remain controlled. No new Data build or browser run is claimed because those sources/contracts are unchanged. Earlier browser evidence retains its separate runtime and component/controlled-response scope. Historical 58-64-second pages came from a different local run and are not a controlled causal benchmark.

Actual provider/schema/constraints/isolation/concurrency/LOB/Beep Migration, full host/OIDC/browser/RTL/a11y/load, production performance, multinational qualification, notification delivery and remaining correction/completion/WELL/finance/construction/handover work remain open.
