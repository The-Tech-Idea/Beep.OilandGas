# Implementation 224 — Procurement cycle history and reopening screen

## Actionable company procurement workflow

The existing ProcurementWorkbench now selects an owned recorded line, reads its complete cycle history and explicitly reviews resolved reopening or pending supersession. Neither action is selected by default. Saving requires a bounded single-line reason and the already reviewed action; it archives original evidence and reopens the line through the existing typed company API. The current inventory reference is preserved for a later separate preparation and independent decision.

The MudBlazor history panel shows the snapshot's UTC retrieval time, order/history revisions, archived cycle actor/time/reason, original preparer/time/target/reason, recorded independent reviewer/outcome/time/reason when present, and archived reference. Pending preparations explicitly have no independent decision. Empty retained history is distinguished from unconfirmed history. Archived evidence is separate from the current reference and does not imply stock movement or financial posting.

Handlers require the currently authenticated account to match the selected company account, a standard assigned app role and both Procurement.View/Inventory.View. Review/save additionally require Procurement.ReopenInventoryLink. Users with only both read grants can inspect history. Authentication/company changes clear display/form choices, while private uncertain cycle intent remains recoverable in the same scoped workspace. Existing theming/MudBlazor controls are reused; no custom CSS/JS, SQL, frontend database access or new EF business path is introduced.

## Existing scoped state and exact confirmation

PurchaseOrderWorkspaceState owns copied history/review/submission/result, generation/busy state and the original recovery route. Reviews must agree with the complete displayed source revision and original company/account/line/action. Returned copies cannot mutate retained evidence. Context, clear/disposal and generation changes discard late evidence.

Save freezes the original action, reason, company/account, full review and canonical intent before sending. The result must confirm that exact submission. A fresh complete history API read-back must independently confirm the same submission and agree with both returned order/history revisions. Only then are order lines/receipts and source provenance refreshed, with confirmation displayed. Missing/changed/late results retain uncertain intent; retry ignores replacement form action/reason and requires the original company/account/order. Circuit loss is not covered by this scoped retention.

Preparation, independent decision, receiver and inventory-link operations now guard against busy or uncertain cycle work. Cycle review clears competing displayed reviews and invalidates earlier receiver-review generation. Cycle work likewise refuses competing busy/uncertain operations. History is a frozen source snapshot; its as-of time and revisions remain visible.

## Focused verification

The final affected gate passes 525 checks in 46.7213 seconds. The overlapping source gate passes 566 checks in 12.9562 seconds. Both final runs reach terminal exit 0 without failed/skipped tests; composition warnings remain and whitespace checks pass. Canonical Data is unchanged from implementation 223's clean actual build and is used by both gates.

Twenty-four new checks include 15 scoped-state cases for resolved/pending save, uncertain retry, invalid returned/read-back evidence, company/clear/disposal invalidation, bounded reason, competing action refusal, frozen copies and late/invalid review. Nine complete HtmlRenderer page-handler journeys use the actual typed client, Kestrel company API, native PO services, common-column handling and fixture provider transactions. They cover resolved/pending cycles, missing grant/account, read-only history, no default action, invalid reason, unknown commit with unchanged original-intent recovery, and authentication invalidation. The existing preparation/reviewer/receiver screen checks also pass.

This proves a continuous rendered-handler/client/HTTP/native fixture journey. It does not prove a real browser, live provider locking, real schema/grant provisioning, OIDC sessions, keyboard/screen-reader/RTL/responsive behavior or durable circuit reconnect. No schema change or live migration is executed; canonical history/cursor Beep provisioning remains open. No phase closes.

## Next persona delivery

Verify procurement/warehouse/inbox handoffs and landing outcomes across subsequent reviewed cycles, preserving explicit source revision and physical acceptance boundaries. Then deliver the warehouse stock/return workflow through existing canonical inventory owners and API/screens, with explicit quantity/unit/location/provenance and independent authority. Unknown availability must remain explicit; no stock or financial effect may be inferred from a reference approval or cycle reopening. Wider company personas, multinational acceptance and live provider/browser/provisioning work remain active.
