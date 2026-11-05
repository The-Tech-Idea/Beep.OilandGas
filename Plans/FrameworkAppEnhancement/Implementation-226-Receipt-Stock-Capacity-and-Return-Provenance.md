# Implementation 226 — Receipt stock capacity and return provenance

## Warehouse workflow source prerequisite

The existing canonical INVENTORY_TRANSACTION now has a nullable, bounded PURCHASE_RECEIPT_STOCK_JSON field. Its compact movement proof records owning company, order/receipt/item, exact unit/warehouse/location, receiver-decision revision, original stock source review/binding/intent, actor/reason, positive physical quantity and prior/result balances. Returns reference an original receipt transaction. The proof validates the complete native row digest, signed movement quantity, canonical intent identity and unchanged UTC recording audit without embedding earlier histories. It does not establish financial posting.

PurchaseReceiptStockReview reuses the complete PurchaseReceiptInventoryReview source graph. It exposes accepted quantity, gross stock receipts, returns and remaining receipt capacity. Returns never replenish gross receipt capacity. Each return must identify a matching original receipt and cumulative returns cannot exceed that original quantity. Retained source/company/receipt/decision, unit and destination evidence must agree. Gross receipts cannot exceed the receiver's acceptance. Fully rejected and fully stocked receipts have zero remaining capacity.

Unknown or negative recorded quantity on hand refuses stock review. Unproven receipt/return or unclassified movement history requires reconciliation; explicit active negative USAGE remains recorded source evidence and is not counted as receipt acceptance. This review does not derive available inventory from incomplete movements or fabricate an opening balance. Historical movements for the same item are checked for company/proof/return membership; the current receipt's accepted capacity is evaluated separately.

## Existing API and typed client

The existing inventory source service requires freshly retrieved nullable string provenance metadata of at least 16384 characters for stock review. INVENTORY_TRANSACTION is already registered in the canonical ProductionAccounting module setup; the additive column uses that existing Beep MigrationManager lane. Ordinary source reads retain their existing schema boundary.

The existing company API adds GET company/purchase-orders/{orderId}/receipts/{receiptId}/stock-review under the traditional inventory controller. Current standard assigned app roles, Procurement.View and Inventory.View protect repeated complete source captures. A final order capture after each stock-history read catches late order drift. Current full authority/binding and both complete source revisions must agree. Existing receipt linkage routes preserve their refusal status behavior.

The existing typed accounting client escapes exact identifiers, validates the original company/account/order/receipt and complete stock review, copies returned evidence and refuses changed selected context. No frontend database access or duplicated entity/DTO owner is introduced.

## Focused verification

The actual canonical Data build passes with zero warnings/errors in 2.25 seconds. The affected inventory/procurement/receiver/handoff/screen gate passes 634 tests in 52.5346 seconds. The overlapping source gate passes 566 tests in 8.6022 seconds. The final receipt-stock gate passes all 29 tests in 6.1508 seconds, including the two added zero-capacity edge cases. All final runs reach terminal exit 0 without failed/skipped tests. Composition warnings remain; whitespace checks pass. These scopes overlap and counts are not summed.

New actual typed-client/Kestrel/native fixture journeys cover empty, partial, returned, fully rejected and fully stocked receipt history; escaped identifiers; recorded usage; unknown/negative balances; excessive receipts/returns; missing original receipt; foreign company; unit/location/decision disagreement; legacy/unclassified/malformed evidence; changed row/audit; undefined movement kind; missing/undersized/nonnullable metadata; missing grants; binding drift and order drift during closing stock-history capture. No additional native writes occur. Returned review copies preserve the original destination. An initial gate caught an unintended existing binding-refusal status change; it was corrected and the affected existing route checks pass.

## Remaining delivery

This is a read-only warehouse stock/return source prerequisite, not the stock command or completed persona screen. Next implement receipt recording through the existing canonical inventory owner with owned atomic item/transaction writes, exact current authority/source/binding, frozen intent and durable read-back/reconciliation. Then implement original-receipt-bound supplier returns and actionable warehouse/receipt screens and landing outcomes. Financial posting requires its own reviewed owner and authority.

No live migration, grant provisioning, stock mutation, handwritten SQL or new business EF path is introduced. Beep column provisioning, provider concurrency/locking, full host/browser/OIDC, accessibility/RTL/responsive behavior and durable circuit acceptance remain open. Wider personas and multinational acceptance remain active; no phase closes.
