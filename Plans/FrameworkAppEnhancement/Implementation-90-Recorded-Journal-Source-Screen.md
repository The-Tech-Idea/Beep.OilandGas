# Implementation 90: recorded journal source screen

Updated 2026-10-06. Continues accountant landing -> assigned work -> source -> reviewed action from [Implementation 89](Implementation-89-Recorded-Journal-Preparer-Review.md). Partial source/state delivery; actual rendered finance approval/post/readback/KPI journey remains open.

## Reviewer screen

The existing process source page now asks to Review source and available actions. For a journal source, that same API review displays its recorded journal identity/number, date, status, description, preparer, creation time, reference and source module before the action panel. The response is already bound to the selected company/account/FIELD/process and action revision; no additional frontend database access or alternate journal read path is introduced.

Recorded debit/credit values preserve nullable amounts. Missing values are displayed as unavailable, while an actual zero stays zero. No culture-derived currency symbol is appended. Currency and accounting period remain explicitly unavailable because the current header does not supply authoritative fields for them. A journal header or workflow transition is not presented as balanced lines or confirmed ledger posting.

Book is shown only when the existing writer's SOURCE and exact matching BOOK= remark agree on a nonempty value. Missing/conflicting markers remain unavailable; no IFRS or GAAP default is inferred. Retrieval time is labeled as retrieval, rather than transactional source-as-of evidence. The screen also shows the reviewed revision and permits the existing recorded history reload.

## Active owner changes

TheTechIdea.Data owns JournalSourceReview, a shared read projection appended to the existing ProcessTransitionReview. Existing legacy journal entities retain their current owner; no duplicate journal entity is introduced. ProcessTransitionReviews projects the actual captured JOURNAL_ENTRY used for original-preparer checks and review revision. No source contract is independently reconstructed in Web.

ProcessInstanceDetailState requires a journal projection for a JOURNAL_ENTRY source, validates exact entity identity and a nondefault UTC retrieval time, and rejects journal payloads on non-journal sources. Existing company/actor/FIELD/state/step/revision checks still apply. Reload/invalidation clears the projection together with the action review. The source page renders the projection with existing MudBlazor components; no custom CSS/JS added.

## Focused evidence

Actual Data sources built in isolated output: zero warnings/errors, terminal exit 0, 2.04 seconds. Log `%TEMP%/oilgas-journal-source-data-build.log`.

- Test-ProcessTransitionReviews.ps1: **19 passed**, zero failures/skips, terminal exit 0, 1.0356 seconds. New cases verify exact recorded fields, missing versus zero amounts, UTC retrieval time, absent/conflicting book evidence and non-journal projection absence; previous maker/checker and revision cases pass. Log `%TEMP%/oilgas-journal-source-projection.log`.
- Test-ProcessInstanceDetailState.ps1: **26 passed**, zero failures/skips, terminal exit 0, 0.9610 seconds. New cases reject missing/wrong-entity/invalid-time journal evidence, verify source invalidation and refuse journal details on a non-journal source. Previous source lifecycle/repeat-protection cases pass. Log `%TEMP%/oilgas-journal-source-state.log`.
- Whitespace diff check passed.

These are actual projection/state sources with actual Data/Models references. They do not compile/render Razor or execute native API/DI/company-bound accounting providers. Missing production dependencies are not substituted.

## Outstanding full journey

Obtain authoritative accounting period/currency, journal line items, account validity and approval/posting authority through existing accounting/workflow owners. Verify actual maker/checker outcomes, ledger posting receipt, persisted read-back, financial KPI drill-down, atomic version checks and durable command reconciliation. Render the current source screen in keyboard/RTL/mobile/company-switch/error scenarios and verify typed HTTP/provider binding with colliding company IDs and revoked authority. This header projection does not certify any of those gates or financial approval/posting readiness.

No SQL, schema change, business EF lane, role grants, fabricated financial data or phase/goal closure.
