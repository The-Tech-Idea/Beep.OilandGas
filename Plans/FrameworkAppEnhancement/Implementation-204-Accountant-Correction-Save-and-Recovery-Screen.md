# Implementation 204 — Accountant correction save and recovery screen

## Complete accountant screen action

The existing payables correction editor now saves its exact reviewed source correction through the existing typed client/API/native owner. The existing APInvoiceWorkspaceState owns the frozen submission, busy state, retained uncertain intent and copied confirmed result. No second source model, store, database path or workflow is added.

Screen actions require current standard roles and the authenticated NameIdentifier/sub matching the selected accountant account. Current read/purchase/correction grants remain required, and the API independently checks its own current authoritative role/grant/binding sets. Intent details are hidden outside the original company/account; an authentication-only account change cannot reveal or resend the prior account's intent even while selection is stale.

After fresh review the screen offers Save reviewed source correction. On unknown or unconfirmed outcome it shows the retained intent and original reason, Reconcile original correction intent and the escaped original invoice recovery route. A confirmed result shows the recorded version/total and explicitly keeps independent financial rereview and correction task completion pending. It does not claim matching/posting/payment approval.

## Frozen scope and exact confirmation

The scoped owner creates one original GUID intent from the exact current review and history predecessor. Record callbacks receive an independent copied submission; caller mutation cannot replace the retained or verification graph. Reconciliation reuses the original submission, never a new review, values, reason or intent.

Missing/changed source read-back, API failure, uncertain commit or late responses retain the original unconfirmed intent. Confirmation requires the canonical addressed result and a fresh complete source whose header and line set exactly match the returned recorded history. Before applying confirmed source, stale linked purchase/receipt allocation, policy and financial review state is cleared.

Unsaved edits and transient reviewed results clear on source/financial reload, company/account changes and disposal. Authentication changes additionally clear the entire displayed invoice and line set. The separate retained command survives those clears while unconfirmed. Version, correction-display generation and exact company snapshot fences reject late publication. Busy state settles without converting an unknown outcome into success. Recovery requires the original company/account/grants and loaded original invoice route. Whole circuit/browser lifetime recovery is still open; this scoped retention is not a durable client recovery store.

## Conflicting source work

Pending/busy correction blocks new correction editing and variance/controller, allocation and policy command paths in their existing owners. Payables action controls and the shared invoice source controls suppress conflicting work. Generic company accounting handoff is hidden while correction remains unsettled. The retained correction is reconciled through its protected command endpoint before another source command or workflow start can proceed.

## Focused verification

The affected gate passes **1553 checks with zero failures/skips in 3.0464 minutes**, including fourteen new scoped/full-screen cases: nine state cases and five complete payables page cases. After authentication changes were tightened to clear the complete displayed source, **all 28 correction editor/workspace checks pass in 36.5813 seconds**. The gates overlap and their scopes differ: the later targeted run verifies the final authentication-handler change. The initial checkpoint passed 1551 checks in 3.1911 minutes. All command handles reached terminal exit and whitespace verification passes.

Scoped cases cover copied results/inputs, exact original unknown-intent recovery, missing/changed source refusal, late financial/company/account/disposal response rejection, privacy and command conflicts. Complete static rendering invokes actual screen review/save/recovery handlers against Kestrel, typed clients and native AP owners. Cases cover confirmed save, uncertain commit/reconciliation without extra writes, revoked grants, changed contributors and authenticated account change without selection change. These checks retain the original DRAFT status and distinguish source confirmation from financial approval/task completion.

Actual canonical Data builds with zero warnings/errors in 1.87 seconds. The overlapping source gate passes 519 checks in 9.4839 seconds. Composition has 374 existing source/cache/package warnings. An initial test compile referred to TotalAmount instead of the existing projected Amount property; it was corrected before runtime verification.

Native provider/physical transactions and authoritative access capture remain fixture seams. Browser/OIDC/accessibility, live schema/grants/provider, durable circuit recovery and broad solution acceptance are unverified. No SQL, new business EF lane, live migration/grant or phase closure is introduced.

## Next implementation target

Implement independent rereview of the latest recorded correction and confirmed correction evidence for configured workflow work. Keep current source/contributors/policy/binding and role independence explicit; source correction alone cannot authorize financial matching, posting/payment or correction task completion. Live definition provisioning, lifetime recovery, tax/non-PO/book/period/journal evidence, policy tasks, stock/returns, other personas/dashboards and multinational acceptance remain active.
