# Implementation 174 — Role-filtered landing and controller handoff

Date: 2026-10-06. Status: shared persona landing/inbox role drill-down and actual AP/PO reviewer-to-controller queue handoff pass focused verification. Complete financial execution and browser/provider acceptance remain open.

## Shared company landing and role decisions

The existing PersonaTaskInboxState and PersonaAssignedWorkPanel now support assigned-role drill-down alongside source, work type and attention. Role options come from the verified loaded tasks, rather than a hardcoded role list or a persona preference. Role counts use the selected source; pending/type/attention summaries and visible contributing records use the selected role and source together. Choosing a role resets work-type/attention filters. Reload, company/persona changes and failures clear selected role and previous values.

The accounting dashboard, procurement landing and task inbox already consume this same component. No alternate queue/store is added. A current company role does not guarantee it has pending assignments, and the financial status source queue remains distinct from assigned workflow-task counts.

AccountingDashboard now injects the existing narrow IFieldSelectionContext for its FIELD preference lookup/event rather than the broad DataManagement service interface. The production DI registration already resolves this contract to the existing typed client owner; no new transport, persistence or selector owner is introduced.

## Recorded task detail

The Data-owned UnifiedTask gains additive optional StepDisplayName and CreatedByUserId fields. Existing StepName remains the task's recorded step-instance identity used by admission and routes. The API takes the display name from the admitted owned process step and the creator account from the recorded task audit. Absent metadata stays explicitly unavailable; account IDs are not presented as person names or inferred originating personas.

WorkflowInboxReview validates bounded, nonblank, control-free metadata when present. The API compares the admitted step display name again during the closing owned-process read and already compares the complete task row, including creator audit. Changed names or creator records, invalid metadata, revoked role/company or changed process/source evidence cannot publish partial landing counts. UI text remains normally Razor-escaped.

The shared panel shows recorded step name, separate step identity, assigned role and creator account, with the existing source-process route, task creation and due evidence. It grants no action or financial authority.

## Actual handoff journey

For both AP_INVOICE and PURCHASE_ORDER, tests start from the actual native reviewed workflow and complete its reviewer step through the actual company HTTP endpoint. The recorded old task set closes and the next native Controller step routes to its configured CONTROLLER/FINANCE personas.

An explicit separate SQLite app-RBAC fixture grants Controller and the CONTROLLER workspace. The actual persona API/client/state, actual task router, current owned process/source reader and complete accounting landing/inbox/process pages confirm exactly one entitled CONTROLLER assignment. Its task identity belongs to the recorded receipt's next task set, its step identity matches the receipt, and its source/process link opens the recorded decision and original invoice/order. The original reviewer persona queue is empty after closure. Selecting a second company clears prior role/results and returns a verified queue without the first company's process.

Role fixtures retain the test account actor across these separate contexts; this is a configured role/persona handoff test, not a claim that two production users signed in. No live assignment/grant occurs. FIELD selection uses an explicit no-selection preference fixture, so the company queue is exercised while FIELD financial catalog availability remains explicit. Source-client callbacks call the real AP/PO HTTP endpoints; the production FIELD transport, OIDC and complete DI host are not certified.

## Focused verification

Actual Data source build: zero warnings/errors, 1.81 seconds. Log: %TEMP%/oilgas-role-handoff-174-data.log.

Final actual-source affected gate: Scripts/Test-PurchaseOrderOwnership.ps1 with actual Data and -IncludeWorkflowInbox. **755 passed, zero failed/skipped, terminal exit 0, 16.1375 seconds**. Log: %TEMP%/oilgas-role-handoff-174-final.log. Twelve new cases cover AP/PO actual HTTP completion to entitled controller landing/inbox/process and original queue closure; six malformed optional metadata payload refusals; changed/invalid step name or creator evidence without partial counts.

Separate shared state/render gate: Scripts/Test-PersonaTaskInboxState.ps1 with actual Data. **71 passed, zero failed/skipped, terminal exit 0, 1.2893 seconds**. Log: %TEMP%/oilgas-role-handoff-174-state.log. Two added cases combine role/source/type counts and exact contributing routes, reject unknown roles, reset filters and clear failed reloads. These counts overlap previous gates and are not added.

Whitespace checks pass. Expanded affected composition retains 319 existing dependency/source-precedence/nullable warnings, with no new changed-source warning reported. The initial 745-case handoff gate passed. Adding complete AccountingDashboard rendering exposed a missing authorization service in the static fixture (743 passed, two failed); registering standard AddAuthorizationCore fixed the fixture, and the expanded 751-case gate passed before the final 755-case validation.

Checks compile actual Data, API/native process/history/approval/task owners, complete clients/states, accounting landing, FIELD selector, shared assigned-work component, inbox and source process parent. Static rendering is not browser interaction, keyboard/RTL/accessibility or production host/provider verification.

## Next delivery and remaining scope

Connect the Controller's reviewed Workflow.Execute decision on its recorded next step through terminal process read-back and refreshed landing/inbox. Keep financial matching/approval/post/payment as distinct source commands; completing a workflow step never proves those financial outcomes.

Durable browser/new-circuit/historical recovery/outbox, configured policy/conditional routing/delegation, receipt acceptance/concurrent consumption, full accounting KPIs/period/currency dashboards, other personas and multinational requirements remain active. Beep MigrationManager remains the business schema owner. No handwritten SQL, new business EF lane, schema change, live migration/grant, deployment or feature/phase closure.

