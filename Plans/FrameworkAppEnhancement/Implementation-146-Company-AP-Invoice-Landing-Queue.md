# Implementation 146: Company AP invoice landing queue

## Accountant daily landing

The existing /accounting/payables landing loads recorded active invoices for the selected company without requiring the accountant to know a supplier identity first. Its existing assigned-work inbox remains in place. The supplier input optionally restricts the source query to one exact recorded business-associate ID; clearing the input and reloading restores the company query. The existing encoded invoice links open the same invoice detail and owned purchase-source workflow.

The same source panel shows the returned-record count and visible-record count, plus a selector containing actual nonblank recorded statuses. All statuses includes rows with unavailable status; selecting a recorded value displays exactly those current returned rows. It does not infer unmatched, approved, unpaid or overdue work from incomplete evidence. Amounts retain recorded currency and missing values; no mixed-currency liability total is calculated. The panel identifies an unrestricted supplier query as All suppliers. Counts are of returned records, not certified enterprise totals.

## Existing owners and boundaries

APInvoiceService.GetCompanyBillsAsync is the canonical active-list query. Existing GetVendorBillsAsync delegates to it with the required vendor restriction rather than retaining a parallel query. Native predicates retain exact OWNER_TENANT_ID, OWNER_COMPANY_BA_ID and ACTIVE_IND=Y; an optional vendor predicate is exact. Current required schema and repository failure handling remain in place.

The existing Data APInvoiceOwnership owns shared active-list validation. RequireRows rejects null, foreign, inactive and duplicate invoice identities; RequireVendorRows additionally validates exact supplier membership. No new domain entity, schema, SQL or business EF path is added.

The existing AP controller exposes the company list on its authenticated GET root and independently checks current app-owned company roles, Accounting.View and the original PRODUCTION binding before and after the read. Empty supplied vendor assertions are invalid; an omitted vendor selects the company query. The typed accounting client captures original company selection, encodes an optional vendor and validates the returned source rows. These controller/client changes remain source-only.

APInvoiceWorkspaceState.LoadCompanyAsync reuses the same list projection for company and vendor readers, freezes/sorts current records and retains the applied supplier restriction. Reload/failure/company change clear rows, status filter and source time. Late company away-and-back responses cannot publish. The status selector accepts only the current recorded status catalog. PayablesWorkbench now invokes this company reader on the root and preserves existing detail authentication and source navigation.

## Focused verification

Current complete Data sources build with zero warnings/errors in 1.78 seconds. The final affected actual-source gate passes **221 tests**, zero failed/skipped, terminal exit 0, in 1.6218 seconds. Log: `%TEMP%/oilgas-ap-company-queue-146.log`. It includes the previous 211 cases plus ten new cases; overlapping totals are not summed. The existing 175 dependency/source-precedence warnings remain.

Six new native service cases cover the exact original-owner/activity query across multiple suppliers, four foreign/inactive/duplicate/null refusals and an empty supplier assertion denied before provider access. Four new state/render cases cover frozen recorded status filters and nullable amounts/encoded links; stale company responses; failed reload clearing; and actual queue rendering with counts, status, unrestricted supplier label and detail link. Prior vendor-query checks now exercise the same canonical company-list owner.

Actual current AP/PO services, Data contracts, generic repository/interface/transaction helpers, UI states and source panels compile in the gate. Full controller/typed client/parent page, production auth/DI/HTTP, browser/keyboard/RTL, real providers and physical schema remain unverified. The missing diagnostics dependency still prevents the full HTTP-client gate; no replacement production project or diagnostics implementation is created. This source query materializes the provider-returned list; server paging, provider completeness and large-company performance require acceptance before claiming enterprise-scale delivery.

## Remaining journey

Verify the full landing-to-detail client/HTTP/browser path, current role bridge and provider behavior. Apply/reconcile the AP-line schema through reviewed Beep MigrationManager. Complete canonical receipt allocations, matching cardinality/discrepancy review, independently configured duties, invoice creation/approval/posting/payment and durable outcomes/recovery. Currency-bound aging and exact liability drill-downs remain open, as do every other persona and multinational requirement. No phase closes.
