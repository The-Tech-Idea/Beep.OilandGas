# Implementation 260 — Approved commercial term date and currency review

## Commercial persona screen

The existing CommercialTermsWorkbench now displays applicability of independently approved terms to the exact recorded revenue transaction date and currency. It uses the current typed API review and validated native commercial history; the canonical projection lives in TheTechIdea.Data and reuses SalesContractTermsReadReview. Preparation, approval, rejection, source ownership and independent reviewer validation remain with their existing owners.

The panel preserves the recorded date and currency and displays source and term revisions. It distinguishes missing preparation, pending independent approval, rejection, required timezone interpretation, dates outside the effective interval, missing currency, currency mismatch and approved stored date/currency. It explains that delivery approval, measured quantity and units, invoicing, recognition policy and ledger posting require separate reviewed evidence.

## Exact evidence rules

Effective boundaries are inclusive calendar dates; an explicit open end remains open. UTC or local transaction timestamps require company calendar/timezone interpretation before matching stored effective dates. The projection does not convert them silently. Currency matching uses the exact recorded value and approved terms; missing currency is not replaced, and a mismatch does not imply foreign-exchange conversion. The original API/native review is revalidated before deriving the result. The digest binds status, recorded date/currency and original source/term revisions; Require rederives and compares the complete result.

This is a read-only prerequisite review. An approved date/currency result does not assert workflow completion, measured delivery, quantity/unit compatibility, financial calculation, invoice readiness, recognition policy or ledger posting. No endpoint shape, persistence, SQL, business EF migration, CSS, JavaScript or authorization mechanism is added. Business schema work continues through Beep Migration.

## Existing owner audit

The existing Accounting InvoiceService accepts request amounts and can default missing currency to USD. GaapRevenueRecognitionService accepts an amount and creates cost/basis postings with legacy defaults. Neither path alone proves a measured-delivery and approved-commercial-term source chain. ProductionAccounting RevenueService exposes the allocation owner and run-ticket lookup; DeliveryInformation is a legacy DTO with defaults. The older AR_INVOICE model is not promoted into a competing canonical owner. Delivery/allocation/run-ticket, customer, quantity/unit, approved price, tax/currency policy and financial source revisions need explicit review before connecting the current company workflow to financial writes.

## Focused verification

Canonical Data builds with zero warnings/errors. Fifteen new cases cover inclusive boundaries, open end, unprepared/pending/rejected terms, earlier/later dates, unavailable/mismatched currency, UTC/local interpretation, changed source/native evidence and tampered derived reviews. They render the actual current commercial component and verify that reading applicability invokes no preparation or decision mutation.

The first narrow compile caught a nested Razor code-block error, which was corrected. The next narrow run passed thirteen cases and failed two timezone fixtures: assigning identical clock ticks with a different DateTimeKind did not replace the existing model property. The corrected fixture initializes the nullable property before assigning the timestamp, matching native materialization of an initially empty value. The existing model's same-ticks setter behavior is not changed or claimed fixed.

The final affected source/Razor/local HTTP/native/migration-metadata gate passes 462 tests, zero failures/skips, in one minute twenty-eight seconds, terminal exit 0. Reproduce using Scripts/Test-PurchaseOrderOwnership.ps1 with the canonical Data DLL, IncludeWorkflowInbox and FieldRevenueSourceTests|CompanyProcessStart|CompanyStep|PersonaInbox|CommercialIntentMigration|ModuleSchemaVerification. Logs: oilgas-260-data.log, oilgas-260-applicability.log, oilgas-260-corrected-applicability.log and oilgas-260-final-source.log. Earlier failed runs are diagnostic evidence, not acceptance; focused dependency/source warnings remain.

## Remaining delivery

Connect approved measured delivery, effective terms and explicit quantity/unit/price evidence to invoice/receivable/recognition and ledger read-back through existing owners. Continue royalty/partner/project-cost queues, wider thirteen-persona workflows/landing/dashboard and multinational acceptance. Physical database/provider isolation, paging/order/concurrency, browser/OIDC/circuit reconnect, accessibility/RTL and live Beep provisioning remain open. No phase or overall goal closes. Commit and push remain authorized after the complete goal and plan pass the completion audit.
