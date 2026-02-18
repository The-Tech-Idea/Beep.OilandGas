# AssetsHeldForSaleService

## Overview

The `AssetsHeldForSaleService` manages non-current assets classified as held for sale under **IFRS 5**. It handles the reclassification from non-current to current assets and impairment testing upon classification.

## Key Functionality

- **Classification**: Transfers an asset from its original category (e.g., Fixed Assets) to "Held for Sale" (Current Asset).
  - GL: Debit Held for Sale Asset, Credit Fixed Asset.
- **Impairment**: Checks if Fair Value less costs to sell is lower than the carrying amount and records impairment if necessary.
  - GL: Debit Impairment Loss, Credit Held for Sale Asset.

## Key Methods

### `ClassifyAssetHeldForSaleAsync`
Moves the asset to the Held For Sale account.

### `RecordHeldForSaleImpairmentAsync`
Records any necessary write-down upon classification or subsequent measurement.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
