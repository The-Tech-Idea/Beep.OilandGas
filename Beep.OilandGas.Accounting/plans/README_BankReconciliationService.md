# BankReconciliationService

## Overview

The `BankReconciliationService` ensures that the company's General Ledger (GL) cash balance matches specific bank statements. It identifies discrepancies caused by timing differences (Checks not cleared, Deposits in transit) or errors.

## Workflow

1.  **Input**: Bank Statement Balance, Statement Date, list of Cleared Items.
2.  **Process**:
    - Calculates `Adjusted GL Balance` = GL Balance + Outstanding Checks - Deposits in Transit.
    - Compares `Adjusted GL Balance` to `Bank Statement Balance`.
3.  **Output**: Reconciliation Report showing difference (if any).

## Key Methods

### `ReconcileBankAccountAsync`
Performs the core reconciliation logic. Returns a `BankReconciliation` object indicating if the account is balanced.

### `AnalyzeCheckClearingAsync`
Analyzes how long it takes for checks to clear, helping with cash forecasting.

### `AnalyzeAgedOutstandingItemsAsync`
Identifies stale checks or deposits that may need write-off or investigation.

## Models
- `BankReconciliation`
- `OutstandingCheck`
- `DepositInTransit`
