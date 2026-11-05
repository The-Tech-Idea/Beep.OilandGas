# Implementation 231 — Original stock recovery evidence

## Warehouse persona recovery prerequisite

Canonical Data now owns INVENTORY_STOCK_INTENT: bounded original command, purchase source and item snapshots; a sorted manifest of prior movement identities and complete row revisions; the committed physical movement revision; and original company/account audit. The separate entity is registered in the existing PRODUCTION Beep module entity list. It avoids embedding earlier recovery entries in inventory transaction history. There is no handwritten SQL or new business EF migration lane.

Restoration reconstructs the exact original stock review from its retained item audit, original purchase source and unchanged prior native movements. It validates original scope, actor, receipt/item/intent, canonical serialized evidence, recording audit, entry revision, original review revision and the full committed movement. Original receipt/return capacity checks remain authoritative. Missing or altered history, duplicate identities, changed command/snapshot/audit and movement disagreement refuse restoration.

The existing native InventoryService provides a read-only recovery owner. It requires a current trusted authority callback and stable request-bound PRODUCTION connection/source, fresh bounded nonnullable Beep metadata, one globally unique intent entry belonging to this company and original actor, and complete current owned purchase/inventory sources. Two complete captures, closing order/entry checks and repeated authority/binding checks refuse drift. The returned copied contract separates restored original submission from current owned source: a later item, purchase or stock change can preserve valid original movement evidence while current state no longer matches its exact postimage. Recovery reads do not replay a command or change quantities.

## Focused verification

The actual canonical Data build passes with zero warnings/errors in 1.58 seconds. The focused native recovery gate passes 22 tests in 7.8704 seconds. The overlapping source gate passes 566 tests in 10.8917 seconds. The expanded affected procurement/warehouse/receiver/handoff/screen gate passes 757 tests in 51.7828 seconds. All three gates reach terminal exit 0 without failed or skipped tests; whitespace checks pass. Focused composition warnings remain; counts overlap and must not be summed.

The new tests execute the existing native stock writer, then explicitly seed the recovery entity into the native fixture. They verify exact restoration, later item/purchase/receipt changes, original actor/company fences, altered snapshot/command/audit/digest/movement, missing or duplicate records, fresh schema bounds/nullability, authority/binding changes and closing capture drift. A later return restores from a prior movement manifest without recursively embedding the earlier recovery entry; mutation of prior native evidence refuses. The recovery reader adds no native writes.

## Remaining delivery

This is the canonical evidence and native read prerequisite. The production stock writer does not yet persist this entity. Integrate its insertion and read-back into the existing owned Serializable movement/item transaction, then expose authorized API/typed-client lookup and actionable screen recovery after circuit loss or intervening source changes. Tests currently seed the entry explicitly; they do not prove automatic durable recording, reconnect recovery or a provider deployment. Do not mark durable recovery or a phase complete.

Then verify the complete procurement/warehouse/configured-inbox journey across receipts, partial returns and later link cycles. Wider company personas, role-specific landing pages and multinational acceptance remain active. No live migration, stock mutation, grant assignment or deployment is performed. Actual provider concurrency, Beep provisioning, host/browser/OIDC and accessibility/RTL/responsive acceptance remain open.
