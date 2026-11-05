# Implementation 228 — Receipt stock and return API and client

## Current company authorization and physical commands

The existing traditional inventory controller adds POST company/purchase-orders/{orderId}/receipts/{receiptId}/stock-movement. It invokes the existing native atomic inventory owner with the canonical frozen stock submission. Read-only stock review remains available under Procurement.View and Inventory.View without mutation authority.

Standard authenticated assigned app roles and both source grants are mandatory for mutation. Explicit Receive additionally requires Inventory.RecordReceiptStock; explicit Return requires Inventory.RecordSupplierReturn. These separate constants do not provision any live grants. Current authority is sourced through the existing app company context; IdentityServer remains authentication-only. A return grant cannot authorize receipt, and a receipt grant cannot authorize return.

The API freezes the complete submission and checks original company/account/order/receipt route and binding. Its native authority callback repeatedly compares the complete original company/actor/binding/role/permission tuple, including at repository writes, transaction closing and durable confirmation. Changed roles/grants, lost source authority or changed binding refuses confirmation. Unknown commit and invalid/changed source or proof produce an unconfirmed conflict response; authority loss produces forbidden. After-commit authority loss may leave a durable movement, so the original intent must be retained for reconciliation.

## Existing typed client

IAccountingServiceClient and AccountingServiceClient now submit physical stock movements through that company endpoint. The client copies the complete canonical submission before sending, validates original selected company/account, escapes exact order/receipt identifiers and requires the exact complete original native postimage. Changed item/history, malformed or missing movement evidence, null results and late company/account changes refuse confirmation. Copied responses and caller-owned source mutations cannot replace the retained submission.

The client does not apply stock or financial data locally. Source models and command/result validation remain in canonical Data; frontend database access, parallel business DTOs, handwritten SQL and a new business EF path are not introduced.

## Focused verification

The final expanded affected gate passes 703 tests in 1.3763 minutes. The overlapping source gate passes 566 tests in 22.4496 seconds. Both reach terminal exit 0 without failed/skipped tests; composition warnings remain and whitespace checks pass. Canonical Data is unchanged from implementation 227's actual clean build and is used by both gates. These scopes overlap and counts are not summed.

Twenty-seven new checks comprise nineteen actual typed-client/Kestrel/native receipt/return journeys and eight client response-boundary cases. They cover both movement kinds, escaped resource identities, exact retry after unknown commit, read-only review without mutation grants, missing/wrong movement/source grants, standard-role mismatch, current account/binding/route/source changes, anonymous refusal, complete role/grant-set changes, revocation after insert with rollback, revocation after commit with original-intent recovery, recomputed but altered item/history source responses, malformed/missing movement proof, null results, caller mutation and late company/account changes. Existing native rollback/capacity/return/recovery and procurement/warehouse/receiving screen checks pass.

## Next persona delivery and acceptance

Next add stock movement workspace recovery and actionable warehouse/receipt screens using these existing typed operations. Show explicit receipt and return selection, original physical receipt choices, accepted/gross/returned/remaining quantities, units/location/revisions and confirmed versus uncertain outcomes. Retain original reason/kind/quantity/intent across uncertain responses and company/account changes; reconcile original intent before permitting competing actions. Update warehouse landing outcomes from fresh confirmed source data.

Native mutation API/client is verified in focused fixtures; the stock screen is not delivered yet. Live Beep column/grant provisioning, real provider locking/concurrency, full host/browser/OIDC, accessibility/RTL/responsive behavior and durable circuit recovery remain open. No live migration, grant assignment, deployment or stock write is executed. Financial posting still requires its separate reviewed owner and authority. Wider personas and multinational acceptance remain active; no phase closes.
