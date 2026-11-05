# Implementation 209 — Receiver and warehouse receipt evidence queues

## Delivered persona screen

The existing procurement order detail now provides receiving evidence queues for buyers, receivers and warehouse reviewers. All receipts, active receipts needing a verified decision, active receipts with accepted goods, active receipts with rejected goods, and inactive/unconfirmed active indicators filter the same recorded receipt history. Counts cover the current reviewed order only. Mixed accepted/rejected receipts appear in both queues; quantities are never summed across units. Unknown source evidence has unavailable counts; a confirmed empty source has zero counts.

Each contributing row retains the original receipt and PO line identities, recorded quantity/unit, receiver decision quantities/reason/actor/time, and creation/change provenance. A pending active receipt offers Review receiver decision, invoking the existing workbench review and authority checks. The action is disabled during receiving operations, unsettled original intents or missing receiver permissions. Recorded accepted/rejected quantities without a canonical decision proof remain unconfirmed rather than entering accepted/rejected queues.

The existing scoped PurchaseOrderWorkspaceState owns filters and read-only projections. Its canonical source verification, frozen copies, company/account generation fences and authoritative receiver read-back remain in use. Company changes, failed reloads and clear/disposal remove counts, rows and filters. The shared source panel keeps its current empty/error evidence behavior. Existing API-first data access and app-owned standard roles remain authoritative. No new domain contract, data access path, role, endpoint, schema, SQL or business EF lane was added. Any later business schema work uses Beep MigrationManager.

## Focused verification

The first focused source gate passes 522 checks in 6.9652 seconds, with 330 composition warnings. It includes three added checks for exact proof-backed membership, mixed acceptance/rejection, frozen provenance, empty versus unavailable evidence, invalid filters, company/reset behavior and rendered scope/gap wording. After adding the receipt-row action, the final overlapping procurement gate passes 106 checks, zero failed/skipped, with a reported test duration of 4 seconds and 378 composition warnings. It compiles the complete procurement workbench and covers purchase-order state/rendering, native receiver decisions, HTTP receiving and receiver-to-accountant handoff. Both command handles reached terminal exit 0. Whitespace checks pass. Existing actual Data assembly is reused unchanged.

These are focused source/renderer/native/HTTP fixture checks. The new row-action wiring compiles; real browser clicks, keyboard/screen-reader behavior, OIDC, real provider transactions, migration provisioning, company role grants and circuit lifetime recovery remain unverified.

## Remaining stock and return journey

Receipt acceptance is evidence for warehouse review, not confirmation of stock availability or supplier return. Before stock or return commands, identify the existing material/inventory owner and establish reviewed warehouse/location, item/unit, source receipt identity, movement quantity, reservation/issue linkage and valuation provenance. Add current company/resource authorization, independent adjustment/return review where required, exact reviewed revisions and original-intent reconciliation through that owner. Extend the current procurement/task/landing drill-downs with authoritative movement records. A rejected receipt must not automatically create a supplier return or a stock movement.

No persona or phase closes. Cross-order receipt exception landing counts need an authorized complete API population; the current detail counts must not be presented as company-wide KPIs. Wider persona screens/dashboards, multinational currency/unit/time-zone acceptance and financial matching/posting/payment evidence remain open under the existing roadmap.
