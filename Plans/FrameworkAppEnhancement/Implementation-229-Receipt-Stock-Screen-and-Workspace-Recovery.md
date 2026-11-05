# Implementation 229 — Receipt stock screen and workspace recovery

## Actionable warehouse receipt workflow

Verified receiving inventory evidence now links to /warehouse/receipt-stock/{OrderId}/{ReceiptId}. The receipt-specific MudBlazor screen reads the existing typed stock API under the current authenticated selected account, standard app roles and both procurement/inventory read grants. It shows company/order/receipt/item, accepted quantity, gross receipts, returns, remaining receipt capacity, exact unit/warehouse/location, recorded quantity on hand and stock/purchase/inventory revisions with UTC retrieval time. Escaped source identifiers are decoded once from the original location through the canonical source-id helper.

No movement or quantity is selected by default. The operator explicitly chooses Receive or Return, enters physical quantity and a bounded single-line reason. Return additionally selects an original recorded physical receipt, displaying identity, quantity/unit, recorder and UTC recording time. Separate receipt/return grants govern save; both read grants alone show a read-only review. Handlers independently recheck current authenticated account and standard roles. Existing native/API validation enforces receipt capacity, original-return limits and recorded stock balance; returns do not replenish receipt capacity. The screen does not claim available stock, valuation or financial posting.

## Existing inventory workspace owner

InventoryWorkspaceState now owns copied stock review, original submission and confirmed result, busy/generation state and the original recovery route. Context/auth changes, clears and disposal invalidate displayed or late evidence. Before sending, the state freezes company/account/order/receipt/kind/quantity/original receipt/reason/review/binding/intent. Returned copies cannot mutate that retained evidence. Invalid choices/quantities do not create intent.

Complete command result validation is followed by a fresh full receipt inventory API read-back confirming the same original submission and source revision. Only then does the state publish updated item/movement detail and current stock capacity. Uncertain, malformed, changed or late results retain private unconfirmed intent. Retry ignores replacement form kind/quantity/original receipt/reason and requires the original company/account/receipt route. A competing stock review is blocked while busy or uncertain. Scoped retention does not survive circuit loss.

The warehouse landing page shows a busy/unconfirmed stock warning and original-intent recovery link. Returning to landing after a confirmed movement reads fresh native company inventory rows and derives its counts from that queue. Navigating to landing while uncertain can inspect recorded inventory but does not confirm the pending operation or discard its intent. Existing theming is reused; no custom CSS/JS or frontend database access is added.

## Focused verification

The actual canonical Data build passes with zero warnings/errors in 1.73 seconds. The expanded affected gate passes 729 tests in 1.3098 minutes. The final screen/workspace/warehouse gate passes 43 tests in 9.1714 seconds, including the added landing recovery banner journey. The overlapping source gate passes 566 tests in 8.8204 seconds. All final runs reach terminal exit 0 without failed/skipped tests; composition warnings remain and whitespace checks pass. Overlapping counts are not summed.

Twenty-six new checks comprise fifteen workspace cases and eleven continuous rendered-handler/actual typed-client/Kestrel/native fixture journeys. They cover receipt/return confirmation, unknown commit and failed read-back recovery with unchanged original intent and no additional writes, copied evidence, late review/result after company/account/clear/disposal changes, invalid action/quantity/original receipt, escaped identifiers, read-only review, missing movement grant, authenticated-account mismatch, no default action, invalid reason, authentication change after uncertainty and during native writes. Screen journeys also prove fresh warehouse landing rows/counts after confirmed movement and retained recovery after navigating to landing while uncertain. Existing procurement/receiving/link/handoff/inventory checks pass.

## Remaining delivery and acceptance

Next verify multi-persona procurement/warehouse/inbox handoffs and landing outcomes across stock receipts, partial returns and subsequent inventory-link cycles, preserving original source evidence and explicit physical/financial boundaries. Continue the wider company persona roadmap and multinational requirements. Cross-persona concurrent operations and any durable reconciliation after intervening source changes remain acceptance work; scoped retention is not a durable ledger/UI recovery implementation.

No live stock mutation, Beep migration, grant assignment, deployment, handwritten SQL or new business EF lane is executed. Real provider locking/concurrency and column provisioning, full host/browser/OIDC, keyboard/screen-reader/RTL/responsive behavior and durable circuit recovery remain open. Financial posting requires a separate reviewed owner and authority. No phase closes.
