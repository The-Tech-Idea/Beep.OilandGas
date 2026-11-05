# Implementation 130: Recorded maintenance landing queue

## Daily planning screen

The existing /ppdm39/workorder landing previously placed canonical headers into a workflow-unavailable column beside seven empty workflow columns. The canonical WORK_ORDER read does not establish these lifecycle states. The page now renders a recorded-order queue using its existing WorkOrderListState and typed API read. The existing PersonaTaskInbox remains above it for assigned process work and actual workflow detail/action links.

WorkOrderQueuePanel shows the loaded FIELD/header count, actual order number and identity, recorded type, request/due dates, separate recorded component FIELD/well/equipment references and an encoded link to order detail. Missing dates/numbers/references remain unavailable. The MudBlazor table includes responsive data labels, pagination and a local search over recorded number/identity/type/component references. WorkOrderListState.Matches requires the row to belong to the confirmed current response; company invalidation prevents old rows matching even an empty query. Search ignores case and surrounding whitespace and does not request or invent new source data. Component search resets with the existing review version.

Counts explicitly cover the loaded FIELD/subtype response, not the entire company population. Dates use the current display culture without timezone conversion or inferred overdue/readiness status. The screen directs workflow actions to assigned process work and keeps scheduling, permit and acceptance evidence separate. Seven state columns and their obsolete card/type helpers were removed; no compatibility board remains. The now-unused ApiClient injection was removed from this page. Creation, existing subtype filtering, FIELD selection and page-lifetime unknown-outcome handling retain their current owners.

## Focused verification

Scripts/Test-WorkOrderListState.ps1: **332 passed**, zero failed/skipped, terminal exit 0, 1.2878 seconds. Seven new cases cover case-insensitive/trimmed recorded-number/identity/type/component/well/equipment search, refusal of unloaded rows, absent matches and original-company invalidation. Existing 325 service/state/SDK/inspection cases remain green. Log: `%TEMP%/oilgas-order-queue-state-130.log`.

Scripts/Test-WorkOrderInspectionRendering.ps1: **15 passed**, zero failed/skipped, terminal exit 0, 1.3787 seconds. Seven new actual queue component cases cover recorded header/relationships/encoded detail route, missing dates, empty confirmed response, unconfirmed/company-cleared state, encoded source text and en-US/fr-FR display dates without conversion. Existing eight inspection component cases remain green. This gate links the actual queue/state and uses actual MudBlazor with a test-only popover host and explicit no-browser JS boundary. Log: `%TEMP%/oilgas-order-queue-render-130.log`.

No Data/schema changes; the actual clean Implementation 129 Data assembly is reused. Whitespace check passes. These source/state/static-component checks do not compile/execute the complete parent landing page and host/DI, exercise browser search/pagination events, call live HTTP/OIDC, prove role assignments or run native Beep provider transactions. Date formatting evidence is scoped to two display cultures and does not complete localization/calendar policy.

## Remaining execution

Verify the parent maintenance landing and assigned inbox through actual typed transport/HTTP/browser, and implement approved duty roles and actual planning/material/permit/inspection/supervisor acceptance/return-to-service/accounting handoffs. Native inspection migration/provider transactions, authoritative process/domain state, durable receipts/corrections, timezone/calendar/localization and complete company-persona/multinational coverage remain open. No SQL, new business EF lane, live writes/deployment or persona/phase/goal closure.
