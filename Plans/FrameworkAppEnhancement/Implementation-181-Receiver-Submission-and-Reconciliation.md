# Implementation 181 — Receiver submission and original-intent reconciliation

Date: 2026-10-07. Status: existing procurement receipt screen, circuit workspace, PO API and complete typed accounting client now execute the native receiver decision and exact-intent recovery. Broader receiver-to-accountant workflow routing, corrections/stock and purchase-to-payment remain open.

## Bound source and independent write authority

TheTechIdea.Data owns PurchaseReceiptDecisionReview, PurchaseReceiptDecisionSubmission and PurchaseReceiptDecisionResult. The bound review contains the complete owned order snapshot, one explicit receipt and current production binding. The result validates the exact original command, original physical receipt, canonical persisted proof and UTC confirmation. These extend the existing canonical receipt command; no client DTO, alternate store or mutation owner is introduced.

The existing PO controller exposes read-only receipt decision review and receipt decision submission routes. Read preview requires current standard company roles and Procurement.View, and confirms the source/binding twice. Submission and retry independently require Procurement.View plus the separate explicit Procurement.RecordReceiptDecision grant. The API freezes the input and repeatedly requires the original company, actor, binding and unchanged current role/permission sets through the native transaction owner. Route/body mismatch is bad request; denied current authority is forbidden; changed evidence, provider refusal and uncertain transaction/rollback return fixed conflict without exposing exception text.

The new permission constant grants physical receiving only. It does not grant invoice allocation, matching, posting, payment, stock movement or financial approval. No grant is automatically seeded or assigned. IdentityServer remains authentication-only; configured application/company roles and current RBAC permissions are authoritative.

## Complete typed client and existing circuit owner

AccountingServiceClient adds the bound receipt read and original submission/reconciliation to its complete interface. It escapes exact source identifiers, freezes inputs, requires the original selected company/account, checks the returned bound source or exact result, and rejects context changes. The existing company transport invalidates changed-company calls. No direct frontend datasource/EF access is added.

PurchaseOrderWorkspaceState is extended as a partial class and registered once per circuit. The existing procurement page injects it. Its visible source/review/result clear on page disposal and company/account/source changes while an unresolved original intent is retained. Pending and unknown intent prevent another command/review; an exact retry keeps the original reason, quantities, receipt, revisions, binding, company/account and GUID. Returning to that original account/company/order enables reconciliation and an encoded recovery link. A recreated page subscribes to completion events and can reconcile the same circuit intent. Circuit loss/new browser sessions and historical durable recovery are still open.

After a valid result, the state stages a fresh full order read through the existing projection owner. The exact persisted receipt must appear in the owned read-back, proof must validate, and replacing only that receipt with its proven original must reproduce the original complete reviewed order revision. Changed other order evidence or a wrong receipt prevents confirmation. Only then are the refreshed order/line/receipt frame, new revision and confirmed decision published together. Late response adoption is refused without losing the original uncertain intent.

## Actual receiver screen and existing handoff surface

ProcurementWorkbench adds an explicit recorded receipt selection, bound source review, labelled accepted/rejected inputs, required bounded reason, record action, reconciliation action, recovery link, progress, unavailable/error and confirmed states using Mud components. Accepted plus rejected must partition the recorded received quantity exactly; units come from the reviewed source. A reason/quantity failure before submission reports that the action could not start; a submitted unknown outcome instructs original-intent reconciliation.

Handlers independently check current standard ASP.NET company roles, Procurement.View and Procurement.RecordReceiptDecision. Controls disable for missing selected-company receiver grants or in-flight/unknown intent. The API remains independently authoritative when grants are revoked. Page recreation retains recovery through the scoped existing owner rather than creating a separate intent store. No custom CSS, JavaScript or theme palette is added.

Confirmed receiver evidence appears in the existing receipt history. Its refreshed order revision flows to the existing CompanyAccountingHandoffPanel, whose existing source attachment invalidates stale handoff evidence. The existing assigned-task inbox remains on procurement landing. This enables the source review/handoff surface; it does not automatically complete a process step, route a receipt task or invent configured receiver/accountant assignments. Explicit configured cross-role task transitions remain next.

## Focused verification

Actual Data source build: zero warnings/errors, 1.69 seconds. Log: %TEMP%/oilgas-receiving-181-data.log.

Scripts/Test-PurchaseOrderOwnership.ps1 with actual Data and -IncludeWorkflowInbox: **967 passed, zero failed/skipped, terminal exit 0, 27.5529 seconds**. Log: %TEMP%/oilgas-receiving-181-final.log. Twenty-four new cases extend the prior 943-case gate; counts overlap prior slices and are not added.

New checks execute actual controller, full typed client, native receipt writer, existing state and complete procurement page handlers. Cases cover bound preview/exact escaped IDs/retry without duplicates; current role/read/write/binding and changed role/permission sets; preview-only authority; changed source/body binding/invalid reason/route; wrong selected account/company; uncertain commit with original reason and refreshed proof; copied intent; busy duplicate refusal and late company/source outcomes; wrong receipt or changed whole-order read-back; actual page recording/permission/reconciliation/reason failures; and recreated-page circuit recovery. Existing procurement landing/inbox render fixtures now register the same scoped state owner.

The smaller source composition passes **517 checks**, zero failed/skipped, terminal exit 0, 7.1543 seconds. Log: %TEMP%/oilgas-receiving-181-source.log. Counts overlap. The larger composition retains 339 warnings, including six new current-source precedence warnings for existing refusal/permission types in the PO controller. No new changed-source nullable or other compiler warning is reported. Whitespace checks pass.

These tests use explicit current RBAC/binding, physical provider and static renderer/auth/JS seams. Real host/DI/OIDC, browser/keyboard/RTL/accessibility, live Beep migration/provider durability and concurrency remain unverified. The production Program scoped registration is present in source; focused complete-page tests exercise shared state reuse. This slice adds no business schema, SQL, business EF lane, live migration/grant, commit, push or deployment.

## Next delivery and remaining roadmap

Carry verified receiver acceptance into the existing accountant allocation preparation and complete receipt-contributor review, keeping received capacity, accepted capacity and recorded invoice consumption explicit. Connect confirmed receiving evidence to configured receiver-to-accountant workflow tasks through the existing reviewed company process/inbox owners; do not infer handoff completion from physical receipt status or a screen confirmation.

Independent corrections/returns, stock/valuation, company matching/tax/rounding policy, ledger linkage, financial decisions/post/payment, durable new-circuit recovery/outbox, delegation, wider dashboards/personas, multinational requirements and live host/browser/provider verification remain active. Beep MigrationManager continues to own business schema. No phase or feature closes.
