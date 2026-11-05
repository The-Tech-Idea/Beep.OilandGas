# Implementation 169 — Assigned source summary drill-downs

Date: 2026-10-06. Status: source-specific assigned-work summaries implemented and focused-tested. The full company/persona roadmap remains active.

## Persona landing behavior

The existing PersonaAssignedWorkPanel, already composed by accountant/procurement landing views and the general inbox, now offers all-source, invoice, purchase-order and field-work assignment summaries. Selecting a source resets work-type/attention filters and opens exactly its contributing authorized task records. Approval/review/data-entry/overdue/critical summaries then operate inside that selected source; their displayed counts use the same loaded records. Each task retains its encoded existing recorded process link, assignment role, sender/workspace, creation and due evidence.

These are assigned-task counts, not distinct invoices/orders or financial/receipt completion. Multiple tasks can refer to one source. No aging, matching, approval, posting, payment, currency conversion or stock KPI is inferred. The display names its selected source and distinguishes visible/selected counts from all loaded assignments. Existing reload/error/loading/empty handling remains in the parent. Source controls use Mud buttons with text labels and aria-pressed; wrapping follows the existing Mud layout/theme. No custom CSS/JS or local palette is added. Browser keyboard/RTL/responsive/accessibility interaction remains unverified.

## One state and evidence path

PersonaTaskInboxState extends the existing validated frozen UnifiedInbox snapshot. Company accounting AP_INVOICE/PURCHASE_ORDER summaries require both the explicit COMPANY_ACCOUNTING scope and matching entity type; FIELD records cannot be classified by matching entity text alone. SelectedCounts reuses the existing Data-owned InboxCounts contract. There is no new domain model, task query, dashboard database access or parallel source owner.

Summary selection changes local presentation only. It grants no role/permission and does not alter tasks. Current app-owned role/persona/company authority, native owned source/process/step admission and closing evidence checks remain in the existing API. Company/persona changes and failed refreshes clear the source selection and rows; late away-and-back responses cannot republish them. Exposed rows/counts are independent snapshots, so consumers cannot change the stored summaries.

## Focused verification

- Scripts/Test-PersonaTaskInboxState.ps1 with the actual Data assembly: **69 passed, zero failed/skipped, terminal exit 0, 1.4877 seconds**, zero compiler warnings. Log: %TEMP%/oilgas-source-summary-169-state.log. Fourteen new cases cover exact AP/PO/FIELD contributions, source/work-type intersections, invalid choices, snapshot mutation, failed reload, changed company/late response and selected-source rendering.
- Scripts/Test-PurchaseOrderOwnership.ps1 with the actual Data assembly and -IncludeWorkflowInbox: **672 passed, zero failed/skipped, terminal exit 0, 11.8118 seconds**. Log: %TEMP%/oilgas-source-summary-169-final.log. Two new cases create native company handoffs for accountant/AP and buyer/order, then consume their tasks through actual inbox HTTP/typed clients and source drill-down state for two companies. The buyer case renders the complete procurement landing parent, actual assigned-work component and source queue; the accountant case renders the complete general inbox parent. Foreign company process links are absent.

The scopes overlap and are not summed. The earlier 672-case checkpoint passed in 11.1108 seconds with one nullable test-boundary warning; its source-client boundary now refuses a missing response explicitly, and the final gate returns to the prior 315 dependency/source-precedence and nullable warnings. No warning is reported for the modified state/panel/test sources. Whitespace verification passes. Data contracts are unchanged; these gates use the actual clean Data build recorded in Implementation 168.

Native records/physical transactions and resolved account/current binding/diagnostics seams remain explicit fixtures. Existing company/persona/app-role stores use actual SQLite configuration code. The procurement source-client test boundary invokes the real API source owner through ApiClient; it does not certify the complete production AccountingServiceClient. Static parent rendering and state invocation do not prove browser clicks. The complete accounting dashboard parent, production host/DI/OIDC, live native provider/concurrency/durability, browser/RTL/accessibility and whole-solution verification remain open.

## Next delivery and remaining work

Continue from an assigned company accounting process to reviewed role-specific actions and authoritative recorded outcome reload. Reuse the actual company source/maker/assignment/revision owners and retain uncertain command intent. Financial preparer/reviewer evidence, receipt acceptance/concurrent consumption, approval/post/payment, durable recovery, full accounting dashboard composition, all other persona workflows and multinational/company requirements remain active. Beep MigrationManager remains the business schema owner; no handwritten SQL, new business EF lane, live migration/grant or phase closure.
