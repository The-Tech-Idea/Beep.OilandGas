# Reproduce the scoped recovery-feedback gate

Run from the repository root in PowerShell:

```powershell
Scripts/Test-PurchaseOrderOwnership.ps1 -DataAssemblyPath TheTechIdea.Data/bin/Debug/net10.0/TheTechIdea.Data.dll -IncludeWorkflowInbox -TestFilter 'FullyQualifiedName~WorkflowComponentTransportTests|FullyQualifiedName~DrillingDailyReportStateTests|FullyQualifiedName~DrillingExecutionStateTests|FullyQualifiedName~DevelopmentDraftIntentStoreTests'
Scripts/Start-WorkflowProtectedTab.ps1 -FixtureAssemblyPath '<new-focused-runtime>/bin/Debug/net10.0/PurchaseOwnership.dll'
```

Use the installed Playwright CLI in a dedicated session and the emitted local host URL. Use actual MudBlazor inputs to select the configured role/unit and enter explicit completed UTC observations, measured depth, provenance and summary; review, then confirm. The controlled typed client records one report and simulates a lost acknowledgement. Reload, toggle the test-only view-only control and recover. Snapshot before each click so element references remain current.

Before Blazor caches bound storage methods, install browser/storage-fault-injection.js using the CLI run-code --filename command. Set window.__oilgasBlockRemoval=true before recovery to fail only owned key removal. Call the host's test-only POST /fixture/recovery-outage/true before another recovery to verify the prior receipt and checked UTC survive a failed lookup. Restore the outage and removal controls, then recover without another write. Metadata exposes call counts independently of UI text.

Exhaust real sessionStorage with uniquely owned filler keys until QuotaExceededError. Prepare another original: retention must prevent submission, show a page-held original and say no submission was sent from this page. Remove only those owned fillers and confirm absence through recovery. Separately set window.__oilgasBlockReadback=true before confirmation: the real protected Set succeeds but its readback throws; encrypted storage may contain the original, so the page must promise neither confirmed retention nor memory-only storage. Reload to a new state owner; it must conservatively allow prior submission and recover absent without another write.

The fixture has a separate start original. Recover it with removal blocked, check the validated start receipt and cleanup warning coexist, then restore removal and recover. Final snapshots/metadata must show exactly one controlled report record request, no owned pending/filler keys and no final browser console errors. Close only the dedicated browser and emitted temporary host.

browser/browser-verification.json verifies 12 saved observation sets. Business outcomes are controlled typed-client seams; native/API/provider/OIDC/RBAC/production route behavior is not established by this host. Readback/removal/outage failures are injected; quota exhaustion and protected storage are real Chromium. No full browser matrix, RTL/a11y, concurrent circuit or production performance qualification is claimed. The host wrapper's -1 exit reflects intentional teardown, not a passing test process. Focused test exit is separately 0. Prior native/API evidence stays in its original source/runtime scope.
