# Implementation 197 - Controller evidence in assigned work and persona landing

Date: 2026-10-07. Status: the existing assigned-work API and shared persona inbox/dashboard panel now expose configured controller task evidence and status drill-downs. No persona phase is closed.

## Existing API and contract ownership

UnifiedTask in canonical TheTechIdea.Data now carries optional CompanyAccountingStepReview, reusing the configured review introduced in implementation 196. No new task, decision model, workflow persistence or dashboard store is introduced. Existing external processes without a company command intent retain their admitted task path; company command process definitions are checked for the accounting evidence configuration, and configured steps are reviewed through the existing current step owner. Unconfigured steps keep their existing read behavior.

The API requires the addressed company/account, exact process/source/current step and assigned role. The canonical inbox snapshot validator requires the review to match the task's AP invoice, scope, step identity, display name and role, and the response's company/account. Before publishing, the API rereads configured evidence and requires equality with the original review. Changed controller disposition, proof, source, role/grants or workflow step cannot produce a partial successful response or counts.

Completed native tasks disappear from pending work through the existing router/read/admission path. A recorded disposition with a still-pending task remains pending workflow work; it is not financial matching, posting or payment.

## Shared inbox and persona dashboard screen

PersonaAssignedWorkPanel is shared by the existing task inbox and persona landing/dashboard experience. It shows configured controller work counts within the selected source and role: disposition needed, ready for workflow completion, or recorded controller required. Each summary drills down to matching recorded task rows. All work statuses restores the full selected queue. Individual rows explain the missing disposition, recorded-controller requirement, current permission/independence refusal or confirmed disposition, and retain their exact process route.

Counts come from the loaded addressed response, not invented finance KPIs. Users reload and open the process for a fresh action review. PersonaTaskInboxState retains its existing copied snapshots and company/persona late-response fences; changing company clears the evidence filter and loaded work. Source/role/work-type/attention selection continues to operate on the same canonical rows. MudBlazor/theme conventions are preserved; no CSS/JavaScript was added.

## Focused verification

Actual Data builds with zero warnings/errors in 1.83 seconds; final overlapping source gate passes 519 checks in 8.4325 seconds. Final affected gate passes 1396 checks with zero failures/skips in 2.0576 minutes, including five new journey cases. The composition retains 366 existing warnings. All run handles reached terminal exit; whitespace verification passes. The two gates overlap and do not establish a full solution or live-provider pass. New cases use the actual API and typed inbox client, native company process/task completion, existing SQLite app RBAC and current standard configured role. They cover missing disposition, eligible recorded controller, another controller, completed task removal and changed decision during the closing review. The shared panel is statically rendered with actual API-backed scoped state, checking counts, process links, status drill-down and company clearing. Foreign-account review evidence cannot be accepted by canonical client validation.

Native datasource, configured persona mapping and production read authority remain explicit test seams. Static rendering is not browser verification. Live provider/schema migration, OIDC, accessibility, browser and cross-database transactional consistency remain unverified. No SQL or new business EF lane; existing app RBAC uses its current owner. No live migration, grant, workflow definition seed or deployment was performed.

## Remaining roadmap

Continue configured workflow definition provisioning and correction/cancellation/history handling so accountant correction work and controller completion form the full company journey. Durable lifecycle recovery, financial tax/non-PO/book/period/journal evidence, policy tasks, stock/returns, other personas and multinational requirements remain open. Broader dashboards need source-backed domain measures rather than task counts. Implementation 196's PRODUCTION-before/after-LIFECYCLE evidence checks do not establish cross-database atomicity.

Checkpoint evidence: the initial affected gate passed 1386 of 1396 checks. The new fixture installed the controller role before the starter, and generic workflows were unnecessarily sent through configured-only full step review. The starter/controller sequence and explicit configuration detection were corrected. The next run passed 1393 of 1396; all API/refusal/completion cases passed, while three static displays lacked NavigationManager in the fixture. The renderer now registers it. These checkpoints do not claim final verification.
