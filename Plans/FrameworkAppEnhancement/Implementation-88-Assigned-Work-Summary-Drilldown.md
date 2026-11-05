# Implementation 88: assigned-work summary drill-down

Updated 2026-10-06. Extends the accountant landing and shared task inbox from [Implementation 80](Implementation-80-Company-Assigned-Work-Landing.md). Source/state delivery; native/rendered persona journey remains open.

## Visible workflow

The accounting landing's existing PersonaTaskInbox now presents selectable pending, approval, review, data-entry, overdue and critical summaries. Selecting a summary opens exactly its contributing authorized records from the same loaded inbox response. Selecting another summary resets the previous work-type/attention combination. Work type and attention can also be combined explicitly. The screen reports the active filters and shown/total task count, with an explicit empty state for an intersection containing no records.

Every row retains the existing exact encoded source-process link. Critical/high/normal buckets are combined once, then ordered by priority, due date (missing dates last) and ordinal task identity. Summary counts continue to describe the complete loaded response, rather than being silently rewritten by a filter. No second counts request or separate financial aggregation is introduced. The same component serves the existing task inbox.

## Active owners

PersonaTaskInboxState owns filter selection and row projection. Its existing validation requires company/account/persona, counts and task source links to match before publishing the response. Reload, company/persona invalidation and failed reads clear values and selections. Invalid filter names fail explicitly rather than becoming an unfiltered result. Filtering performs no additional API request and never grants task authority.

PersonaTaskInbox uses existing MudBlazor buttons/selects and a status message for filtered row counts. Counts are labeled as loaded task counts; reload is the refresh action. Existing backend selected-company role/workspace and source resource checks remain the authority. This change does not certify those checks through native HTTP or add a page-level role grant.

## Focused evidence

Test-PersonaTaskInboxState.ps1 compiled actual state/context/test sources against actual Data assembly: **22 passed**, zero failures/skips, terminal exit 0, 1.2259 seconds. Log `%TEMP%/oilgas-work-summary-drilldown.log`. Six summary cases prove exact source identities and cardinalities, including resetting prior intersected filters. Four additional cases cover intersection, context clearing, failed reload and invalid filters. Previous twelve company/workspace/source-response cases also pass. Whitespace diff check passed.

## Remaining delivery gates

Rendered keyboard/RTL/mobile operation, Razor/DI/native transport and current role/resource-store authority remain unexecuted. Counts are task quantities; invoice aging/currency, monetary totals, ledger reconciliation and period readiness require their actual financial sources and identical authorized drill-downs. There is no claim that task counts deliver those finance KPIs.

Continue accountant source transaction -> independent approval/post -> recorded read-back -> financial dashboard drill-down, then production/operator -> maintenance using existing screens and service owners. Durable workflow intent/receipt remains open as recorded in [Implementation 87](Implementation-87-Source-Action-Repeat-Protection.md).

No SQL, schema change, business EF lane, invented data, role grant or phase/goal closure.
