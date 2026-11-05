# Implementation 222 — Atomic inventory-link cycle reopening

## Procurement reassignment and supersession

The existing PurchaseOrderService now atomically archives and reopens an inventory-link cycle. Shared Data contracts require an explicit ReopenResolved or SupersedePending action, original company/account/order/line, full current order/history review, binding revision, canonical GUID intent and bounded single-line reason. Resolved approval and rejection can both reopen; supersession requires a still-pending verified preparation. Neither action chooses or applies a new target. A later preparation and independent decision continue through the existing owners.

One complete original line is appended to PO_INVENTORY_LINK_HISTORY. The active line advances its history digest cursor and clears only active proposal/decision JSON, with reviewed actor/UTC audit. Its current inventory reference, quantities, commercial fields and original creation audit remain unchanged. The archived line retains the original preparer and independent reviewer proof exactly, and earlier history entries remain unchanged. A supersession review can acknowledge a changed current order header or target without pretending the old preparation reviewed that new evidence.

## Atomic native boundary and original-intent recovery

The command uses the existing request-bound repository factory and an owned Serializable Beep datasource transaction. Current authority callbacks, original datasource/connection identity, fresh nullable proof/cursor metadata and complete canonical history repeat around captures and writes. Global line, archive-key and cycle-intent reads refuse ambiguous or foreign identities. The cycle GUID is also its archive key; no separate generated retry identity is substituted.

Narrow row policies permit one append-only archive insertion and one exact original-line update. An exact unique archive read-back precedes clearing the line. Complete stored order/history evidence must confirm the original submission before commit, and durable post-commit captures must agree. Any pre-commit failure rolls both writes back. Unknown commit, closing authority loss or changed post-commit order remains unconfirmed; retry uses the exact original intent/reason/review and performs no extra writes when evidence still agrees. An older reopen intent cannot clear a later preparation or later cycle.

The shared result contract validates the complete predecessor history, exact archived preimage and original command fields, exact reopened line and complete reconstructed original order revision. The 256-entry chain bound remains fail-closed. Original independent reviewer separation is preserved in retained proof and subsequent native decisions; reopening itself is an explicitly authorized preparation-cycle action, not an independent approval or stock command.

## Focused verification

Actual Data builds with zero warnings/errors in 1.38 seconds. The affected gate passes 451 checks in 24.8203 seconds. Thirty-two new cycle cases cover pending supersession, resolved approval/rejection reopening, exact retries, unknown commit, post-commit order/authority drift, lost authority, changed source, missing metadata, archive/line failures and corruption, duplicate line/foreign intent, insufficient isolation, invalid submissions and altered result evidence. A continuous native fixture journey runs original preparation/decision, reopen, second preparation/independent approval and second reopen; earlier archive bytes remain unchanged and older intent retries refuse the newer work.

The initial test compile caught a fixture referencing a nonexistent description property; it was corrected to a quantity change. A rollback check then caught archive corruption being detected only after the line update. Exact archive read-back now refuses corruption before that update. The affected final gate passes. After strengthening old-intent assertions, the latest 32 cycle checks pass in 12.4723 seconds. The overlapping source gate passes 566 checks in 8.9084 seconds. All final runs reach terminal exit 0; composition warnings remain and whitespace checks pass.

Tests exercise actual canonical Data, native services, common-column handling and transaction code against fixture provider rows/metadata. They do not prove live provider locking, migration/grant provisioning, HTTP authorization, circuit recovery or browser behavior. No SQL, new business EF path, frontend database connection, live migration or physical stock/receipt/financial mutation is introduced. No phase closes.

## Next persona delivery

Expose owned cycle history and explicit review/reopen through the existing company inventory API and typed client with standard app-role/read-grant enforcement, a dedicated cycle mutation grant, complete current authority/binding fences and exact original-intent result validation. Extend the existing procurement screen/scoped owner for history, review, reason, save and uncertain-result recovery, keeping competing preparation/reviewer/receiver actions guarded. Verify the existing procurement, warehouse and inbox outcomes before stock/return or financial commands. Wider persona and multinational acceptance remain active.
