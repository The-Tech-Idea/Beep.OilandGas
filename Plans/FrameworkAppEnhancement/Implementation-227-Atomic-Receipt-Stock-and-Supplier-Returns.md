# Implementation 227 — Atomic receipt stock and supplier returns

## Existing native warehouse owner

InventoryService now records reviewed receipt stock and original-receipt-bound supplier returns through its existing canonical INVENTORY_ITEM and INVENTORY_TRANSACTION owners. Canonical Data owns the frozen stock command/submission and exact result validation. The command requires the original company/account/order/receipt, explicit movement kind and positive quantity, exact unit, complete stock review revision, binding, canonical intent and bounded single-line reason. Receipt quantity cannot exceed remaining gross receiver acceptance. A return must identify an original physical receipt for the same order/receipt, fit its cumulative unreturned quantity and fit recorded quantity on hand. Returns never free gross receipt capacity.

The command requires a trusted current-authority callback and request-bound PRODUCTION resolver. An owned Serializable Beep transaction captures complete purchase/item/history evidence, rechecks the purchase after inventory capture and validates globally unique original item and movement-intent keys. Fresh canonical key/proof/decimal-quantity metadata is required. Changed source, binding, actor authority or ambiguous identity refuses. No default connection or borrowed/nested transaction is authorized.

The narrow repository policy appends only the original physical movement. It normalizes recording audit, stores compact canonical source/receiver/binding/intent/quantity/original-receipt proof and checks exact unique native movement read-back before updating the item. The item policy requires the exact original preimage and changes only quantity on hand and change actor/time. Item cost, valuation fields, location, unit and creation data remain exact. Complete closing evidence validates the original unchanged purchase, exact expected item and original full history plus one movement before commit; durable read-back must agree after commit.

No journal or AP operation runs in this physical command. Movement cost fields remain null. This does not establish valuation or ledger reconciliation, and the existing generic financial stock methods are not used for these company receipt commands.

## Original intent and uncertainty

An unchanged retry of the same intent reads its unique recorded movement and exact original postimage without another insert/update. Changed reason/quantity/proof/audit or item data cannot repair a recorded result. Later receipt/return work invalidates an older intent's exact postimage; retry refuses rather than clearing, overwriting or replaying it. Unknown commit outcome returns no confirmed result, but an unchanged original retry can reconcile an intact durable outcome. Lost movement, changed item or changed purchase after commit remains unconfirmed and is not automatically repaired.

## Focused verification

The actual canonical Data build passes with zero warnings/errors in 1.53 seconds. The expanded affected inventory/procurement/receiver/handoff/screen gate passes 676 tests in 1.0695 minutes. The overlapping source gate passes 566 tests in 11.1404 seconds. Both final gates reach terminal exit 0 without failed/skipped tests; composition warnings remain and whitespace checks pass. Scopes overlap and counts are not summed.

Forty native command checks use the existing Beep repository, common-column handler and fixture provider's actual held Serializable transaction. They cover partial/full receipt, partial/full return, unchanged retries, gross-capacity preservation, accumulated partial returns, invalid command boundaries, stale item/history/purchase, duplicate item and cross-item intent collision, corrupt movement refusal before item write, item corruption/write failure, closing purchase/binding/authority loss, rollback of both rows, unknown commit recovery, borrowed/insufficient isolation refusal, replacement retry/proof/audit/item refusal, post-commit lost/changed data and refusal to replay older intent after later movement. Receiver evidence, item cost/valuation fields and draft AP state are preserved. Returned result copies do not mutate stored rows.

## Remaining persona delivery

This is native command verification; company mutation endpoints, separate current app grants, typed client submissions, uncertain-intent workspace recovery and actionable warehouse/receipt screens are next. Expose these existing commands through ApiService and typed clients before adding screen actions. Show explicit quantities/units/location/source revisions, original-receipt choices and confirmed versus uncertain outcomes in the persona workflow and landing queues.

No live stock write, migration, grant provisioning, handwritten SQL or new business EF lane is executed. Real Beep provider locking/concurrency, column provisioning, full host/browser/OIDC, accessibility/RTL/responsive behavior and durable circuit recovery remain open. Financial posting requires its separate reviewed owner and authority. Wider company personas and multinational acceptance remain active; no phase closes.
