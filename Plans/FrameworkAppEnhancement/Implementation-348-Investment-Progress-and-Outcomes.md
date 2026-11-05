# Implementation 348: Investment progress and outcomes

2026-10-26. Locally verified under CA-22 / FG-52. Continues [347](Implementation-347-Investment-Stage-Decisions.md). Full goal remains active.

Deliver the sender/read-only source journey after an investment handoff: exact current stage and role/workspace, actual pending tasks, ordered recorded decisions with reviewer/rationale/UTC audit and final financial/native reconciliation. Replace the existing handoff-exists/task-destination summary in the active AFE status contract and screen. Do not add a competing status API, persistence model or workflow engine.

Use the existing DevelopmentInvestmentHandoffStatus, PPDMProcessService validated complete graph, CompanyDevelopmentInvestmentRoutingService.ReadHandoffStatusAsync, AFE API/client, DevelopmentInvestmentHandoffState and its MudBlazor panel. New shared projection contracts belong to TheTechIdea.Data. Current three-view company/standard-role/FIELD READ authority remains required; start/review/write authority and an active recipient persona must not be required for historical status. Ownership precedes private projection; current original source/bindings and complete native/financial evidence must close before response. No business writes, repair, financial commitment, notification dispatch, SQL, business EF or schema change.

The screen must distinguish review in progress, a reserved original decision, financial-only progress, final approval/rejection and evidence requiring reconciliation. An intermediate approval is not final budget approval; historical task rows are not pending counts. Recorded stages preserve original configured names/roles/workspaces and actual approval/history IDs, reviewer, rationale and UTC. Loading, failed/empty reads and context/route/disposal changes must not retain stale evidence or fabricate zero counts. MudBlazor and existing theme/CSS/JS conventions apply; the approved installed auth/UI guidance substitutes for missing project skills.

Focused checks follow each coherent change: ordered native projection, unchanged native rows, configured multiple stages/personas, reservation/financial-only/final/rejection, competing financial evidence, malformed/missing/changed native proof, read-only and revoked/foreign authority, typed protected HTTP/client, source-state/component rendering and late context responses. Log/failure handling uses the current refusal/IFailureReporter/OilGasCallFailures boundary; native process history/approvals remain the audit owner. Shared NotificationService/UI infrastructure exists, but event delivery is a separate open feature; this read path sends nothing.

Record final current-source gates and hashes before promoting the six current tracker links. Native provider/migration/isolation/concurrency, full-host/OIDC/browser/keyboard/RTL and multinational acceptance remain open, alongside notifications, configured tiers, engineering design/program/progress/handover and wider delivery. Commit/push follows full goal and plan completion.


## Implemented source journey

The existing DevelopmentInvestmentHandoffStatus replaces its historical TaskDestinations integer with one complete progress projection. The native owner projects only the validated original process, stages, approvals, history and task graph. Configured names/roles/workspaces come from the frozen workflow; actual decisions retain reviewer, rationale, native UTC and canonical approval/history IDs. The API closes native status and current financial source around the read, reusing its existing operation-scoped financial-source anchor while preserving fresh authority/source checks.

Intermediate approval shows the next stage pending and later stages not reached. The pending count uses current PENDING task rows, including zero only for verified terminal closure. A reservation is not completion. An exact canonical financial record with a pending native reservation is financial-only progress; an unrelated valid financial record requires reconciliation. Completed native proof without matching current financial evidence refuses. A financial record with no native handoff is explicitly unconfirmed workflow completion. Live definition removal does not replace the original audit; caller mutation cannot change later reads.

The existing MudBlazor source panel presents this timeline and the actual AFE parent opens it through the same source-bound API/client. No competing page, route, status store or workflow engine is added. Error/loading/context behavior stays with the existing HandoffState and failure/refusal services; read failures clear prior successful evidence. Original command histories and approvals remain the canonical durable audit. Notification routing/delivery and broader successful-access audit coverage are separate open capabilities; this read journey dispatches no notification or business write.

## Focused verification

| Gate | Passed | Seconds | Terminal exit |
| --- | ---: | ---: | ---: |
| native-source-parent-component-and-affected-workflow | 112 | 17.5917 | 0 |
| protected-readonly-progress-outcome-and-isolation | 2 | 318.0420 | 0 |

The credited gates contain **114 successful executions covering 114 distinct cases**, zero failures/skips. The 112-case gate covers native timeline/source reconciliation, current stage versus historical tasks, malformed projections/native evidence, definition removal, defensive copies, all rendered status states, encoded rationale, actual AFE parent navigation, stale company/persona/route/disposal responses and affected native/receiving/financial regressions. The protected two-case gate uses real app RBAC/registry SQLite, typed HTTP and canonical source owners for a read-only sender's initial -> intermediate approval -> final rejection reload, anonymous refusal, revoked FIELD access and a foreign company with colliding FIELD identity. Status reads leave native/financial write counts unchanged.

Data builds with zero errors in 6.24 seconds; the two warnings are existing missing IdentityServer analyzer project references. Generated harnesses use the actual Data DLL with controlled Beep/auth/resource/JS/diagnostics seams. The final API gate compiles before the added parent-page test; the final 112-case gate compiles and exercises that test with the same production sources. This is local focused evidence, not full-solution, full-host/OIDC, browser, keyboard/RTL, live provider/concurrency/migration or multinational acceptance.

[Observed evidence, logs and hashes](evidence/348-investment-progress-outcomes/observed-evidence.json) records current gates and construction failures. The first native fixture referenced the wrong history property and was corrected to DETAILS. The invalid-projection test initially returned a captured old valid mock result; its transport now returns the changed response and the existing validation refuses it. Protected HTTP correctly refused stale company grants: the fixture held a routing service constructed before reselection. The test now resolves the service per request using the current real authority context; no authorization guard or response status was weakened.

## Remaining delivery

Notifications, configured investment tiers, engineering design/program authoring, persisted drilling/construction progress, financial commitments/actuals and independent production handover remain open, alongside the rest of the company/persona plan. Successful-access audit coverage and live provider/host/browser/accessibility/RTL/multinational qualification remain unproved. No financial commitment/expenditure, notification delivery or cross-provider atomic snapshot is claimed. Run affected focused tests after every coherent change. No staging/commit/push until full goal and plan completion.

[349](Implementation-349-Approved-Investment-Drilling-Program.md) extends this recorded approved investment outcome into the actual company/well drilling-program DRAFT API, retained authoring screen, saved-program reload and read-only recovery. Its focused evidence and remaining gates are recorded in that owner.
