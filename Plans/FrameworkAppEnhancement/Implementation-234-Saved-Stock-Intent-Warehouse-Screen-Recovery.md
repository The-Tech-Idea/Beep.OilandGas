# Implementation 234 — Saved stock intent warehouse screen recovery

## Warehouse persona screen and existing scoped owner

The existing receipt stock workbench adds /warehouse/stock-intents/{IntentId}. An explicit saved-intent link is available from the active receipt operation and from the warehouse landing when the operation is uncertain. A retained/bookmarked intent URL can restore evidence into a fresh inventory workspace using the typed API client, without the previous circuit's in-memory submission. The screen requires current authenticated account, standard app roles and both source grants; mutation grants are not needed for recovery review.

The existing InventoryWorkspaceState validates two complete copied recovery results under company/account/version/generation/lifecycle guards. A pending scoped intent must match the saved identity, full original command and original review revision before it can be reconciled. Missing, changed, competing or late evidence retains the pending uncertainty. Successful recovery confirms the original recorded movement even if current source changed. Current stock result/capacity review is populated only when current complete source matches the original exact postimage; otherwise current inventory evidence is displayed separately and another action requires fresh review. Recovery adds no stock command or native write. Clearing/disposal removes displayed recovery evidence and retains unresolved original intent under existing rules.

The saved screen displays original intent/account/UTC audit, order/receipt/item, movement/quantity/unit/reason, original warehouse/location/on-hand preimage and current warehouse/location/on-hand. It distinguishes exact current effect from later source changes and links to current receipt, item and procurement reviews. Recording/retry controls are absent on the saved route; its Save handler also refuses. It uses existing MudBlazor components, with no custom CSS or JavaScript. Original route identities are decoded from their raw address and canonical intent shape is required.

Overlapping route reloads clear the prior workspace generation and queue the latest source read. The route/account/authentication checks prevent a previous asynchronous read or error from publishing under a new route. Existing procurement/stock coordination continues to fence concurrent work; successful original-intent recovery releases pending stock blocking without rewriting physical history.

## Focused verification

The final affected stock/screen/workspace/coordination/warehouse gate passes 229 tests in 31.8273 seconds, including the added queued-route case. The preceding expanded affected procurement/warehouse/receiver/handoff/screen gate passes 818 tests in 1.0951 minutes. The overlapping source gate passes 566 tests in 13.8426 seconds. Canonical Data is unchanged from implementation 232's actual clean build; All final focused/source runs reach terminal exit 0 without failed or skipped tests, and whitespace checks pass. Counts overlap and are not summed; focused composition warnings remain.

Eleven new rendered screen journeys use the actual typed client, Kestrel API and native writer-backed recovery entry with a fresh scoped owner: exact and uncertain-commit outcomes, later item/purchase/receipt changes, missing evidence, missing source grant, mismatched account, changed authentication, malformed route and a delayed read replaced by the next route. A delayed client wrapper exercises queued navigation; actual API evidence supplies the replies. The saved route displays original/current distinction and handler-level refusal to replay, without added writes.

Ten new workspace cases cover pending exact/historical reconciliation, missing or altered evidence, a competing original intent, closing source drift and late company/account/clear/disposal changes. They check original pending state, both persona owners' blocking, copied display data, current quantity and no added native write. Existing receipt/return, local intent retry, warehouse landing and cross-persona checks remain included.

## Remaining delivery

This recovers a recorded intent from its known saved URL. Discovery/listing of previously recorded intents after losing that identity, and recovery of a command never recorded by the server, remain open. Do not infer missing evidence as permission to replay or repair stock. Actual browser reconnect/OIDC, provider transaction concurrency, live Beep provisioning and accessibility/RTL/responsive acceptance remain open.

Next verify complete procurement/warehouse/configured-inbox journeys across receipts, partial returns and later link cycles, then continue wider persona landing/screens and multinational acceptance. No phase closes. No live stock mutation, migration, grant assignment, deployment or commit/push is performed; persistence remains API-first and Beep-owned, without handwritten SQL or a new business EF lane.
