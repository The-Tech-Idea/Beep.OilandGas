# Implementation 200 — Accountant correction editor and review

## Delivered persona workflow

The existing payables invoice workbench now provides explicit correction editing for the original recorded proposal preparer after a verified controller REQUEST_CORRECTION. The editor shows the recorded request and accepts an invoice total, every existing line's quantity, unit price, line total and description, and a correction reason. It uses MudBlazor and the existing scoped APInvoiceWorkspaceState; no second invoice model, store or database access path is introduced.

Current standard ASP.NET roles and the selected company's Accounting.View, Procurement.View and Accounting.CorrectInvoice grants gate the screen and its actions. The existing typed accounting client and independent API authority/evidence checks perform review. Another preparer cannot access the editor. Complete current financial evidence is required before preparing edits.

## Explicit inputs and scoped publication

Amounts remain nullable UI inputs. Clearing an amount does not substitute zero or reuse a previous value. Only recorded line identities are editable; canonical correction draft validation requires complete exact arithmetic and declared totals. Currency, units and purchase references stay recorded.

Edits invalidate any previously reviewed result. Source/financial reload, company/account changes and workspace disposal clear unsaved edits and reviewed evidence. Request and edit generations prevent a late API response from publishing after those changes. Returned review snapshots are copied. A failed review clears its previous result, settles the busy state and displays the existing application failure wording; API conflicts preserve the API's actionable refusal.

The screen separates recorded invoice values from proposed values. Review confirms the current source and controller request for those edits. It saves no correction, changes no invoice amount/status, approves no financial matching and completes no correction task. The existing native/API refusal to complete correction work without recorded corrected-invoice evidence remains active.

## Focused verification

Final affected gate passes **1442 checks with zero failures/skips in 2.2799 minutes**, including the fourteen new cases. The overlapping source gate passes **519 checks in 8.7193 seconds**. Actual canonical Data builds with zero warnings/errors in 1.31 seconds. Focused composition reports 367 existing source/cache/package warnings. Both gate handles reached terminal exit; whitespace verification passes. The first expanded run passed 1441 of 1442; its only failure expected local fallback text instead of the API conflict message. The assertion now checks the actual actionable API refusal.

Fourteen new cases cover scoped state and rendering of the complete payables page against the actual API/typed client. They include valid copied review, missing values, failed review, late edit/financial/company/account/disposal responses, revoked grants, changed global allocation evidence, edits after review and another preparer. Native writes remain the two preceding proposal/controller writes; the invoice remains DRAFT with its recorded total unchanged.

These are focused state, static rendering, HTTP, canonical data and existing native-owner tests. They do not establish live browser/OIDC/accessibility, deployed Beep provider/schema, production transaction or full-solution acceptance. No SQL, new business EF lane, live migration/grant or phase closure is introduced.

## Next implementation target

Implement the native versioned correction command and bounded history using the existing Beep-owned source path. Preserve the original proposal/controller request; require exact current source/contributor evidence and authority, durable exact read-back and retained original uncertain intent. Independent rereview and confirmed corrected-invoice evidence must precede correction task completion. Live definition provisioning, lifetime recovery, tax/non-PO/book/period/journal evidence, policy tasks, stock/returns, other personas/dashboards and multinational acceptance remain active.
