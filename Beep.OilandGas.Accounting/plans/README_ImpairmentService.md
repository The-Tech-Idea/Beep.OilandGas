# ImpairmentService

## Overview

The `ImpairmentService` handles the recognition of asset impairment losses in accordance with **IAS 36**. It ensures that assets are not carried at more than their recoverable amount.

## Key Functionality

- **Impairment Recognition**: Records a write-down when an asset's value declines.
  - GL: Debit Impairment Loss (Expense), Credit Impairment Allowance (Contra-Asset).

## Key Methods

### `RecordImpairmentAsync`
Records the impairment event, updates the allowance, and posts the expense to the GL.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
