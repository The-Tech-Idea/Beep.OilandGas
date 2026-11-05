# Implementation 206 — Independent correction reviewer screen

## Existing payables workflow

The payables workspace now includes an independent corrected invoice review panel beside the original accountant correction editor. It uses the existing typed accounting client and current review endpoint, with the existing APInvoiceWorkspaceState owning the copied result, busy state and response generation. The canonical correction history is verified when the existing source is projected; no parallel source model or database path is introduced.

The panel requires current standard roles and the authenticated NameIdentifier/sub matching the selected company account. The scoped owner requires invoice/purchase read and independent variance review grants and excludes the source maker, proposal preparer and latest correction preparer. Conflicting unsettled source commands block review. The API independently enforces current authority and complete source/history/contributor evidence.

## Screen and invalidation behavior

The reviewer sees correction version/intent, preparer and reviewer, original/current billed totals with recorded currency, correction reason, current financial and review revisions, UTC capture and the current check/result/evidence table. Unavailable diagnostics remain visible. The panel explicitly keeps independent acknowledgement and configured task completion pending; viewing diagnostics records no financial approval or source/task mutation.

The scoped owner copies callback and exposed results and fences publication by original company/account snapshot, source version and review generation. Invoice, purchase, financial, policy and allocation reloads clear the diagnostic result. Company/account changes clear state through the existing owner; authenticated account changes clear displayed invoice/lines even if the selected account has not changed. Component disposal invalidates its in-flight review. Late responses cannot restore old diagnostics. Current roles and authenticated subject are checked again immediately before the API call; API revocation clears protected results.

Existing MudBlazor paper, alerts, button, loading indicator and responsive table are used. No custom CSS or JavaScript is added.

## Focused verification

The affected gate passes **1589 checks with zero failures/skips in 5.1150 minutes**. After policy/allocation invalidation was added, the final targeted gate passes **all 16 reviewer workspace/panel checks in 28.5487 seconds**. These runs overlap and differ in scope; the targeted run verifies the final invalidation and component-disposal changes. The final overlapping source gate passes **519 checks in 7.7865 seconds**. The initial checkpoint passed 1575 checks in 4.5354 minutes. All launched command handles reached terminal exit; whitespace verification passes. Composition reports 375 source/cache/package warnings. Canonical Data was unchanged in this slice and uses the clean actual Data build verified in Implementation 205.

Eleven scoped cases cover copied results, source/financial/policy/allocation invalidation, company/account/disposal fences, error settlement and maker/preparer refusal. Five static rendered-panel cases invoke actual handlers through typed clients, Kestrel and native AP owners, covering current diagnostics, changed receipt contributors, API grant revocation, authenticated account change without selection change and source-maker exclusion. All preserve four prior correction writes and DRAFT status. The panel is also compiled in the existing complete payables page composition.

Native provider/physical transactions and authoritative access capture remain fixture seams. Browser/OIDC/keyboard/accessibility, live schema/grants/provider, durable circuit recovery and broad solution acceptance are unverified. No SQL, new business EF lane, live migration/grant or phase closure is introduced.

## Next implementation target

Record version-bound independent acknowledgement and confirmed correction evidence for configured workflow work. Source correction and diagnostic viewing alone cannot authorize financial matching/posting/payment or task completion. Live definition provisioning, circuit lifetime recovery, tax/non-PO/book/period/journal evidence, policy tasks, stock/returns, wider persona dashboards and multinational acceptance remain active. No SQL, new business EF lane, live migration/grant or phase closure is introduced.
