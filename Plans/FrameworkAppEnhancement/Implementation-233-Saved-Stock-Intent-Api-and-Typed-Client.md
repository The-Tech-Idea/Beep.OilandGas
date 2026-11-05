# Implementation 233 — Saved stock intent API and typed client

## Company warehouse recovery lookup

The existing company inventory API exposes GET company/stock-intents/{intentId} through the native InventoryService recovery owner. It accepts only a canonical nonempty original intent identity. The endpoint requires authentication; current standard app roles and both Procurement.View and Inventory.View grant access. Mutation grants are not required to review already recorded evidence. Complete current account, company, sorted roles/grants and binding are retained and checked repeatedly through native capture and closing publication. IdentityServer remains authentication-only.

The native owner resolves the one retained original entry for this company and original recorded actor, then reconstructs original command/source audit and obtains complete current owned purchase/item/history evidence. The API validates the copied canonical recovery contract before returning it. Invalid route identities produce 400; missing authentication produces 401; failed current authority produces 403; unconfirmed, missing, altered, ambiguous or foreign retained evidence produces 409. Responses do not imply replay, repair or a quantity change. Recovery can succeed with valid original evidence despite later item, purchase or stock changes; the explicit current-postimage check remains false in those cases.

The existing typed accounting client adds GetPurchaseReceiptStockRecoveryAsync. It validates the original intent before HTTP, captures selected company/account, sends the explicit read route, requires a complete canonical result for that original scope/account/intent and rejects late selection changes. The returned copy separates retained original submission from current source. The original recording path, physical/financial boundary and three-write Beep transaction are unchanged.

## Focused verification

The final affected procurement/warehouse/receiver/handoff/screen gate passes 798 tests in 1.3176 minutes, including all 30 new recovery API/client cases. The overlapping source gate passes 566 tests in 15.5088 seconds. Both runs reach terminal exit 0 without failed or skipped tests; whitespace checks pass. Counts overlap and are not summed. Canonical Data is unchanged from implementation 232's actual clean build. Focused composition warnings remain.

Thirty new HTTP/client cases are included in the affected gate. Actual typed-client/Kestrel/native journeys recover writer-persisted evidence using read grants alone after exact recording, later item/purchase/receipt changes and an uncertain commit. They verify current-postimage distinction, original command/review preservation, repeated reads/caller copying and no added native writes. Denial cases include current role/grant/account mismatch, different selected company, different recorded actor, authority/binding drift, missing authentication, malformed identity and missing/duplicate/altered saved evidence. Fake-response client checks refuse missing or modified entry/source/history/revision/intent and late company/account switches; invalid intent never reaches HTTP.

## Remaining delivery

Implement actionable warehouse screen recovery after circuit loss or changed current source using this lookup. Preserve the distinction between an original recorded movement and current mutable source; do not replay old commands merely to rebuild UI state. Then verify complete procurement/warehouse/configured-inbox journeys across receipts, partial returns and later link cycles. Wider persona landing/screens and multinational acceptance remain active; no phase closes.

No live database operation, migration, grant assignment, deployment or commit/push is executed. Actual provider concurrency, Beep provisioning, browser/circuit/OIDC and accessibility/RTL/responsive acceptance remain open. The native recorder saves confirmed movement evidence; this lookup alone does not recover an unrecorded command lost with a circuit.
