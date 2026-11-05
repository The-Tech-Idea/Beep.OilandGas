# Implementation 221 — Inventory-link cycle history source

## Procurement audit prerequisite

Subsequent reassignment and supersession need to retain the original preparer's proposal and independent reviewer's decision. The canonical Data owner now defines PO_INVENTORY_LINK_HISTORY, holding one complete archived line per cycle with explicit tenant/company/order/line ownership, cycle intent, source/binding revisions, reason and actor/UTC audit. A nullable active-line history revision points to the latest entry. Each entry references its predecessor by key and digest; it does not embed the entire earlier history again.

This slice delivers the history contract and native read prerequisite. It does not yet archive a line, reopen a preparation, apply another reference or expose history through an API or screen. Existing preparation/reviewer screens remain the persona implementation surface. No stock, receipt or financial effect is added.

## Complete retained evidence

The shared history review freezes entries in sequence order and requires a complete contiguous chain, unique entry/cycle IDs, exact predecessor references, canonical entry digests, monotonic audit times and the active line's exact head revision. Every archived line must retain valid original preparation proof and, when present, independent decision proof. Ownership and canonical archived JSON must agree exactly. Source reviews now include non-null history cursors in their revisions and refuse malformed cursors.

At most 256 entries and 4,194,304 characters per archived line are accepted. Exceeding these bounds refuses confirmation; it does not silently truncate or claim paginated history is complete. These digests establish consistency of retained records, not an externally signed audit or a current inventory-availability assertion.

## Existing native owner and Beep migration

PurchaseOrderService reads through its existing request-bound repository factory. Required authority callbacks and original datasource/connection checks repeat around captures. Fresh metadata must contain the nullable 64-character line cursor, canonical history key, required history columns and complete archive bound. Native queries filter all four ownership/resource fields; returned typed rows are independently validated.

Two full order/history captures must agree. A final complete order read and cursor check catches order changes after the second capture. No writes occur. The canonical history entity is registered in the existing ProductionAccounting module entity manifest for Beep MigrationManager provisioning, and PO_LINE_ITEM owns the cursor metadata. No handwritten SQL, new EF business path or frontend database access is introduced. Registration is source verified; no live migration or complete module/solution build was executed.

## Focused verification

Actual Data builds with zero warnings/errors in 3.36 seconds. The final affected gate passes 419 checks in 23.1065 seconds. The overlapping source gate passes 566 checks in 7.6169 seconds. Composition warnings remain; both final test runs succeed without failed or skipped tests. Whitespace checks pass.

Sixteen new history checks exercise copied valid/empty history, authority loss, foreign ownership, duplicate/missing entries, missing table/cursor metadata, order drift, wrong head and altered sequence/predecessor/intent/audit/archive/revision evidence. The initial drift case exposed a change after the second order capture; the final order check fixes it and the final gates pass. Fixtures use actual canonical Data, native PO/preparation/decision services and mocked provider metadata/rows. They construct archived history as a prerequisite fixture; they do not prove an implemented archive transition or a continuous browser/live-provider journey.

## Next persona delivery

Implement one atomic archive-and-reopen command through the existing PO owner: append the complete original line, advance its digest cursor and clear active preparation/decision proof while preserving the current reference. Require complete original intent/source/binding evidence and authoritative actor checks, then extend the existing API/client/procurement screens for reassignment and supersession with independent reviewer separation and recovery. History API/screens, transactional provider acceptance, migration/grant provisioning, browser/OIDC/accessibility/RTL and durable circuit recovery remain open. No phase closes.
