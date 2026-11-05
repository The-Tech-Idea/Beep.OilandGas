# 333 - Recorded maintenance workflow closure

Status: implemented with focused verification; the full goal and live/release gates remain open.

Continue the approved plan after 332. Close the actual final return assignment only after its original recorded physical return acceptance still matches current work/inspection/person evidence. Reuse the native process, history, task router and serializable Beep owner. This is workflow closure; it does not claim an asset operating-state transition or financial close.

## Delivery

1. Add a viewer-independent physical return subject revision to existing return evidence/public facts, without rewriting original approvals. It binds original work, verification/handoff and exact return observations/effective inspector identities/native people.
2. Review the actual terminal configured return step, current original report/order/components/inspection/person evidence and original recorded acceptance. Enforce current app-owned configured role, ReturnToService plus Workflow.Execute, FIELD/order WRITE, stored selection and native bindings. Do not infer an asset state from ACTIVE_IND or a missing WELL_STATUS row.
3. Retain original closure intent, explicit reason and confirmation before transport. One owned native command closes the complete return task set, completes its final step, appends immutable closure history and marks the original process COMPLETED with actor/UTC. Refuse definitions with remaining steps or incomplete earlier duties.
4. Store and verify only the exact declared graph delta; support original closure recovery without replay and all earlier acceptance/handoff/verification/executor receipts through a historical projection. Public outcome is separate from private rationale/body/intent. Current source changes, replacement intents and graph corruption refuse.
5. Expose protected API, typed client/state and actual completion/detail/assigned-role entry with explicit source/audit, original URL retention and fresh-circuit recovery. Public status does not require workflow-execution capability. Clear/fence context changes.
6. Focused native, client/form/parent and protected full-journey checks. No SQL, business EF, new store/table or implicit schema apply.

## Asset advancement continuation

WellLifecycleService currently substitutes PLANNED when state is missing and FacilityManagementService projects ACTIVE_IND as ACTIVE/INACTIVE. Neither establishes a controlled return-to-production state. Extend the existing asset owners with original field/company/asset/status/transition/rights evidence before coupling them to this closure. Preserve exact native well/equipment/facility identity qualifiers and reviewed source connections. No automatic asset state, mixed-source atomicity or production readiness is claimed by closure.

## Implemented source

- Shared contracts: `TheTechIdea.Data/OilGas/CompanyMaintenanceClosure.cs`; the existing return review now computes a viewer-independent physical subject revision. Original acceptance commands/approvals are retained unchanged.
- Native owner: `PPDMProcessService.MaintenanceClosure.cs` extends the existing lifecycle owner. It verifies the complete terminal assignment, original acceptance, current work/inspections/identities/people, earlier completed steps and exact reviewed graph. It closes the original task set before completing the final step/process and appending closure history. For two original tasks this is five Beep writes. No new table, migration, SQL or business EF is introduced.
- Historical projection: validate exact process/step/task/history business and audit fields and the complete declared delta before restoring the earlier graph in memory. Acceptance, return handoff, verifier and executor receipts remain recoverable. A closed workflow no longer appears as an active assigned duty.
- API: existing protected maintenance controller adds POST `execution-permit/closure/review`, `/closure`, `/closure/recovery/reference`, `/closure/status`. Standard configured roles and ReturnToService remain required; commands/private recovery also require Workflow.Execute. Public status needs no execution capability or private intent. The existing completion/return source readers and owned CORE bridge are reused; separate sources retain paired comparisons without claiming distributed atomicity.
- Persona queue integration: the existing WorkflowTasksController selects PENDING tasks assigned to current authoritative app roles and standard role claims. Closure updates the existing task set to COMPLETED, so reloading uses the existing queue filtering; no separate counter or inferred asset status is added. Recorded outcome remains reachable after the active assignment closes.
- Web: existing workflow typed client, scoped `MaintenanceClosureState`, MudBlazor `MaintenanceClosurePanel`, actual work-order detail and assigned-duty parent. Review displays the original accepted return, people, audit IDs, current conditions and comparison UTC; reason/confirmation are explicit. Original source and UUID are retained in URL before transport. Lost acknowledgements require original recovery. Public absence differs from failed reads. Context/FIELD/auth changes fence responses and clear displayed evidence. Closure remains reachable from an original assigned task after no active duty remains.

## Focused evidence

- Data contracts built with zero warnings/errors (3.85 seconds).
- Native closure: 16 passed, zero failed/skipped, 5.4998 minutes, terminal session 71583. Includes actual common audit handler, exact writes/task population, unchanged report/approvals, all prior receipts, refusal/revocation, complete graph corruption, private ownership, replacement and uncertain-commit recovery.
- Initial typed client/state/actual form/public status: 43 passed, zero failed/skipped, 25.3839 seconds, terminal session 57852.
- Expanded UI regression: 189 passed, zero failed/skipped, 51.0408 seconds, terminal session 28217.
- Actual native work-order detail parent: 20 passed, zero failed/skipped, 5.4561 seconds, terminal session 54095.
- Earlier native handoff/decision/review regression: 104 passed, zero failed/skipped, 2.5168 minutes, terminal session 44439.
- Assigned-parent closed-outcome follow-up: 44 passed, zero failed/skipped, 19.8823 seconds, terminal session 86689. After the final public report-revision consistency addition, Data built with zero warnings/errors (3.80 seconds), native positive/actual-audit/prior-receipt checks passed 2 (1.4767 minutes, terminal session 83971), and closure client/state/form/status/assigned-parent checks passed 44 (19.0086 seconds, terminal session 36823). Counts overlap.
- Initial protected actual upstream-to-return/closure HTTP journey: terminal session 63561 failed 1 after 11.9357 minutes. Actual closure, exact writes, retries, original acceptance recovery and replacement refusal passed before the final assertion. Removing Execute correctly invalidated the fixture's captured company grants; attempting public status before a fresh company selection returned 403. The fixture now explicitly resolves and selects a fresh app-RBAC snapshot before proving public status without Execute. Production authority rules were not weakened.
- Final protected HTTP rerun: 1 passed, zero failed/skipped, 8.7089 minutes, terminal exit 0 and session 1868 closed (`oilgas-333-http2.log`). Actual upstream receiving/execution/submission/verification/handoff/physical acceptance/terminal closure, original and prior receipt recovery, exact five closure writes, replacement refusal, fresh app-RBAC selection after Execute revocation, public status without Execute and consistent replacement-report refusal are verified. All checkpoint jobs are terminal.
- Final actual loading indicator and combined return/execution action visibility: all 44 closure client/state/form/status/assigned-parent checks passed again (18.4741 seconds, terminal session 44317).

## Remaining boundaries

Focused native providers do not establish real-provider rollback, locking/concurrency or distributed consistency. Browser, real OIDC, responsive/keyboard/screen-reader, live provider and full deployment gates remain open. Asset operating-state advancement and financial close are separate recorded transitions. The full goal also includes rejection/rework, finance, all personas, multinational and release requirements. Commit/push follows the complete requirement audit; this checkpoint does not complete that goal.
