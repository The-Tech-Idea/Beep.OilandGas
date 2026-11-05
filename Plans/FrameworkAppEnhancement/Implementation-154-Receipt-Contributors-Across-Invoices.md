# Implementation 154: Receipt contributors across invoices

## Accountant workflow and screen

The receipt-allocation proposal review now shows allocations recorded against the selected receipt across invoice lines. Each row shows its owning invoice link, invoice line, allocation identity, recorded quantity/unit and activity flag. Invoice links encode the exact recorded identity, including slashes and literal percent sequences. The existing preparation permission and both accounting/procurement read permissions gate this view; no role grants are assigned.

The screen distinguishes a confirmed empty contributor read from unavailable evidence. A recorded active quantity total is shown only when every activity flag is explicitly Y/N and every active row uses the exact recorded receipt unit. Inactive history remains visible. Different active units or unknown activity produce an explicit unavailable total, without conversions or defaults. These quantities are not an available balance, accepted receipt amount, invoice match or approval.

Existing proposal state freezes the contributor projection. Input changes, source reloads, company changes and failed reviews clear the reviewed result through the existing generation guards. The typed client refuses a proposal response without receipt contributor evidence. The parent and complete client remain source-only; the shared panel and state compile in the focused gate.

## API-first source owners

The existing native APInvoiceService now queries AP_RECEIPT_ALLOCATION by exact PO_RECEIPT_ID, without selected-invoice or active-only restrictions. It checks fresh Beep metadata through the existing repository owner. Every contributing invoice is read with original tenant/company predicates and exact line membership; its supplier/order-line/receipt references must resolve to the same reviewed owned PO. Null, missing, incompatible, foreign, duplicate or changed evidence refuses the whole result; foreign rows are not silently discarded.

Receipt IDs can collide across companies without allocation ownership columns. Such ambiguous references conservatively refuse when owning invoices cannot be established in the original company. This change does not claim a provider-independent way to distinguish otherwise colliding complete references. Review owner-qualified allocation identity/schema before enabling persistent commands.

The Data-owned APReceiptConsumptionReview composes existing invoice-line allocation reviews, freezes contributors and binds their revisions to the selected invoice/PO/receipt and UTC provenance. It rejects duplicate invoice-line groups or allocation identities across groups. No duplicate business entity, SQL, EF owner or alternate store is added.

The existing proposal-review endpoint validates preparer input, reads contributors, refreshes selected invoice/order/allocation sources, reads contributors again and compares their revisions before returning the closing review. Original actor/company/binding, current standard app roles and all three permissions are rechecked before global reads, each contributor read and publication. Changes return a fixed Conflict or Forbidden result. No database record is written.

Two matching reads detect observed changes; they do not provide an atomic snapshot, reservation or concurrent consumption guarantee. The model retains an explicit absent-evidence state for read-only source reviews, and the current production client requires the complete contributor review.

## Focused evidence

Current complete Data sources build with zero warnings/errors in 1.53 seconds. The final actual-source gate passes **398 tests**, zero failed/skipped, terminal exit 0, in 4.5100 seconds. Log: `%TEMP%/oilgas-receipt-consumption-154.log`. The 175 existing dependency/source-precedence warnings remain. Whitespace verification passes.

Twenty-four new checks extend the previous 374-case gate: ten Data tests for frozen JSON, exact unit/activity arithmetic, ambiguous identity, changed content and confirmed empty evidence; eight actual native-service cases for another invoice, owner/line/receipt failures, null/duplicate rows and revoked authority; four complete-controller loopback Kestrel HTTP cases for other-invoice contributors, changed content, foreign ownership and revoked procurement access; two actual Razor/state cases for frozen contributors, exact encoded invoice links, different-unit refusal and input clearing. The gate now includes 42 complete-controller HTTP cases. Scopes overlap and are not summed across runs.

Complete AP/PO controllers, actual native services/repository, Data contracts, state and shared panels compile and run. Full typed client/parent pages, production role bridge/context adapter/full host, browser/RTL/accessibility, actual provider/schema and performance acceptance remain unverified. No migration, grants, deployment or financial writes were performed.

## Remaining handoffs

Before persistent allocation submission, establish owner-qualified identities, atomic concurrent receipt consumption, accepted receipt state, invoice/PO unit compatibility, currencies/tolerances and independent reviewer authority. Reuse existing durable command/Beep transaction/readback patterns for explicit draft/submission identity, recorded/not-recorded/uncertain outcomes and reconciliation. Invoice matching, independent approval/post/payment and buyer/receiver/stock outcomes remain open. This slice advances the accountant screen; it does not complete that persona or any roadmap phase.
