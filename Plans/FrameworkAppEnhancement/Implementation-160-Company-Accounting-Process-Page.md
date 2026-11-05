# Implementation 160: Company accounting process page

## Accountant and buyer process detail

/accounting/processes/{Id} now consumes the existing typed company accounting process API and displays the recorded company workflow without a FIELD selector. The page checks standard current company roles and Accounting.View/Procurement.View, decodes its original escaped location exactly once and rejects invalid locations or late company changes. It links back to assigned work and uses the recorded source type/identity to open the existing AP invoice or purchase-order workbench.

The common RecordedProcessSourcePanel displays company, process status, source identity, initiator/time, current state/step, recorded assignments/roles, steps and history. The existing FIELD process page uses this same panel; its FIELD source/action handling remains in its existing owner. Shared display avoids a parallel process model/table renderer. Recorded history now appears with source/step evidence before available actions on the FIELD page.

The company panel identifies company accounting scope and provides exact encoded invoice/order links, including slash/international/literal percent identities. It explicitly states that recorded workflow metadata does not establish matching, accepted receipts or payment. No submit/approve/post/pay action is introduced by the read page. All components use MudBlazor; no CSS/JS or inline style/script is added.

## Existing state owner

ProcessInstanceDetailState adds LoadAccountingAsync using the same company selection subscription, generation guard and Clear owner. It requires an exact company-bound process payload, no FIELD ID, supported owned accounting source, confirmed process status and exact step/history parent identities. Duplicate step-instance/history identities or foreign records refuse. It freezes the full process/step/history content before publishing an owned source link.

Company changes, invalidation, failed reloads and late responses clear/discard old source and history. Company mode does not invoke the FIELD process/history readers. It refuses FIELD action-review requests before transport and CanSubmit is false for company accounting mode. Company decision/submission needs its own reviewed command contract within the existing accounting/process owners; it is not a fallback to the FIELD transition API.

The company parent checks current roles/read permissions before transport and the exact source permission after loading. The existing API independently validates current authority, company/process/source ownership and binding/source revisions. Parent/client/full authentication remain source-only in this verification scope.

## Focused evidence

The actual affected controller/native/state/Razor gate passes **469 tests**, zero failed/skipped, terminal exit 0, in 5.9312 seconds. Log: `%TEMP%/oilgas-company-process-ui-160.log`. It retains 63 complete-controller HTTP cases and the existing 261 dependency/source-precedence warnings. Eighteen new cases extend the prior 451 gate: fifteen company state tests and three actual shared-panel render cases.

The separate extended process-detail state gate passes **118 tests**, zero failed/skipped, terminal exit 0, in 0.9697 seconds, with zero reported compiler warnings/errors under its existing source-precedence suppression. Log: `%TEMP%/oilgas-process-state-160.log`. Its scope includes the same fifteen company state cases plus prior FIELD/action/definition/catalog checks; totals across gates are not summed. Whitespace verification passes.

New state cases cover both source types, frozen roles/history/identity links, nine ownership/FIELD/status/step/history failures, late company/invalidation changes, failed reload clearing and blocked FIELD action transport/submission. Actual Razor renders cover both company source types, encoded links, recorded assignment/history and clearing, plus preservation of FIELD scope without a company source link. The static render fixture needs no popover because this shared display has no popover control; duplicate fixture providers were removed after the first render attempt exposed a test-host subscription conflict.

Actual state/shared panel compile and run; complete parent pages, typed process client, production context/DI/role bridge/full host, inbox routing, browser/RTL/keyboard/accessibility and real native provider remain unverified. Data sources and persistence schema are unchanged. No SQL, EF path, live process/task, migration, grant or financial write is introduced.

## Next assigned-work handoff

Route explicitly company-owned accounting tasks in the existing inbox to this page, while retaining FIELD tasks' existing resource admission. Reuse the module/process/native source read boundary and validate original company/process/step/task identities before showing assignments. Add durable company process creation/submission, actual invoice-preparer separation and reviewer assignment, accepted receipt/policy/concurrent consumption, definition/schema reconciliation and recorded/not-recorded/uncertain recovery. Independent match/approval/post/payment and other personas/multinational requirements remain open; no phase closes.
