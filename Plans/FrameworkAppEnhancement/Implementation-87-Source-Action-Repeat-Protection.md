# Implementation 87: source action repeat protection

Updated 2026-10-06. Continues the accountant assigned-work -> recorded source -> reviewed action screen from [Implementation 86](Implementation-86-Source-Action-Review-and-Screen.md). Partial source delivery; no persona or phase closure.

## Screen behavior and active owner

The existing ProcessInstanceDetailState retains an attempted reviewed action before the panel hands it to transport. The original company, account, FIELD, process and revision stay attached to that attempt. Uncertain responses block further actions on that record, including after ordinary reload or a newly returned revision. Reload remains available to inspect recorded history. History or matching current state does not certify which command ran.

An exactly matching successful response marks the response returned, rather than claiming durable completion. That reviewed revision remains blocked; a fresh changed revision may offer the next action. Wrong/null/failed responses, exceptions and scope changes preserve uncertainty. Unreviewed choices and invalidated reviews cannot start an attempt. No automated retry or alternate command owner is introduced.

The existing MudBlazor panel disables repeat actions, shows submission progress and explains an already-used review. The existing source page shows an unconfirmed-outcome alert after submission and after reload. Changing company hides another company's attempt; returning to the original company restores its block. The same page state retains attempts across FIELD/source invalidation.

## Focused verification

Test-ProcessInstanceDetailState.ps1 compiled actual state/context/test sources against actual Data and Models assemblies: **20 passed**, zero failed/skipped, terminal exit 0, 4.1362 seconds. Log `%TEMP%/oilgas-source-repeat-state.log`. Four new cases exercise uncertain repeat after reload/changed revision, returned-response revision gating, company-return isolation, and unreviewed/invalidated choices. Prior sixteen source lifecycle/action-review cases also passed. Whitespace diff check passed.

## Remaining gates

This retention lasts only while this source page state lives. Leaving the page, a new browser session or reconnect can lose it. Durable command identity/receipt, authoritative reconciliation, provider atomic version enforcement and idempotency must extend existing workflow owners before durable retry protection is certified. There is deliberately no clear-uncertainty button backed only by current-state comparison.

Razor/DI/native HTTP and rendered notification, keyboard, RTL and reconnect journeys remain unexecuted due to missing production dependencies. These focused state tests do not certify panel transport behavior. Actual journal maker, company/book/period/currency, approval/posting and dashboard drill-down remain open in the accountant plan. Continue those visible workflows and then production/operator -> maintenance.

No SQL, schema change, business EF migration, grants, fabricated dashboard data or goal completion.
