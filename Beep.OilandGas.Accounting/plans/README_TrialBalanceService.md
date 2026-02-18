# TrialBalanceService

## Overview

The `TrialBalanceService` is the primary tool for validating the mathematical accuracy of the General Ledger. It aggregates all account balances to ensure the fundamental accounting equation holds.

## Responsibilities

- **Generation**: Calculates current balances for all accounts.
- **Validation**: Checks `Total Debits == Total Credits`.
- **Reporting**: Exports TB to CSV.

## Key Methods

### `GenerateTrialBalanceAsync`
Returns a list of `GL_ACCOUNT` entities with their `CURRENT_BALANCE` populated.

### `ValidateGLAsync`
Returns a boolean indicating if the ledger is balanced, along with the magnitude of any difference.

### `GetPostClosingTrialBalanceAsync`
Filters the TB to show only **Permanent Accounts** (Assets, Liabilities, Equity), effectively representing the Balance Sheet.
