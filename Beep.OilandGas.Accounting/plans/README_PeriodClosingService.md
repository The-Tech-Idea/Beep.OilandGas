# PeriodClosingService

## Overview

The `PeriodClosingService` manages the formal closing of accounting periods (Month-End, Year-End). It ensures data integrity and prepares the GL for the next period.

## The Closing Process

1.  **Validation**: Ensures Debit = Credits (`CheckCloseReadinessAsync`).
2.  **Closing Entries**:
    - Zeroes out Temporary Accounts (Revenue, Expense).
    - Transfers Net Income/Loss to Retained Earnings.
3.  **Locking**: Marks the period as closed to prevent new backdated entries.
4.  **Verification**: Generates a Post-Closing Trial Balance to ensure only Permanent Accounts remain.

## Key Methods

### `ClosePeriodAsync`
Executes the full closing workflow.

### `ReopenPeriodAsync`
Reverses closing entries if a period needs to be re-opened for corrections.

### `GetClosingChecklistAsync`
Returns a status report of prerequisite tasks (Bank Rec, Inventory Count, etc.).

## Dependencies
- `TrialBalanceService` (for validation)
- `JournalEntryService` (for posting closing entries)
