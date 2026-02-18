# BusinessCombinationService

## Overview

The `BusinessCombinationService` handles the accounting for business acquisitions under **IFRS 3**. It calculates Goodwill or Bargain Purchase Gain based on the difference between consideration paid and the fair value of net assets acquired.

## Key Functionality

- **Acquisition Recording**: Records the net assets acquired and consideration paid.
- **Goodwill Calculation**:
  - If Consideration > Net Assets: Debit Goodwill.
  - If Consideration < Net Assets: Credit Bargain Purchase Gain (P&L).

## Key Methods

### `RecordBusinessCombinationAsync`
Performs the full acquisition entry, automatically balancing with Goodwill or Gain.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
