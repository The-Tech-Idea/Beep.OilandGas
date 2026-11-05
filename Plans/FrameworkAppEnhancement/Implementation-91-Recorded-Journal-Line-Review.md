# Implementation 91: recorded journal line review

Updated 2026-10-06. Extends [Implementation 90](Implementation-90-Recorded-Journal-Source-Screen.md) on the existing accountant source/action screen. Partial source/state delivery; native/rendered financial approval/post/readback remains open.

## Reviewer workflow

The same recorded journal review now includes line identity/number, GL account, cost center, debit, credit, description, currency and exchange rate. The existing source page displays these in a responsive MudBlazor table with DataLabel cells. Null credit/debit/rate and blank currency/cost center remain unavailable; no zero, rate of one or currency default is substituted. Lines sort by recorded number then ordinal identity; missing numbers sort last.

The header has no authoritative currency/period fields. Line currency is shown per line, without inferring a common header currency or adding amounts across currencies. A genuine empty recorded line result is explicit and does not imply financial posting readiness. GL account identity is not presented as proof of account validity or authority, and line/header visibility is not a posting receipt.

## Active owners

The existing JournalEntryService.GetEntryLineItemsAsync is exposed through the existing IJournalEntryService. Its existing bound Beep repository remains the data owner; empty journal IDs are refused and a null provider result now fails rather than becoming an empty list. No new repository, direct frontend database access or SQL is added.

BusinessProcessController reads the header and lines through that registered request-bound journal owner during each existing review capture. ProcessTransitionReviews requires nonnull line evidence when a journal is supplied and validates unique nonempty line identities, exact parent journal and nonempty account identities. The review hash includes all recorded line fields in stable identity order. Changed debit/currency/other line content therefore invalidates the original revision; provider row order alone does not.

TheTechIdea.Data owns JournalSourceLineReview within the existing shared review projection. ProcessInstanceDetailState repeats exact parent/identity/account/uniqueness checks before publication. Missing lines, foreign/duplicate rows or invalid account identities cannot attach an action review to the source screen. Source reload/company invalidation clears the projection with the existing review.

## Focused evidence

Actual Data sources built in isolated output: zero warnings/errors, terminal exit 0, 2.03 seconds. Log `%TEMP%/oilgas-journal-line-data-build.log`.

- Test-ProcessTransitionReviews.ps1: **27 passed**, zero failures/skips, terminal exit 0, 1.3473 seconds. Eight new cases cover missing/foreign/duplicate/empty-identity/account lines, exact nullable currency/rate projection, changed debit/currency revision and stable provider/display ordering. Log `%TEMP%/oilgas-journal-line-reviews.log`.
- Test-ProcessInstanceDetailState.ps1: **31 passed**, zero failures/skips, terminal exit 0, 1.0097 seconds. Five new cases reject missing/foreign/duplicate/invalid-account projections and preserve recorded nullable line values before reload clears them. Log `%TEMP%/oilgas-journal-line-state.log`.
- Whitespace diff check passed.

These verify actual factory/state sources against actual Data/Models assemblies. The journal service/provider, controller/DI/HTTP and Razor rendering are source-only here; the focused tests do not execute the changed service getter or provider null handling. Missing production dependencies are not replaced.

## Remaining full journey gates

Verify bound journal/line reads and resource authority through native providers with two companies, colliding IDs and revoked access. Test concurrent header/line changes, exact accounting book/period/currency, GL account validity, recorded approval and actual atomic posting receipt/read-back. A SHA preflight does not prove transactional consistency. Durable original command intent/reconciliation and actual financial KPI drill-down remain open.

Render the source table and action confirmation in keyboard/RTL/mobile/error/reload journeys. Accounting approval/posting readiness is not certified by these read projections. Continue through existing financial owners before broadening to production/operator -> maintenance and the remaining persona plans.

No SQL, schema change, business EF lane, grants, fabricated data or phase/goal closure.
