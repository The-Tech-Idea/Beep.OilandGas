# Native completion test source inspection

Owner: [implementation 351](../../Implementation-351-Approved-Program-Drilling-Execution.md). Full plan and goal remain partial; 350 remains last promoted. No staging, commit or push before full goal/plan completion.

The assigned independent reviewer explicitly selects one reported test and the existing five-part PPDM `WELL_TEST` key. UWI comes from the exact original execution; source, test type, run and test number are entered explicitly. The current original company, standard role, mapped reviewer workspace, permissions, FIELD READ, program, execution, claim, pending task and complete native review graph qualify the API read. A stale displayed packet refuses before private test output.

The canonical drilling owner uses the existing PPDM model/repository and Beep source. It validates exact schema keys/columns, active test, recorded reference/result/date and creation/change audit, and checks complete row revisions, including values omitted from the screen. Fresh original well/FIELD and closing program/resource/native review checks bracket the read; another full native test read follows the closing external callback. Borrowed transactions, binding changes, unavailable/failed/duplicate/wrong-key evidence and changed/revoked sources refuse.

The actual reviewer parent uses the existing typed client and state. Native keys start empty. Lookup shows reported and native references/results separately, reference/calendar-date discrepancies, native audit and checked revision. A timezone-less native TEST_DATE stays a recorded date; a calendar-date comparison does not prove the reported UTC instant. No threshold, default numeric result or physical acceptance is inferred. Lookup records no decision, journal, task completion, WELL/financial update or production release. Existing retained decision recovery has priority. Company/persona/route/disposal changes clear and fence private comparisons.

From the repository root:

```powershell
dotnet build TheTechIdea.Data/TheTechIdea.Data.csproj --no-restore
./Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath ./TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~DrillingCompletionTestInspectionTests|FullyQualifiedName~DrillingCompletionTestInspectionStateTests|FullyQualifiedName~DrillingCompletionDecisionStateTests|FullyQualifiedName~DrillingCompletionAssignedStateTests'
./Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath ./TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~ActualReviewerScreenShowsExplicitLookupAuditAndMismatchWithoutAcceptance'
./Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath ./TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~FocusedCompletionDecisionsRecoverOriginalAndRouteOrderedReviewerInboxWithoutClosingExecution'
```

Terminal evidence is recorded in `observed-evidence.json`; live handles are progress only. Construction runs remain distinct: two fixture namespace compile failures, followed by a 58-case run with 55 passes/three fixture assertion failures because the reused native seed reports PASS rather than the protected API seed's FAIL. Corrected assertions check preservation of each actual original value; no production guard is weakened. A final overlapping screen gate fills the actual selection/key fields and clicks the actual lookup button.

The protected journey uses actual app RBAC/registry SQLite and typed Kestrel requests with controlled external native/auth/resource seams. The native laboratory row is a persisted-provider fixture, not an HTTP-created physical test. Positive lookup, stale task proof, opening FIELD denial and revocation during native lookup are checked before the existing ordered-decision/recovery/final rejection history journey. Test inspection cannot change native/WELL/finance/review rows.

This is read-only source qualification for the acceptance workflow. Durable independently accepted physical evidence/criteria, exact original acceptance recovery/history, accountable reassignment/reporting closure and separate WELL/finance/construction/production handover remain implementation work. No new schema, SQL, domain EF, ledger or compatibility path. Beep MigrationManager remains the schema owner; no provider migration was run. Focused local rendering/API/native checks do not establish full host/OIDC/browser/RTL/a11y, real-provider schema/LOB/isolation/concurrency, notification/multinational acceptance or production performance. No new Chromium run is claimed; request timeouts remain unchanged.
