# FixedAssetLifecycleService

## Overview

The `FixedAssetLifecycleService` manages the accounting lifecycle of fixed assets in compliance with **IAS 16**. It handles capitalization, depreciation posting, and disposal.

## Key Processes

1.  **Capitalization (Acquisition)**:
    - Creates `EQUIPMENT` record.
    - Creates `ACCOUNTING_COST` record (Capitalized).
    - Posts GL Entry: Debit Fixed Assets (1200), Credit Cash/AP.

2.  **Depreciation Posting**:
    - Periodically posts depreciation expense.
    - Debit Depreciation Expense (6002), Credit Accumulated Depreciation (1210).

3.  **Disposal**:
    - Calculates Gain/Loss.
    - Removes Asset and Accumulated Depreciation from GL.
    - Posts GL Entry: Debit Cash, Debit Accum Dep, Credit Asset, Credit/Debit Gain/Loss.

## Key Methods

### `RegisterAssetAsync`
Handles the initial acquisition and capitalization.

### `PostDepreciationAsync`
Posts the actual GL journal entry for a specific period's depreciation.

### `DisposeAssetAsync`
Handles the sale or scrapping of an asset, ensuring the balance sheet is cleared and P&L reflects the result.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
