# AgricultureService

## Overview

The `AgricultureService` manages biological assets (living animals or plants) in accordance with **IAS 41**. It handles initial recognition and subsequent measurement at fair value less costs to sell.

## Key Functionality

- **Acquisition**: Records the purchase of biological assets.
  - GL: Debit Biological Assets, Credit Cash.
- **Fair Value Changes**: Recognizes gains or losses due to physical changes (growth) or price changes.
  - GL (Gain): Debit Biological Asset, Credit Fair Value Gain (P&L).
  - GL (Loss): Debit Fair Value Loss (P&L), Credit Biological Asset.

## Key Methods

### `RecordBiologicalAssetAsync`
Records the initial acquisition.

### `RecordFairValueChangeAsync`
Updates the asset's book value and recognizes the P&L impact.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
