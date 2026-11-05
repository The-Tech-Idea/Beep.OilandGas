# Implementation 239 — Recorded receipt destination after approved link change

## Business behavior

An independently approved change to a PO line's inventory item previously made physical stock review follow the new item. Earlier receipt/return movements remained on the original item, so the new item's empty history could show accepted receiving capacity as unused. Returns against the original receipt could also become inaccessible. A second problem used that current PO-link review to confirm a successful stock save, leaving the screen uncertain after a destination change.

The existing physical receipt workflow now resolves its inventory destination from complete native receipt movement membership. Before any recorded stock movement, it uses the current PO line's item. Once a receipt has verified physical receiving history, all of its stock movements must share that recorded destination. An approved line change remains effective on the PO, while the existing receipt's remaining capacity and returns continue against its original physical item. This does not transfer stock to the new item. Different physical destinations require separate governed receiving/transfer evidence.

The warehouse receipt screen explicitly shows when the PO line's current item differs from the receipt's recorded stock destination. It identifies both items and explains that returns and capacity use the original history. After save, confirmation reads the same complete physical-stock review used by the form, preserving the scoped owner's exact intent/result/source checks.

## Existing owners extended

The canonical Data-owned receipt inventory contract resolves stock destination from typed, canonical movement proofs and unique identities. A differing current line/item is accepted only with a verified original receipt movement matching company, order, receipt, receiver decision, unit, warehouse and location. Current line item and recorded-destination state are derived from the existing source graph; no business table or parallel DTO is introduced.

The existing Beep inventory owner reads complete receipt-reference membership, rejects multiple physical items or returns without original receiving history, and loads the owned recorded item with its complete native history. It validates current receipt movements against the complete company/order/receipt saved-intent membership and restores each retained original record against native movement evidence. Missing movement or saved entry, altered provenance/digest, ambiguous identity, foreign ownership and mismatched receipt reference refuse review instead of producing fresh capacity. Receipt-reference metadata is required. Native receipt and saved-entry membership are copied and recaptured before return, and datasource connection/object identity is guarded.

The existing company API read owner uses this resolver for physical stock review, and the stock writer uses it again inside its existing owned serializable Beep transaction. Current app roles, both read grants, movement grant, original company/account/binding, exact command and complete result guards remain in place. Generic current PO-link review remains available for procurement's current line relationship; the receipt stock screen no longer uses that relationship alone as post-save confirmation.

No hand-written SQL, business EF migration lane, frontend database access or stock replay is introduced. Existing Beep-registered inventory and stock-intent entities supply all persisted evidence; no live migration or mutation is executed.

## Focused evidence

Canonical Data builds with zero warnings/errors in 2.24 seconds. The final affected native/client/API/workspace/rendered gate passes 204 tests with zero failures/skips in 1 minute 4 seconds. The source gate passes 566 tests with zero failures/skips in 11.2216 seconds. Whitespace checks pass. Counts overlap; focused composition warnings remain. The preceding 70-case gate is superseded by the final affected gate. An intermediate rendered test exposed the incorrect post-save read path; the final gate verifies its fix.

The combined maker/receiver/warehouse/link/cycle/configured-accountant journey now includes an approved alternate target after earlier physical receipts and partial returns. Actual typed-client/API stock review retains four gross units received, two returned and four units remaining on the original item despite the new PO link. A five-unit receipt refuses before writing. A later one-unit native return still records against the original receipt/item, producing three returned units and eleven on hand without freeing gross receiving capacity. Independent inbox completion retains original maker/receiver/warehouse/link audit and leaves physical/financial records unchanged through that completion.

A rendered receipt screen journey drives actual typed-client/API/native return recording after independent destination approval. It displays the recorded/current-item distinction and confirms the original-item return through complete read-back. Six additional native cases refuse missing movement, missing saved entry, duplicate movement identity, foreign saved ownership, hidden receipt reference and changed saved digest before a fresh review. Existing receiver/stock/authority/uncertain-result and recovery-screen checks remain in the affected gate.

## Scope of proof and next delivery

These are controlled native Beep datasource/transaction fixtures and local Kestrel API/HtmlRenderer checks. Alternate target callbacks in the approval fixture supply canonical owned target reviews; real provider target lookup is not established by this fixture. No real browser/OIDC, provider concurrency or live schema provisioning acceptance is claimed. Receipt membership validation currently reads and restores complete retained records; large-history/provider efficiency and ordering acceptance remain open.

Continue company persona landing/task navigation across the wider thirteen-persona plan, starting with the commercial/land landing and its revenue/royalty/partner source drill-downs. Keep current app role/grant/company boundaries, explicit unavailable states, and the distinction between reviewed work and physical/financial effects. Multinational units/currency/time zones, localization/RTL, accessibility and responsive/browser reconnect acceptance remain required. No phase closes and the overall implementation goal remains active.
