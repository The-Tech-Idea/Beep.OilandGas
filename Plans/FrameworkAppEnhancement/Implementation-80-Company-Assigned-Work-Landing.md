# Implementation 80: company assigned-work landing

Updated 2026-10-06. Supports [accountant journey](Persona-01-Accountant-and-Controller.md) and shared [persona workflows](Company-Persona-Data-Workflows.md). Partial: actual shared inbox view state has focused evidence; API/transport/host/Razor changes are source-only. No complete finance or persona journey is certified.

## Existing gap and user-visible behavior

The accounting landing contained process catalog links without a daily assigned-work queue. The existing task page loaded persona tasks through an independent HttpClient, did not establish selected-company view state, reset counts to zero on failures and could display a success-like empty state. Its persona code was not populated. Separate critical/normal lists risked inconsistent filtering and counts.

The existing API queried all repository memberships rather than approved current-company roles, and filtered task rows without confirming process/resource entitlement. These gaps must be addressed before displaying accountant/company work.

## Shared owner and screen changes

- [UnifiedTaskInboxService](../../Beep.OilandGas.Web/Services/UnifiedTaskInboxService.cs) now consumes the existing ApiClient transport, preserving canonical company headers, cancellation, API failures and stale-response checks. Its existing interface moves to its own file; no parallel inbox API/store.
- [PersonaTaskInboxState](../../Beep.OilandGas.Web/Services/PersonaTaskInboxState.cs) uses existing company/persona owners. It clears work on company/persona change, refuses missing account/company/eligible persona, discards late results and validates response company/actor/persona, pending rows, unique identities, priority buckets, counts and exact encoded process routes. Failed loads leave unavailable state rather than fabricated zero counts. Subscription startup is idempotent and disposal unsubscribes.
- [PersonaTaskInbox component](../../Beep.OilandGas.Web/Components/Workflow/PersonaTaskInbox.razor) shows loading/error/retry/no-company/no-workspace/empty states, actual pending/approval/review/overdue counts, local work-type filtering, entity/process/step identity, recorded due date and source-process link. Both the [existing task inbox](../../Beep.OilandGas.Web/Pages/PPDM39/Workflow/TaskInbox.razor) and [accounting landing](../../Beep.OilandGas.Web/Pages/PPDM39/Accounting/AccountingDashboard.razor) consume this single component. Accounting catalog links remain below the queue, with cost/revenue workbench entry points. No task mutations are added.
- Data-owned UnifiedInbox carries response company, actor and persona. Existing API endpoints and router remain the owners.
- [WorkflowTasksController](../../Beep.OilandGas.ApiService/Controllers/WorkflowTasksController.cs) uses the selected request company and canonical workspace review, matches the current preferred eligible persona, filters by that company's approved roles and recorded pending tasks, verifies process/entity/step linkage and current READ resource access, then rechecks workspace/profile and resource access before publication. Source-process routes are constructed from exact recorded process IDs. Counts and filters use the same visible list. This is source-connected only; real HTTP/provider closure and workflow state consistency remain open.

## Focused evidence

Actual shared Data sources compiled in isolated output with zero warnings/errors. [12 state cases](../../Beep.OilandGas.Web.Auth.Tests/PersonaTaskInboxStateTests.cs) passed, zero failed/skipped, terminal exit 0, 1.0431 seconds, through [Test-PersonaTaskInboxState.ps1](../../Scripts/Test-PersonaTaskInboxState.ps1), against the actual current compiled Data assembly. Log `%TEMP%/oilgas-task-inbox-state.log`.

Cases cover critical rows and encoded opaque source IDs, wrong company/actor/persona, invalid counts/external routes/duplicate rows, company clear and late away/back response, missing company/workspace no request, failed refresh unavailable state, persona invalidation and disposal. The fixture is an explicit typed inbox/persona authority; this does not execute HTTP, actual role transformation, Razor rendering or task routing/persistence.

Existing native role/persona routing test source was updated for the new controller dependencies and current-company/resource boundary; it is not executed evidence. Current host/UI diagnostics and identity project gaps remain; no production dependency shim or missing project clone was introduced. User-approved installed UI/auth/form guidance remains in force.

## Required next delivery

1. Execute actual controller/API/client integration, including two companies with colliding process IDs, wrong/pending/revoked roles and mappings, restricted resources, changing linkage/authority/binding, missing and inconsistent tasks, actual process state and filter/count agreement. Confirm independent direct source-route/action refusal.
2. Compile/render the shared component and accounting/task pages; exercise company/workspace switch, sign-out, busy/error/retry, empty filters, keyboard/RTL and notification-driven refresh without trusting notification counts as data authority.
3. Deliver actual finance source detail and independent approval/post through existing process/accounting owners, recorded reload and identical-filter KPI drill-down. This queue is shared selected-workspace work, not a verified AP aging, journal or finance exception dashboard. Region/book/period/currency configuration remains explicit and unverified.

No schema, SQL, new business EF persistence, synthetic financial metrics or role grants. Beep MigrationManager remains the business schema owner. No phase or complete goal closes.
