# Website royalty details workflow - 2026-09-21

## User-visible delivery

The existing Accounting > Royalties navigation now leads from the field report to a View details action on each row. The new authenticated route `/ppdm39/accounting/royalties/{CalculationId}` shows the stored obligation, owner/lease, amounts, status and calculation date. Users can inspect paged payment history and request a paged posting review with readable findings and next steps. Back to royalties and Refresh are explicit actions. Switching the selected field clears the details state and returns to the field report.

The page distinguishes loading, no recorded payments, missing records, expired sessions, denied access, failed payment history and failed posting review. Failures in a secondary section preserve the loaded obligation. Posting review runs only when requested and makes no financial changes. The API independently checks app-owned posting permission and source-field access. Both royalty routes use standard ASP.NET Authorize; IdentityServer remains authentication-only.

The report now validates date order before sending requests and offers a retry action. All new UI uses existing MudBlazor/theme components; no inline CSS, scripts or hardcoded colors were introduced. Amounts use numeric formatting rather than inventing a currency that the response does not supply.

## Supporting website framework changes

Extended the existing typed AccountingServiceClient with calculation detail, payment history and posting review reads. Escaped IDs and cancellation flow through ApiClient. Required null payloads fail explicitly. ApiClient GET preserves HttpRequestException.StatusCode so pages can distinguish 401/403/404 from service outages, and disposes the HTTP response.

RoyaltyDetailsState cancels obsolete requests, rejects late results by generation, clears displayed data on navigation/disposal and keeps section errors independent. The website tests exposed a payment JSON collision between STATUS and its Status alias. Marked the alias JsonIgnore so the canonical status property is the wire representation; the new round-trip regression exercises actual Web JSON options and typed-client deserialization.

## Validation

Website test project: **103 passed, zero failed**, including 12 new royalty website regressions. Tests cover route authentication, typed-client null payloads/escaped IDs, canonical payment JSON, access/error messages, secondary-section failures, explicit review requests, stale responses and disposal. The test build compiles the actual Web Razor pages. Evidence: Beep.OilandGas.Web.Auth.Tests/TestResults/royalty-website.trx; log `%TEMP%/beep-royalty-website-tests.log`.

Full API regression result is recorded in MASTER-TODO-TRACKER.md; evidence Beep.OilandGas.ApiService.Tests/TestResults/royalty-website-api.trx. Whitespace checks passed. These are client/state tests and Razor compilation, not live browser/OIDC, responsive visual or keyboard/screen-reader acceptance. Existing build warnings remain.

## Website-first next work

Prioritize FG-50/52: verify the accountant journey in the running website, then improve persona-driven navigation and the next complete engineer/HSE/manager workflow. Persona selection must never grant roles. Add framework changes only where needed by an identified website interaction. The separate financial claim/recovery infrastructure remains unfinished and is not the next standalone workstream.

Browser acceptance: navigate Accounting > Royalties, filter dates, open a record, inspect paged payments, request review with allowed and denied accounts, test retry after an outage, switch fields while loading and verify return navigation. No payment initiation or recovery mutation UI is exposed by this slice.
