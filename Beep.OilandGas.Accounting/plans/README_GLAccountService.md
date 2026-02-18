# GLAccountService

## Overview

The `GLAccountService` manages the Chart of Accounts master data. It handles creation, retrieval, validation, and balance calculation for General Ledger accounts.

## Responsibilities

- **Master Data Management**: CRUD operations for `GL_ACCOUNT` entities.
- **Validation**: Verifies account existence and active status.
- **Balance Calculation**: Computes account balances from posted `GL_ENTRY` records, respecting the Normal Balance (Debit/Credit) of the account type.

## Key Methods

### `GetAccountByNumberAsync(string accountNumber)`
Retrieves a `GL_ACCOUNT` by its user-facing account number.

### `GetAccountBalanceAsync(string accountNumber, DateTime? asOfDate)`
Calculates the current balance.
- **Debit Normal Accounts (Asset, Expense)**: `Sum(Debit) - Sum(Credit)`
- **Credit Normal Accounts (Liability, Equity, Revenue)**: `Sum(Credit) - Sum(Debit)`

### `CreateAccountAsync(...)`
Creates a new GL Account with proper audit fields (CreatedBy, CreatedDate) and PPDM GUIDs.

### `ValidateAccountAsync(string accountNumber)`
Returns `true` if the account exists and `ACTIVE_IND = "Y"`.

## Dependencies

- `IDMEEditor`: Data access.
- `ICommonColumnHandler`: Audit field management.
- `IPPDMMetadataRepository`: Metadata driven repository access.
- `IPPDM39DefaultsRepository`: Default value management.
