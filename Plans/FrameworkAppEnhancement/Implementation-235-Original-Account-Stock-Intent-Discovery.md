# Implementation 235 — Original-account stock intent discovery

## Warehouse landing and API-owned index

The warehouse landing adds an explicit Find saved stock intents action for the current company/account with both source grants. It displays original order/receipt/item, physical movement/quantity/unit, recording UTC and whether current source still matches the original exact effect, with a direct saved-evidence recovery link. This allows a fresh scope to find recorded movements without knowing or retaining their intent URL. It uses the existing warehouse page, inventory workspace and typed accounting client; no parallel persona owner, custom CSS or JavaScript is introduced.

Canonical Data owns the stock intent index and compact summary contract. Each summary carries the validated original command, original item/recording audit, retained entry revision, complete current recovery revision and explicit current-match result. The index validates original company/account, unique sorted identities, bounded field/quantity/kind/original-receipt vocabulary, UTC audit and a complete revision. Full purchase/item snapshots remain in the separate canonical recovery entity rather than being copied into discovery summaries.

The existing InventoryService enumerates original-company/account entries through Beep, validates fresh recovery metadata and every entry through the complete native recovery owner, verifies closing membership and repeats the entire capture. Ownership, type/identity ambiguity, altered evidence, source changes and request-bound authority/connection/source drift refuse publication. The index is bounded at 200 entries; larger results refuse rather than silently dropping records. This is an initial bounded index; provider paging is still required for larger company histories. Any candidate whose original/current evidence is unavailable refuses the complete index rather than inventing a partial successful list.

GET company/stock-intents uses authentication, current standard app roles and both Procurement.View/Inventory.View. Mutation grants are unnecessary for discovery. The typed client freezes selected company/account and validates the full index after HTTP, discarding late scope changes. The existing scoped workspace clears discovery on lifecycle/context resets, holds stock busy fencing during the read and rejects late generation/version/scope results. The screen hides previous entries while discovery is running or failed. Discovery adds no native write, stock command or financial effect.

## Focused verification

The actual canonical Data build passes with zero warnings/errors in 1.39 seconds. The expanded affected procurement/warehouse/receiver/handoff/screen gate passes 843 tests in 1.3071 minutes. The overlapping source gate passes 566 tests in 12.1544 seconds. The final discovery/API/client/rendered/workspace gate passes 28 tests in 13.6356 seconds. All final runs reach terminal exit 0 without failed or skipped tests; whitespace checks pass. Focused composition warnings remain.

Twenty-four API/client/rendered cases in the expanded gate validate actual writer-backed exact, empty, later-receipt and other-account history; denied read grants, duplicate/altered records, changing authority, complete membership/source drift and the 200-entry bound; malformed index/entry/revision/scope/account/duplicate responses and late company/account switches. Actual warehouse handlers call the typed client/Kestrel/native owner, displaying empty/changed-source states and canonical recovery links without knowing the saved identity beforehand. Four further workspace cases verify late results are discarded after company/account/clear/disposal changes. Test scopes overlap and are not summed.

## Remaining delivery

Add provider-backed pagination and large-history acceptance; the 200-entry bound is not a permanent scale solution. Unavailable referenced sources cause an explicit unconfirmed index and still require an evidence-governed historical recovery approach. Commands never recorded by the server cannot be discovered as committed effects. Do not infer missing evidence as permission to replay or repair quantities.

Then verify full procurement/warehouse/configured-inbox journeys across receipts, partial returns and later link cycles, and continue wider persona landing/screens and multinational acceptance. Actual provider concurrency, live Beep provisioning, browser reconnect/OIDC and accessibility/RTL/responsive verification remain open; no phase closes.

No live migration, stock mutation, grant assignment, deployment or commit/push is executed. Business persistence remains API-first and Beep-owned, without handwritten SQL or a new business EF lane.
