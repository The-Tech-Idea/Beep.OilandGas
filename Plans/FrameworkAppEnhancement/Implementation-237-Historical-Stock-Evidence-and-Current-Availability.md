# Implementation 237 — Historical stock evidence with unavailable current source

## Original recorded effect and explicit current availability

Canonical stock recovery now distinguishes the retained original movement from nullable current owned purchase/item sources. Availability is explicit in both the recovery and compact discovery summary. No archived purchase/item snapshot is substituted for current data. CurrentMatchesOriginalEffect returns false whenever either current source is absent, and its revision includes source availability and original native evidence. The existing API/client/index/page owners use this same canonical contract.

The original recovery entity validates its recording ownership/account/audit/digest before returning required movement identities. If the current owned item is absent, the native owner reads only those original prior-manifest movements and the recorded intent movement, requiring globally unique native identities and the expected original item. The original submission is reconstructed only after complete snapshots, prior row hashes, command/review revision and recorded native movement agree. Later unrelated movements are not included in this historical fallback. Missing, changed or globally ambiguous original movement evidence refuses; the ledger snapshot alone does not prove a physical stock effect.

Current purchase and inventory availability/revisions are checked again before closing entry verification, and two complete recovery captures remain required under original authority/binding/source guards. Only confirmed absent owned rows produce historical availability states. Provider errors, malformed or ambiguous current source, missing schema, denied authority and altered retained evidence still refuse; exceptions are not downgraded to unavailable data.

The existing saved stock screen shows original audit/movement/preimage and explicit unavailable purchase/item messages. Current quantity/detail/capacity/stock result remains absent when the item is unavailable. The discovery landing retains original saved-evidence links and warns about unavailable current source. Saved-route record/retry controls remain absent. A matching original pending scoped intent can be reconciled from valid historical native evidence, releasing its uncertain stock block; this confirms original recording only and does not repair quantities or grant another stock action without fresh review.

## Focused verification

The actual canonical Data build passes with zero warnings/errors in 1.71 seconds. The preceding expanded affected procurement/warehouse/receiver/handoff/screen gate passes 885 tests in 2.9487 minutes. The source gate passes 566 tests in 11.9729 seconds. The final historical/pending-intent gate passes 15 tests in 4.8593 seconds. The earlier overlapping API/rendered historical gate passes 50 tests in 37.2176 seconds and native recovery gate passes 37 tests in 1.7893 minutes. All final runs reach terminal exit 0 without failed or skipped tests; whitespace checks pass. Counts overlap and are not summed; focused composition warnings remain.

Eight native historical cases cover missing purchase/item/both, a now-foreign item, retained prior receipt evidence for a return, and missing/altered/globally duplicate native movements. They restore the original command/review, report current availability without defaults and refuse unrelated later history disclosure, with no added writes. Added actual typed-client/Kestrel cases cover unavailable sources. The rendered saved route and paged landing show historical evidence and explicit unavailable states; current item quantity/capacity is not fabricated. A scoped pending-intent case verifies original reconciliation releases the procurement/stock guard while keeping current item/stock result empty.

## Remaining delivery

Malformed, ambiguous, retired or inaccessible current source is not treated as confirmed absence by this fallback; those states require separate governed handling. Completely missing original native movement evidence remains unconfirmed even when a saved snapshot exists. Commands never recorded by the server cannot be inferred or replayed.

Next verify complete procurement/warehouse/configured-inbox journeys across receipts, partial returns and later link cycles, preserving original maker/receiver/warehouse/reviewer audit and the physical/financial boundary. Continue wider company persona landing/screens and multinational acceptance. Actual provider paging efficiency/order/concurrency, live Beep provisioning, browser reconnect/OIDC and accessibility/RTL/responsive acceptance remain open; no phase closes.

No live stock mutation, schema migration, grant assignment, deployment or commit/push is executed. The change extends Data-owned read contracts and existing Beep/API/client/screen owners, without handwritten SQL, direct frontend database access or a new business EF lane.
