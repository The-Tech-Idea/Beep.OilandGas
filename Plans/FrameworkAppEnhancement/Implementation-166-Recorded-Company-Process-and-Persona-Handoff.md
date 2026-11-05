# Implementation 166 — Recorded company process and persona handoff

Date: 2026-10-06. Status: native company initial graph/task composition and actual writer-to-inbox HTTP verification pass. Secure API submission, authoritative native preparer review and command screens remain open.

## Persona and company outcome

A company accountant/procurement handoff now has an explicit native producer in the existing PPDMProcessService. It records the frozen intent and company/source binding in the existing process payload and composes the complete initial process, steps, start history and persona tasks in one owned Beep Serializable transaction. The existing initial process factory and native repositories are reused; the task router participates through an internal boundary that requires the transaction helper's actual owned command. Its public standalone route method still owns its own transaction and refuses borrowed transactions.

CompanyAccountingProcessStart, its receipt and command contract live in TheTechIdea.Data. The intent includes original company, actor, stable intent ID, workflow/source identity, source/definition/binding revisions and recorded source preparer identity. No parallel workflow/task store or business entity/schema is created. The production fixed-target process factory passes the existing repository persona reader; full production DI/host remains unverified.

## Recorded creation and reconciliation

Process, step and start-history identities derive deterministically from original company/intent and recorded step identity. Unique active source-matched definitions and explicit unambiguous role assignments are required. The recorded definition snapshot and graph are reviewed again before commit. A different intent cannot create another active company process for the same source; active unowned company processes require reconciliation.

An unchanged intent confirms the complete original initial graph and task set without additional inserts. Changed actor/preparer/source/revisions, progressed processes, conflicting identities, missing/partial tasks, altered steps/audit/binding or changed definitions/persona destinations refuse adoption, silent repair or reopening. Every insertion failure rolls back the process and all assignments together. Post-commit graph/tasks and current review are confirmed before a receipt is returned. Uncertain commit can be confirmed by the same retained intent; missing committed records require reconciliation.

The method's required callback supplies the API-owned current authority review. This slice tests that boundary explicitly but does not provide the production submission endpoint or prove that supplied source revision/preparer came from authoritative AP/PO audit. It is not permission to trust frontend-supplied ownership/preparer/revisions. Original company/binding/write authority must be checked within the native boundary, while actual source review must remain with its PRODUCTION owner. The native transaction helper forbids crossing to another datasource while owning LIFECYCLE; cross-module source consistency and the appropriate review boundaries must be resolved in API integration rather than claiming an atomic PRODUCTION/LIFECYCLE snapshot.

## Independent review and shared-company queues

Existing standard-role step/transition checks now use recorded company start evidence for company AP/PO independent actions. Both the recorded process starter and source preparer are refused, including Administrators. Missing or inconsistent company preparer/start proof cannot unlock independent action. Actual native source preparer verification and protected company action endpoints remain open; these checks alone do not certify financial approval/posting.

The shared task reader can return persona work from multiple companies. The complete inbox controller now validates each company process's explicit Data-owned payload ownership before native AP/PO review. Confirmed foreign-company work is omitted, with full skipped-task/process snapshots checked again before returning the queue. Missing, mismatched, ambiguous or changed evidence remains a reported conflict. Foreign work no longer poisons a valid selected-company inbox and no foreign task/source link is returned. The prior Implementation 164 foreign writer-to-inbox 409 behavior is superseded: a stable confirmed foreign company task now produces a successful empty queue without querying its native invoice.

A new HTTP journey creates two company handoffs through the actual PPDMProcessService, routes tasks through the actual router and reads processes through the actual native process reader. Switching the app-owned company context returns exactly that company's recorded task and process detail/history/steps. This replaces process projection with the actual reader for this specific journey.

## Focused evidence

Actual Data source build: zero warnings/errors, 1.72 seconds. Log: %TEMP%/oilgas-company-start-data-166-final.log.

Final affected gate: Scripts/Test-PurchaseOrderOwnership.ps1 with the actual Data assembly and -IncludeWorkflowInbox. **630 passed, zero failed/skipped, terminal exit 0, 8.1199 seconds**. Log: %TEMP%/oilgas-company-start-166-final.log. Whitespace verification passes. The expanded composition has 295 dependency/source-precedence and nullable warnings and no compiler errors.

Coverage includes 34 new company-native cases, three new HTTP cases and 23 previously separate standard-role cases now included alongside the prior 570 affected checks. Totals are not summed with historical gates. Cases cover complete creation/unchanged retry, every six-row insertion failure, changed intent, partial/missing destinations, uncertain commit, definition/persona drift, closing authority loss, competing intents, progressed/mutated graphs, dropped committed tasks, recorded preparer separation and incomplete intents. HTTP cases cover two actual company graphs/inboxes and changed skipped foreign ownership/task evidence. Existing typed parent renders and accounting/FIELD checks remain in the gate.

The first broad queue checkpoint passed 622 and failed two existing tests: the request fixture's simulated preliminary process owner retained the previous selected company until an accounting read. The fixture now reads the actual HTTP company header for that simulated process owner. The final source passes both tests and the new actual-native-reader journey. Earlier passing checkpoints and compile failures are superseded, not added to the result. Router composition syntax and the missing process-payload namespace were corrected before the passing runs.

Physical datasource transactions/persistence and current-review callbacks remain explicit fixtures. SQLite executes existing app RBAC/configuration code; native business persistence uses the actual Beep repositories with mocked provider records. Existing WORKFLOW_INBOX_FOCUSED shared identity/diagnostics/reference/wording seams remain explicit. Actual provider locking/concurrency/durability, cross-module atomicity, production binding/DI/host, OIDC/browser/RTL/accessibility and full-solution verification remain open.

## Next visible delivery

Implement the API-owned review/submission boundary: original app roles and Workflow.Start plus native source WRITE authority, exact owned AP/PO revision and recorded preparer, fixed module binding and current recorded definition. Validate the request against that review and preserve the frozen intent through unconfirmed outcomes. Consume it from existing accountant/procurement source screens and show the recorded handoff/read-back on landing queues, process detail and dashboard drill-downs.

Company process actions, acceptance/allocation policy, concurrent consumption, approval/post/payment, durable recovery after process progression, outbox, stored-definition/schema reconciliation and the remaining personas/multinational requirements remain open. No live database write, migration, grant or financial operation was performed. Beep MigrationManager remains the business schema owner; no SQL, new business EF lane or phase closure.
