# Implementation 153: Receipt allocation proposal review

## Accountant preparation screen

The existing AP invoice detail now includes Prepare receipt allocation proposal after the invoice line's owned PO and allocation history have been reviewed. The preparer selects a recorded receipt on the exact PO line and enters a positive quantity. The displayed unit comes explicitly from that selected receipt; no unit conversion/default is substituted. Eligible choices require an active receipt, positive recorded quantity and recorded unit. Missing eligible evidence is shown explicitly.

Review proposal sources invokes a read-only API review and displays the returned receipt, proposed quantity/unit, content revision and UTC retrieval provenance. The panel states that no allocation is recorded by this action. It does not expose submission, approval, matching, posting, payment or stock effects. Global receipt consumption, invoice quantity/unit compatibility, receipt acceptance and matching tolerances remain required before a persistence command can be enabled.

## Existing permission and API-first owners

The existing permission catalog adds Accounting.PrepareReceiptAllocation for this source-review action, separately from Accounting.View and Procurement.View. No role or permission assignment is performed. The current selected-company standard roles remain authoritative; both read permissions plus preparation permission are rechecked by the actual API callback throughout the source read. This permission definition does not grant allocation writing or matching approval.

The existing AP controller exposes read-only POST invoice-line receipt-allocation-review. Its request uses one Data-owned APReceiptAllocationProposal with receipt ID, explicit quantity/unit and expected invoice/order/allocation revisions. It calls the existing source/allocation read helper and native owners, then validates the proposal against fresh source evidence. Denied preparation permission refuses before business source reads; unsupported source/proposal evidence returns the fixed Conflict response.

Data validation requires exact original company/actor/invoice/line and all three displayed revisions; the selected receipt must belong to the referenced PO line, be active, have a positive recorded received quantity and exactly the proposed unit code. Proposed quantity must be positive and cannot exceed that receipt's recorded quantity. This is a local source bound, not a globally available balance: allocations on other invoice lines and independent acceptance are not established by this review.

APReceiptAllocationProposalReview freezes the underlying allocation/source evidence, binds the exact immutable preparer input and records a digest/retrieval time. The typed accounting client captures the original selection, encodes route IDs, validates the exact returned proposal and refuses late/empty responses. No second HTTP/store/model owner is introduced.

## State and UI evidence

APInvoiceWorkspaceState keeps explicit receipt/quantity input and a readonly reviewed-proposal projection. Input changes invalidate the review; changing receipt clears quantity. Company, invoice, purchase source or allocation reload clears proposal context. Late source/input changes cannot adopt an earlier response. A failed review clears its result while retaining explicit preparer input for correction. This is a read-only retry, not durable command recovery.

PayablesWorkbench independently checks current roles and all three permissions for the action, has separate proposal progress/failure feedback and clears preparation eligibility/evidence when authority is denied. The shared source panel gates both form and result with explicit preparation eligibility inside the existing procurement source gate. No custom CSS/JS is introduced.

## Focused verification

Current complete Data sources build with zero warnings/errors in 1.92 seconds. The final affected actual-source gate passes **374 tests**, zero failed/skipped, terminal exit 0, in 3.2607 seconds. Log: `%TEMP%/oilgas-proposal-review-153.log`. It includes the previous 355 cases plus 19 new checks; overlapping totals are not summed. The existing 175 dependency/source-precedence warnings remain.

Five loopback Kestrel HTTP cases cover allowed shared proposal JSON, missing preparation permission before source reads, excessive quantity, wrong unit and changed allocation revision. Every case asserts no insert/update/delete/transaction/commit interaction. Seven Data cases cover exact active receipt/quantity/unit/revisions, another PO line, immutable input and frozen Web JSON. Five state cases cover explicit input/result, three late input/company/allocation changes and failed-reload clearing; two actual Razor cases cover the preparation gate/form/recorded unit/read-only result. The affected gate now includes 38 complete-controller HTTP cases.

Complete AP/PO controllers, actual helpers/native services/repository, Data contracts, state and shared panels compile and run in the focused gate. The complete typed Web client, parent page, production role bridge/context adapter/full host, browser/keyboard/RTL and real providers/schema/performance remain source-only or unverified. No business record is written, migration executed, SQL/business EF path introduced, role assigned or deployment performed.

## Next persistent handoff

Implement global, concurrent receipt-consumption review and configured acceptance/unit/currency/tolerance rules; then connect a reviewed allocation draft/submission to the existing durable command/transaction/readback patterns and independent reviewer workflow. Persist original command identity/input, distinguish recorded/not-recorded/uncertain outcomes and provide explicit reconciliation without automatic duplicate writes. Complete matching, invoice approval/post/payment and buyer/receiver/stock outcomes. Beep schema application, full UI/auth/provider verification and all remaining persona/company/multinational requirements stay open. No phase closes.
