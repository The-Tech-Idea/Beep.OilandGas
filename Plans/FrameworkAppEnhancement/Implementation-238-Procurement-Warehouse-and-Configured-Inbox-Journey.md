# Implementation 238 — Procurement, warehouse and configured inbox journey

## Persona screens and handoffs

This slice verifies the existing company workflow owners together, without treating a task completion as a stock or financial posting. The selected company and authoritative app roles/grants govern each API read and command. Company roles remain configuration-driven; persona labels do not confer authority.

| Persona | Existing landing and screen | Data and decision | Handoff |
| --- | --- | --- | --- |
| Procurement maker | `/procurement`, `/procurement/orders/{Id}` | Owned PO, lines, original maker audit and current revision | Receiver sees the order and independently records inspection |
| Receiver | PO receipt section on the procurement screen | Received quantity, accepted/rejected quantities, unit and receiver proof | Verified accepted receipt opens `/warehouse/receipt-stock/{OrderId}/{ReceiptId}` |
| Warehouse operator | `/warehouse`, `/warehouse/items/{Id}`, receipt stock screen | Explicit receipt/return quantity, original receipt movement, unit/location, stock capacity and current native source | Original-account recorded evidence is discoverable and opens `/warehouse/stock-intents/{IntentId}` |
| Link preparer and independent reviewer | Inventory-link panels on the procurement screen | Alternate target preparation and explicit approve/reject proof | Explicit cycle action archives original proofs before a later reviewed cycle |
| Configured accountant | `/tasks/inbox`, `/accounting/processes/{Id}` | Routed company task, original PO maker, workflow starter and current source revision | Independent step review/completion routes the next task; AP matching and financial approval remain separate |

Routes are verified from current source. This slice does not add a new landing page or duplicate workbench. Existing rendered screen gates remain separate evidence from this combined service/API journey.

## Continuous focused journey

The new journey fixture uses separate actors for original PO maker, receiver, warehouse operator, link preparer, link reviewer, cycle owner, workflow starter and accountant. Existing Beep native owners record acceptance of eight units and rejection of two, then a four-unit physical stock receipt and two one-unit returns against the same original receipt movement. Gross received capacity remains four and remaining receipt capacity remains four; returns do not reopen previously consumed receiving capacity. Quantity on hand moves from ten to twelve.

Two alternate-target proposals are independently rejected and each completed review is explicitly reopened into an archived cycle. Rejection retains the physically received item's reference. Native history preserves both original preparer/reviewer proofs and the cycle owner. The original receipt's saved recovery remains verifiable after later returns and PO cycles, while its original-effect comparison is false against the changed current source.

The typed process client and scoped handoff owner then call actual local Kestrel endpoints to discover and start a configured company process. The independent accountant finds its assigned task through the existing unified inbox API and reviews the current PO revision. Original maker remains `po-preparer`; workflow starter remains distinct. Successful completion closes the assigned task and routes one next task.

Three additional cases mutate the purchase source, alter archived history, or revoke inventory read authority after accountant review. Completion refuses with conflict or forbidden status before any workflow completion writes. All cases retain byte-equivalent receiver evidence, item state, physical movements and saved recovery records through the workflow operation. The invoice remains draft and no AP receipt allocation is created.

## Verification and limits

The initial four continuous journey cases pass. The adjacent receiver, stock-command, inventory-link cycle and prior handoff gate passes 146 tests with zero failures/skips in 29 seconds, including those four cases. The final four journey cases also pass with explicit retained actor-proof assertions, zero failures/skips, in 21 seconds. Counts overlap and are not summed. Whitespace checks pass. Existing focused composition warnings remain.

The physical and link portions execute actual service owners against the controlled Beep datasource/transaction fixture. Alternate-target read callbacks return canonical owned source reviews; they are not proof of a real provider target lookup. The configured process, role inbox and review/completion portion executes the actual typed client and local API controllers. This is not a browser/OIDC, real provider concurrency, live migration or financial posting acceptance test.

## Next delivery

Continue the company persona screen matrix with receiver/warehouse/accountant task navigation and explicit available actions on landing pages. Prioritize the remaining approved alternate-target journey after prior physical receipts, and verify how current receipt stock and original-account history remain accessible when a line changes destination. Extend the existing owners only after the governing receipt/item evidence can be proved; no inferred movement, quantity repair or replay.

Multinational company context, units, currency/time-zone presentation, localization/RTL and denied/empty/unavailable states remain part of each persona's acceptance. Real provider paging efficiency/order/concurrency, browser reconnect/OIDC, accessibility/responsive checks and live Beep provisioning remain open. No phase closes. No SQL, new business EF lane, frontend database access, live mutation, migration, grant assignment, deployment or commit/push is introduced.
