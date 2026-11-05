# Implementation 155: Company-owned receipt allocations

## Company isolation in the accountant journey

Receipt allocations now carry explicit OWNER_TENANT_ID and OWNER_COMPANY_BA_ID in the existing Data-owned AP_RECEIPT_ALLOCATION. Inferring allocation ownership only through local invoice/receipt references allowed otherwise identical identities in different companies to resolve to the wrong local source. The existing invoice-line allocation view and proposal contributor view now require both allocation ownership and exact owned invoice/PO/receipt relationships.

The two owner columns are nullable, bounded to 128 characters and use the existing entity change notifications. Nullable schema evolution allows historical rows to remain explicitly unresolved; application validation never treats missing ownership as current-company ownership. No automatic assignment, inferred backfill, new entity/store or legacy query lane is introduced.

## Existing API/native and Data owners

APInvoiceService.GetReceiptAllocationsForLineAsync adds original tenant/company predicates to its exact invoice/line query. GetReceiptConsumptionAsync adds the same predicates to the exact receipt query while retaining all owned invoice lines and active/inactive history. Both check returned allocation ownership independently, including provider responses that ignore predicates. Foreign or unowned contributor rows refuse before reading their referenced invoices. An owned AP header with identical local IDs cannot establish ownership for a foreign allocation.

The existing fresh schema check requires both owner columns before native allocation queries. Missing columns refuse with the existing fixed API Conflict and PRODUCTION Beep migration guidance. APReceiptAllocationReview.Require validates ownership in addition to invoice-line/PO/receipt membership, and its content digest includes both owner values. Proposal and cross-invoice contributor reviews inherit that same validation rather than adding a second authorization or data model owner.

The existing AP pages, source panel, typed client and API endpoint owners remain in use. Current app standard roles, Accounting.View, Procurement.View and preparation permission remain independently required at their existing boundaries. Changing company or encountering a failed review still clears displayed allocation/proposal results through existing state guards. No grants or authorization authority move to IdentityServer.

## Beep migration and historical reconciliation

The existing PRODUCTION manifest already declares typeof(AP_RECEIPT_ALLOCATION), so its updated canonical Data metadata supplies the new columns to Beep MigrationManager planning. No handwritten SQL, business EF migration or direct client datasource is added. Fresh ClassCreator metadata confirms bounded nullable owner columns and the existing single nonnullable allocation key.

No live migration or backfill has been executed. Review the Beep migration plan/preflight and authoritative historical ownership reconciliation before deployment or enabling persistent commands. Rows with missing ownership are excluded from company-qualified reads and cannot contribute to reviewed consumption. Empty returned owned rows do not establish that unresolved historical allocations are reconciled or that quantity is available. Actual provider constraints/identity uniqueness and concurrency still require verification.

## Focused verification

Current complete Data sources build with zero warnings/errors in 2.04 seconds. The final actual-source gate passes **419 tests**, zero failed/skipped, terminal exit 0, in 5.1903 seconds. Log: `%TEMP%/oilgas-allocation-ownership-155.log`. The 175 existing dependency/source-precedence warnings remain. Whitespace verification passes.

Twenty-one new checks extend the previous 398-case gate: six Data checks for missing/foreign owners and changed frozen ownership; eight native service cases for owner refusal on both read paths and missing owner schema; seven complete-controller loopback HTTP cases for foreign/unowned rows with otherwise identical references and two companies with colliding allocation/source IDs. Existing tests now assert exact owner query predicates and populate explicit recorded fixture ownership. The metadata test checks both nullable owner columns. The gate includes 49 complete-controller HTTP cases; scopes overlap across runs and are not summed.

Actual Data/contracts, AP/PO controllers, native services/repository, state and shared panels compile and run in the affected gate. Full client/parent/production role bridge/context adapter/host, browser/RTL/accessibility, migration planning/application, actual providers and performance remain unverified. No allocation record, grant, migration, deployment or financial write was performed.

## Next controlled handoff

Implement accepted receipt and compatible invoice/PO units/currency/tolerance evidence, provider-backed concurrent consumption and explicit durable draft/submission identity before exposing a persistent allocation action. Reuse existing Beep transaction/readback patterns, distinguish recorded/not-recorded/uncertain outcomes and provide reconciliation. Independent reviewer matching/approval/post/payment and buyer/receiver/stock outcomes remain open. Company ownership is now explicit in this read workflow; no persona journey or roadmap phase is closed.
