# Field royalty reporting and bound preview — 2026-09-21

## Completed implementation

The field royalty report previously compared a field ID with PROPERTY_OR_LEASE_ID, treated pool ID as royalty-interest ID, dropped the requested connection, and used an end-date comparison that omitted most of the final day. Reports now follow persisted provenance: RUN_TICKET.FIELD_ID -> ALLOCATION_RESULT.ALLOCATION_REQUEST_ID -> ALLOCATION_DETAIL.ALLOCATION_RESULT_ID -> ROYALTY_CALCULATION.ALLOCATION_DETAIL_ID.

- Require an explicit field and a local authenticated actor. Check field access before querying records. Read-only actions live in RoyaltyReportsController at the existing GET /api/accounting/royalty/calculations and POST /api/accounting/royalty/preview routes; no duplicate routes remain in the mutation controller.
- Resolve the PRODUCTION module database on the server for both reporting and preview. Remove client-selectable connection and unsupported pool filters from these facade contracts. An absent binding fails explicitly.
- Correct the API IAccountingService factory to supply the required royalty and allocation services; previously the factory resolved services but omitted them from the constructor.
- Select active recorded calculations by inclusive calculation dates, using an exclusive next-day upper bound and invariant date formatting. Retain archived parent records as financial provenance. Do not invent a field from a lease, require parent records to remain active, or run new calculations while reporting.
- Reject missing or duplicate relationship identities instead of double-counting amounts. Propagate provider errors rather than return a successful empty report. Sort by calculation date and stable ID.
- Keep preview read-only and on the same bound production database as its allocation and royalty services.
- Require field ID in the typed reporting client and reject a null API response. The page labels calculation dates, separates errors from empty reports, hides stale totals during reload, follows field-change events and prevents older requests replacing newer field/filter results. A preview does not create report records.

## Verification

- Focused royalty tests: **24 passed** (11 reporting/authorization regressions plus 13 existing recorded-input/preview tests).
- Full API project tests: **901 passed, 10 skipped, 0 failed**, 911 total. Evidence: Beep.OilandGas.ApiService.Tests/TestResults/royalty-reporting-api-full.trx.
- Web authentication project: **91 passed**; Web Razor compilation succeeded during this run. Evidence: Beep.OilandGas.Web.Auth.Tests/TestResults/royalty-reporting-web.trx.
- Final Web build and whitespace checks are recorded in the master tracker.

Tests use the actual PPDM repository over mocked IDataSource plus controller authorization tests. They cover field/lease distinction, archived provenance, inclusive dates, inactive records, duplicate IDs at each relationship level, no writes, missing binding, provider failure, external-subject rejection and denied field access. They do not replace live provider or browser acceptance.

## Remaining work

Reporting currently uses portable filtered repository reads per ticket/allocation/detail. Add measured pagination/batched reads through the existing provider abstraction before high-volume deployment; do not replace this with unscoped table reads or provider-specific SQL in Web.

The separate posting/payment endpoints still need persisted-input reload, authoritative permissions and approvals, database-enforced idempotency, and transaction/recovery guarantees. The in-memory payment path and caller-supplied calculation payment route are not certified by this change. Payment reporting and standard-specific valuation/tax rules are outside this read-only field-report slice. Ten LocalDB tests remain skipped, and live OIDC/browser and clean-CI external dependency checks remain open.
