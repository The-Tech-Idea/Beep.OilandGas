# Implementation 109: Maintenance work-order detail and transition outcome

## Delivered path

The existing work-order detail screen requires standard ASP.NET authentication and loads through its existing typed API client. WorkOrderDetailState captures the selected company/account and confirmed current FIELD before reading the route identity. Missing, mismatched and late data cannot publish. Company changes clear detail, transition choices, checklist, costs, contractor and AFE presentation; newer route/reload generations discard older reads. Loading supplementary evidence publishes as one page result rather than leaving earlier data visible after a failed load. Reload and explicit errors are available.

The existing transition request is now owned once by TheTechIdea.Data, preserving its namespace. ExpectedFieldId and ExpectedState are required JSON members. The API transition endpoint requires FIELD WRITE, compares the submitted original FIELD and state against an actual order read, and checks the available destination before calling the existing transition owner. This is preflight validation, not an atomic compare-and-swap.

The page captures the original order identity, subtype, company/account, FIELD, state, destination and notes. It disables overlapping submissions, retains unknown outcomes and requires both the mutation response and an actual detail reload to match the original identity/FIELD/subtype and requested destination before confirmation. Company/route changes prevent an old command from updating a different displayed order. An unconfirmed attempt remains blocked for that actor/company/FIELD/order across company switches and ordinary reloads in this page session. A different company does not inherit that uncertainty. Errors use the existing failure reporting owner.

## Verification

Scripts/Test-WorkOrderListState.ps1: **51 passed**, zero failed/skipped, terminal exit 0, 1.2970 seconds. This includes 22 new actual detail-state/request cases plus 29 existing list/create cases. Log: %TEMP%/oilgas-workorder-detail-109.log. Tests cover malformed scope/identity, required wire members, unavailable readback, wrong response, concurrent route reads, company changes, original-context retry blocking, missing company, invalid destination and disposal. The actual canonical Data build passed with zero warnings/errors. Whitespace diff check passed.

The focused project compiles actual state/context and current WorkOrder DTO/request sources with actual Data and cached model dependencies; CS0436 suppresses old cached DTO collisions while current source wins. It does not prove the native API/controller/DI, full Razor page, physical provider, OIDC/role admission or browser behavior.

## Open gates and procurement finding

Persisted command identity/receipts, transaction ownership, provider atomic state/version checks, independent completion approval, inspection/cost/scheduling/AFE mutation boundaries and exact durable next-role handoff/read-back remain open. A matching state reload confirms observed state, not unique causation or an audit receipt. Page-session blocking ends when the page is destroyed. Read-only availability is not evidence of permission to execute every transition; native role/resource denial must be verified. No persona or phase is closed.

Procurement discovery found overlapping synchronous traditional purchase-order and asynchronous Accounting services, plus PURCHASE_ORDER/PO_LINE_ITEM/PO_RECEIPT records without a reviewed company/resource relationship path. No buyer dashboard was wired to unscoped/default-connection records. The existing procurement plan must reconcile those owners and establish authorized order/receipt/stock evidence before completing PO -> receipt -> AP handoff.

No handwritten SQL, schema, business EF lane, migration execution or grants were introduced. Business schema changes remain Beep MigrationManager-owned.
