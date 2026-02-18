# FirstTimeAdoptionService

## Overview

The `FirstTimeAdoptionService` handles transition adjustments for entities adopting IFRS for the first time, in accordance with **IFRS 1**.

## Key Functionality

- **Transition Adjustments**: Records adjustments to opening balances (Retained Earnings) and the Reset Reserve.
  - GL: Debit/Credit Account, Credit/Debit Retained Earnings (or Restatement Reserve).

## Key Methods

### `RecordTransitionAdjustmentAsync`
Records a transition adjustment entry.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
