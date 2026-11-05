# Implementation 127: Inspection progress and supervisor review

## Visible maintenance workflow

The existing work-order inspection panel now presents category counts from its confirmed original FIELD/order evidence: requirements, unrecorded results, passes, failures and not-applicable results. It preserves the individual requirement owner, jurisdiction, inspector, timestamp and rationale below the summary. Read-only users can inspect progress without acquiring edit authority. Accessible column and row headers identify the table. Empty evidence does not invent categories or required coverage.

The existing WorkOrderInspectionState computes progress from its reviewed rows and clears it on company/source changes and before reload. Failed reloads cannot retain a previous successful summary. No new dashboard store, business contract, endpoint, authorization rule or persistence path is introduced.

The screen explicitly separates recorded inspection progress from required category coverage, permit review, independent supervisor acceptance and return-to-service authorization. Not-applicable results do not count as passes. The canonical WORK_ORDER header has no workflow-status field; COMPLETE_DATE is not evidence of these approvals. The unsupported legacy PROJECT transition path remains an open implementation issue; this change does not enable it or infer completion from inspection results.

## Focused verification

Scripts/Test-WorkOrderListState.ps1: **325 passed**, zero failed/skipped, terminal exit 0, 1.2989 seconds. Log: `%TEMP%/oilgas-inspection-progress-127.log`. Three additional cases verify mixed-category/status counts with read-only visibility and company invalidation, empty evidence without guessed coverage, and failed reload clearing earlier progress.

Scripts/Test-WorkOrderInspectionRendering.ps1: **eight passed**, zero failed/skipped, terminal exit 0, 1.7983 seconds. Log: `%TEMP%/oilgas-inspection-progress-render-127.log`. The new actual-component case verifies read-only category progress, retained not-applicable rationale, accessible headers, explicit acceptance/completion boundaries and absent editors. Existing seven render cases remain green. Tests use current production state/component sources and controlled typed-client fixtures, not live HTTP or browser events.

No Data/schema changes; the existing actual compiled Data assembly is reused. Whitespace check passes. Native provider, full host/DI, HTTP/OIDC and browser acceptance are not established by these focused gates.

## Remaining work

Approved maintenance duty roles and standard guards, authoritative process-linked work-order state, independent supervisor acceptance, permit/return-to-service handoff, correction history, durable command outcomes and native Beep migration/provider transactions remain open. Continue the other company persona workflows and multinational requirements under their existing plans and tracker owners. No handwritten SQL, new business EF lane, live grants/writes, migration execution, deployment or persona/phase closure.
