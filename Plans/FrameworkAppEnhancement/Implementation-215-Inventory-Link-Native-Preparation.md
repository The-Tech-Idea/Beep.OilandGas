# Implementation 215 — Retained inventory-link preparation in the existing PO owner

## First recorded preparation effect

PurchaseOrderService.PrepareInventoryLinkAsync records a retained proposal on the existing owned PO line in an owned Serializable Beep transaction. It preserves the current INVENTORY_ITEM_ID, including an existing assignment during proposed reassignment. It neither applies the target reference nor changes receipts, stock, AP, journal or financial approval. Independent reviewer approval/assignment remains required and is not delivered by this preparation method.

Shared Data contracts bind the current complete owned PO source, original line, explicitly selected active owned target inventory/movement source, exact matching unit, recorded warehouse/location, original preparer, binding, canonical GUID intent and reason. Missing ownership, inactive/same target, mismatched units or missing location refuse review. Command actor/IDs/reason are bounded and validated. Target quantity can remain unknown because preparation is not an availability claim. Source/header/other-line/receipt and target content revisions must still agree on every confirmation.

The existing PO-line entity adds nullable bounded INVENTORY_LINK_REVIEW_JSON (262144). Its canonical retained proof contains the command and original reviewed graph, exact resulting line content digest and preparer UTC audit. It preserves all original line values except permitted change audit and retained proof. Actor/time and canonical JSON are verified; a changed proof, original line, command/reason or source cannot be repaired or replaced through this path. Only one preparation can be retained on the line by this method; resolution and subsequent review/reassignment cycles are future independent commands, not destructive proof replacement.

## Owned transaction and original intent

The original company/resource/preparation authority callback is mandatory and re-evaluated at transaction, capture, write and confirmation boundaries. The caller must enforce current authoritative company/account/standard-role/grant/binding evidence; that API integration remains the next slice. The native owner also requires the original request-bound datasource and an owned Serializable transaction, fresh bounded nullable proof-column metadata, unique global line identity and exact owned line membership before the narrowly permitted update.

The existing generic repository uses a preparation-specific row policy; it cannot insert/delete or update another line through that policy. Common audit preparation precedes canonical proof generation. Fresh source and target read-back, exact proof and serialized row comparisons confirm before and after commit. Exact same command/review retry adds no writes. A different intent or reason refuses. Failed/corrupt writes roll back; unknown commit retains the original proposal for reconciliation. Post-commit target drift yields no confirmed result; restoring original evidence permits the same intent to reconcile without a new update.

The complete source snapshot can exceed the proof-column bound; preparation refuses rather than truncates evidence. This is a native preparation owner, not a client-exposed business command or independent approval. API access enforcement, endpoint/client/UI preparation, independent decision, recorded reference application and subsequent cycles remain open.

## Beep migration and proof shape

The existing canonical PO_LINE_ITEM is already in the ProductionAccounting module manifest. Its new nullable column is reflected by Beep MigrationManager; preparation verifies the physical column/type/nullability/bound before mutation. No live migration, SQL or EF business path is added. The new proof field is omitted from raw serialization and PO source line digest when unset, preserving the unprepared source shape. A stored proof participates in the source revision, invalidating stale receiving/stock reviews. Original inventory linkage is preserved until a separate approved command changes it.

## Focused verification

Actual Data builds with zero warnings/errors in 1.76 seconds. The final affected gate passes **91 checks in 3.9794 seconds**, zero failed/skipped, with 401 composition warnings. Eighteen new cases exercise the actual PO/repository/owned-transaction path and canonical review: first preparation, existing-link preservation, exact retry, initial/transaction authority loss, target drift, missing schema, corrupt/failed write rollback, unknown commit recovery, changed intent/reason and post-commit target drift/recovery; five review cases refuse wrong unit/company/location/inactive/same target. Prior actual receiver, receipt inventory HTTP/source and scoped review cases also pass. Initial missing test imports were corrected before the final gate.

The overlapping source gate passes **563 checks in 10.0674 seconds**, with 353 composition warnings. All launched handles reached terminal exit 0; whitespace checks pass. Beep provider operations, metadata and transaction state are fixture boundaries; no live provider locking, schema provisioning or application/browser/OIDC acceptance is claimed. No phase closes; wider company-persona and multinational work remains active.

## Next integrated workflow

Expose revision-bound preparation review/command through the existing API and typed client, independently enforcing current company/account/standard roles, explicit preparation grant and resource/binding fences. Add proposal drafting/save/recovery to the current procurement screen. Then implement independent review and apply/reject with original source/current-item/target-item/movement provenance, separate reviewer identity, explicit reason and atomic durable read-back. Retain preparation and decision audit through subsequent reassignment cycles. Only after approved link evidence exists should the existing inventory owner record physical receipt/issue/return effects. Ledger posting remains separate until complete financial and physical/financial consistency evidence exists.
