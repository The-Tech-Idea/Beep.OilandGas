# Implementation 217 — Procurement inventory-link drafting, save and recovery

## Existing order-screen workflow

The procurement order screen now loads the current company's owned active inventory targets through the existing typed client. Preparers select an active recorded PO line without retained proposal evidence and a target from that returned queue, then request the complete revision-bound source review. The screen shows current reference, proposed target and warehouse-detail drill-down, explicit warehouse/location, exact recorded unit, unknown on-hand quantity, source revisions and inventory retrieval UTC. A bounded single-line reason is required before the original canonical intent is created. MudBlazor components reuse the existing theme; no custom CSS/JavaScript or direct database path is added.

The current authenticated account, assigned standard app role, Procurement.View, Inventory.View and Procurement.PrepareInventoryLink are required by the handlers; the API independently enforces authoritative current company access. Receiver command handlers now also require the authenticated account to match the selected company account. Preparations do not apply inventory references, receive stock or post financial effects.

The existing scoped PurchaseOrderWorkspaceState owns copied preparation review, immutable target projections, original submission, busy state and confirmed result. It avoids publishing late results after company/account/context/disposal changes. Review and target selection changes invalidate displayed source evidence. Preparation and receiver review generations prevent an earlier receiver review from publishing after a newer preparation review. Receiver/linkage operations and additional preparation reviews are blocked while a preparation is busy or has an unconfirmed retained intent.

## Exact confirmation and recovery

Save retains the complete original submission before calling the typed API. The returned row must contain canonical retained proof for that exact command and full original review. A fresh complete owned PO read-back must agree exactly with the returned line, preserve all other header/line/receipt evidence, and reconstruct the original source revision when only the prepared line is replaced by its original reviewed preimage. Only then does the screen confirm preparation and refresh the existing PO source projection for handoff.

Failed, absent, corrupt, changed or late results leave the original submission pending. Clearing the display, changing company/account and disposing the page do not replace that pending intent in its scoped workspace. The recovery action requires return to its original company, account and order; edits to the form's reason cannot change the retained retry. Verified retained preparations also appear on PO lines after reload, with proposed target, preparer, UTC time and reason. Invalid retained proof is displayed as unconfirmed, and cannot become an eligible new draft line. The current recorded inventory reference remains visible and unchanged. This is circuit-scoped recovery; durable recovery across circuit loss remains open.

## Retained inventory-evidence permission boundary

PO source payloads now include retained inventory provenance. Existing PO detail and receiver source-review endpoints therefore require Inventory.View whenever any line carries preparation evidence; receipt recording checks that boundary on its submitted source as well. The shared AP-to-PO source reader enforces the same grant at initial and final authority boundaries, protecting financial/allocation reviews that reuse it. Ordinary unprepared PO sources retain their existing Procurement.View access. These checks apply even to malformed non-null retained evidence; they do not silently strip proof or invalidate source digests. Grant provisioning remains open.

## Focused verification

The final affected gate passes **327 checks in 8.0422 seconds**, zero failures/skips, with 407 composition warnings. New scoped-state checks cover frozen review/intent, late company/account/clear/disposal, changed actor/source, null/failed review, invalid reason, changed result/read-back/header, absent read-back, unknown-result reconciliation, return to original company/account, and competing receiver/preparation reviews. Seven complete procurement-page handler/render cases cover target loading and empty queue, account/grant refusals, failed review, save confirmation, unknown-result reconciliation and authentication invalidation. Existing API/native checks remain included. Three additional actual HTTP checks protect prepared PO/receiver reads, and three AP-to-PO source checks exercise allowed/missing/revoked inventory grants.

The overlapping source gate passes **566 checks in 6.8297 seconds**, with 359 composition warnings. Whitespace checks pass and all launched handles reached terminal status. Initial state invalidation and an extra AP authority capture that disturbed existing read-boundary behavior were corrected before the final gates. No shared Data changes were required; focused composition uses the actual Data assembly built cleanly in Implementation 216.

Page tests invoke the existing handlers through HtmlRenderer and mock the typed client; actual typed-client/Kestrel/native preparation is covered separately by the included API tests. This does not prove one continuous browser-to-live-provider journey. Provider metadata/transactions remain fixtures, and complete legacy host, browser/OIDC, keyboard/screen-reader/RTL/responsive behavior, circuit reconnect and live migration/grant provisioning are unverified here.

## Next persona delivery

Implement independently authorized reviewer approval/rejection and audited reference application through the existing PO owner/API/client/screen, including preparer/reviewer separation, exact source/target/binding fences, retained decision/intent history and subsequent reassignment cycles. Confirm outcomes in existing order/warehouse detail and shared task inbox. Continue physical stock/returns, accounting consistency, remaining personas and multinational acceptance after their actual prerequisites. No phase closes.
