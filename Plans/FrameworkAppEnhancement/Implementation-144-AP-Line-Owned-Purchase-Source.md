# Implementation 144: AP line owned purchase source

## Accountant and procurement handoff

The existing accountant invoice detail can review a recorded invoice-line PO reference and display the existing owned purchase-order source panel. The accountant can inspect the order, its exact referenced line and recorded receipt history, then follow the encoded order workbench link. Both Accounting.View and Procurement.View, with a current selected-company role, are required before the parent exposes this action and the API reads procurement source data. No permission or role grant is assigned.

The source panel states that receipt history covers all recorded order lines. No allocation of those receipts to this invoice line is established. Different recorded currencies and units remain visible without conversion, aggregation, quantity comparison or a matching decision. Creation, matching approval, posting and payment commands remain unavailable on this screen.

## One shared relationship review

TheTechIdea.Data owns APLinePurchaseSourceReview. It composes the existing APInvoiceSourceReview and PurchaseOrderSourceReview instead of duplicating their entities, ownership proofs or revisions. Validation requires the original company and actor, the exact invoice and line, the reviewed invoice revision, the recorded PO and PO-line identities, actual target-line membership and the same recorded supplier. Missing, changed, ambiguous or foreign evidence refuses.

The existing AP controller adds the read-only invoice-line purchase-source endpoint. Its source rechecks both permissions and the original company binding before the PO read and at completion. Two compound captures require the same reviewed invoice revision and unchanged order revision. This detects observed changes; it is not an atomic database snapshot. The existing typed accounting client validates the original context and shared response before returning it. These controller/client changes remain source-only pending actual HTTP compilation and integration verification.

## Existing screens and stale-response protection

APInvoiceWorkspaceState reuses PurchaseOrderWorkspaceState for the nested order projection. It clears that projection on failed reload, AP reload, disposal or company change. Independent purchase-read generations prevent older line reads from replacing newer evidence; company away-and-back and AP source resets discard late results. A failed purchase read preserves the current invoice evidence while clearing the linked order.

APInvoiceSourcePanel renders the review action only with explicit procurement-read eligibility and a callback. The owned order section and workbench link use the same eligibility gate. PayablesWorkbench checks the current company roles and both permissions again when the action runs, passes the current invoice content revision and reports a separate source-review failure. The parent page remains source-only; the actual state and nested panels compile and render in the focused gate.

## Focused evidence

The complete current Data source build passes with zero warnings/errors in 1.79 seconds. The affected actual-source gate passes **174 tests**, zero failed/skipped, terminal exit 0, in 1.4545 seconds. Log: `%TEMP%/oilgas-ap-purchase-source-144.log`. This includes the previous 155 cases and 19 new cases; overlapping implementation totals are not summed. The gate retains 175 existing dependency/source-precedence warnings.

New cases cover shared Web JSON with different recorded currencies/units; ten invalid company/actor/supplier/order/line/revision/reference conditions; original reviewed revision and encoded route; frozen order evidence; failed or wrong-source clearing; late company/AP responses; overlapping purchase reads; and actual nested panel rendering with eligibility enabled and disabled. Existing actual AP/PO services, generic repository/interface/transaction helpers, shared contracts, states and renderers compile in this gate. The new full endpoint, full typed client, parent page, DI host and live role bridge are not established by these tests.

## Remaining delivery

Verify the full API/client/parent HTTP path, current app-role bridge, revocation, browser/keyboard/RTL behavior and native provider consistency. Apply and reconcile the previously declared AP-line schema through reviewed Beep MigrationManager; no schema or live migration is executed here and no handwritten SQL or business EF path is introduced.

Establish canonical receipt-to-invoice allocation, required matching cardinality, unit/currency/reference evidence and discrepancy/reviewer handoffs before enabling matching commands. Complete independently configured buyer/receiver/accountant/reviewer duties, creation/approval/posting/payment, durable outcomes and recovery. All other persona landing/data/dashboard workflows and multinational requirements remain open. No feature, persona or phase is certified by this slice.
