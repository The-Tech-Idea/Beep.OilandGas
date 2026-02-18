# IntangibleAssetService

## Overview

The `IntangibleAssetService` manages intangible assets (e.g., patents, software, mineral rights) in compliance with **IAS 38**. It handles capitalization and amortization.

## Key Functionality

- **Capitalization**: Records the initial cost of the intangible asset.
  - GL: Debit Intangible Assets, Credit Cash/AP.
- **Amortization**: Allocates the cost of the asset over its useful life.
  - GL: Debit Amortization Expense, Credit Accumulated Amortization.

## Key Methods

### `CapitalizeIntangibleAsync`
Creates the asset record and posts the acquisition entry.

### `RecordAmortizationAsync`
Posts the periodic amortization expense.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
