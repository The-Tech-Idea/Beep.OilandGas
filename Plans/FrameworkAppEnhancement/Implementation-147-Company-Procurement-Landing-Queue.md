# Implementation 147: Company procurement landing queue

## Buyer landing and accountant source handoff

The existing /procurement landing now loads recorded active orders for the selected company without requiring a supplier ID first. Its configured-persona assigned-work inbox remains. An optional supplier input restricts the query to the exact recorded business-associate identity. The shared order panel shows returned/visible counts, actual recorded-status choices, supplier, order/expected delivery dates and recorded amount/currency. Missing status/date/currency remains explicit. Selecting a status filters only current returned records; All recorded statuses includes unavailable status rows.

Each encoded order link opens the same /procurement/orders/{Id} detail and line/receipt evidence used by the accountant's AP-line purchase-source review. The queue does not interpret recorded status as approval, receipt completion, stock availability or invoice matching, and does not aggregate currencies or quantities. This is a source queue rather than a certified actionable exception or liability dashboard.

## Existing API-first owners

PurchaseOrderService.GetCompanyPOsAsync owns the active list query. Existing GetVendorPOsAsync delegates to it with an exact vendor predicate; no parallel provider query remains. Mandatory native filters are the original tenant, owning company and ACTIVE_IND=Y. Current schema validation and repository failure behavior remain in place. Data-owned PurchaseOrderOwnership.RequireRows validates exact active ownership and unique identities; vendor validation extends that same list proof.

The existing purchase-order controller adds the authenticated GET root and retains fresh standard app-owned company roles, Procurement.View and original PRODUCTION binding checks before and after the read. Empty supplied vendor assertions refuse. The existing typed accounting client captures the original selection, encodes an optional vendor and validates returned rows. No business database connection is added to Web. No role/permission assignment, new entity, schema, SQL or business EF lane is introduced.

PurchaseOrderWorkspaceState.LoadCompanyAsync shares its frozen/sorted projection with the supplier reader, records the supplier restriction and allows only current recorded status values. Reload/failure/company switches clear source rows, filter and retrieval time; late company away-and-back responses cannot publish. ProcurementWorkbench invokes the new company list at the root while retaining existing detail route and authorization behavior. The same state is still reused by the AP linked-source panel.

## Focused verification

Current complete Data sources build with zero warnings/errors in 1.76 seconds. The final affected actual-source gate passes **231 tests**, zero failed/skipped, terminal exit 0, in 1.4667 seconds. Log: `%TEMP%/oilgas-procurement-queue-147.log`. This includes the previous 221 checks and ten new cases; overlapping totals are not summed. The existing 175 dependency/source-precedence warnings remain.

Six native service cases validate exact owner/activity filters, reject foreign/inactive/duplicate/null evidence and reject an empty supplier assertion before opening a provider. Three state cases verify frozen recorded statuses/encoded detail links, late company responses and failed reload clearing. One actual Razor render case verifies company counts, unrestricted supplier label, status selector, amount/currency column, existing order detail link and explicit status limits. The gate compiles actual current AP/PO services, Data ownership/reviews, repository/interface/transaction helpers, states and shared panels.

Complete controller/client/parent/DI-host and live app-role bridge remain source-only. HTTP/OIDC, browser/keyboard/RTL, native providers/physical schema and consistency are unverified. The provider-returned list is materialized; server paging, provider completeness and large-company performance remain acceptance work. No live migration, command or deployment is executed.

## Remaining workflows

The existing process-start reader currently supports journals and work orders only. AP invoice initiation still lacks its verified source-specific resource/duty rules; this slice does not enable generic invoice initiation. Complete full buyer/accountant landing-to-detail HTTP/browser/provider verification, approved initiation/approval duties, purchase creation/approval/receiving and inventory evidence, canonical receipt-to-invoice allocation/matching, financial posting/payment and durable recovery. Beep schema application/reconciliation, all other personas and multinational requirements remain open. No phase closes.
