# Implementation 232 — Atomic original stock recovery recording

## Warehouse receipt and supplier-return persistence

The existing InventoryService stock command now persists canonical INVENTORY_STOCK_INTENT evidence in the same owned Serializable Beep transaction as the physical inventory movement and item quantity update. Fresh recovery-table metadata and globally unique intent lookup run before a write. A pre-existing recovery identity without the matching movement refuses; it cannot be overwritten or repaired by a new command.

After inserting and reading back the exact movement, the writer creates its canonical recovery entry, inserts it through a restricted append-only row policy, and reads back one exact entry before updating quantity. The policy permits only the original table/type/insert/account/object and original evidence. It normalizes the common recording audit to the movement's UTC audit and rejects all other differences. Canonical recovery GUID is the original intent identity, preventing common-column GUID generation from replacing the evidence. The existing closing source and repeated authority/binding checks remain in force.

Both in-transaction confirmation and post-commit confirmation require one exact recovery entry matching the original submission and recorded movement, plus restoration from complete native history. Original intent retries validate this retained evidence and the complete existing exact postimage without adding writes. Missing, duplicate, changed or colliding recovery entries refuse. Changed later current source still requires the separate recovery read flow rather than command replay.

The operation now has three native writes: movement append, recovery append and quantity update. Failed recovery insertion/read-back or later quantity/source/authority checks roll back all three owners. A committed operation with an uncertain acknowledgement retains its recovery record; a fresh native service can restore original intent and audit without the old service instance or another write. There is no direct frontend persistence, handwritten SQL or new business EF migration lane.

## Focused verification

The final affected procurement/warehouse/receiver/handoff/screen gate passes 768 tests in 1.2702 minutes. Canonical Data builds with zero warnings/errors in 1.50 seconds. The source gate passes 566 tests in 12.1200 seconds. The earlier overlapping focused command/recovery gate passes 73 tests in 15.3498 seconds; the final affected gate additionally checks fresh-owner recovery after uncertain commit. All final affected/source runs reach terminal exit 0 without failed or skipped tests, and whitespace checks pass. Counts overlap and are not summed; focused composition warnings remain.

Recovery read tests now use entries actually persisted by the native stock writer instead of seeding them. Eleven new command cases exercise failed/corrupted recovery insertion, authority/binding/purchase changes, duplicate read-back, pre-existing cross-owner identity, absent native metadata and post-commit missing/altered/duplicate recovery evidence. Existing command tests assert retained entries and rollback removal. HTTP/scoped workspace/rendered screen assertions include the third native write while retaining original receipt/return, recovery and source/financial invariants.

## Remaining delivery

Expose an authorized company API and typed-client original-intent lookup, then implement warehouse screen recovery after circuit loss or changed source. Native recording/read coverage does not prove browser reconnect or actual provider concurrency. Verify the full procurement/warehouse/configured-inbox journey across receipts, partial returns and later link cycles. Wider persona landing/screens and multinational acceptance remain active; no phase closes.

No live stock mutation, Beep migration, grant provisioning, deployment or commit/push is executed. Actual provider transaction/locking, live Beep provisioning, host/browser/OIDC, accessibility/RTL/responsive acceptance remain open.
