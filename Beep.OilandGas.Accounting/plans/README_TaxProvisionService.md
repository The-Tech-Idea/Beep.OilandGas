# TaxProvisionService

## Overview

The `TaxProvisionService` manages the recording of tax transactions in compliance with **IAS 12**. It handles Current Tax entries and Deferred Tax accounting.

## Key Functionality

- **Current Tax**: Records the actual tax payable to authorities.
  - GL: Debit Income Tax Expense, Credit Income Tax Payable.
- **Deferred Tax**: Records future tax consequences of temporary differences.
  - GL: Adjusts Deferred Tax Asset/Liability and Income Tax Expense.
- **Tax Returns**: Tracks filed returns and total liability.

## Key Methods

### `RecordCurrentTaxAsync`
Records a specific tax liability (e.g., Corporate Income Tax for 2023).

### `RecordDeferredTaxBalanceAsync`
Updates the Deferred Tax Asset/Liability balances based on period-end calculations, posting the potential movement to tax expense.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
