# InvestmentPropertyService

## Overview

The `InvestmentPropertyService` manages properties held to earn rentals or for capital appreciation (or both), following **IAS 40**. It supports the Fair Value recognition model.

## Key Functionality

- **Acquisition**: Records the purchase of investment property.
  - GL: Debit Investment Property (1260), Credit Cash.
- **Fair Value Adjustments**: Revalues property to market price.
  - GL (Gain): Debit Investment Property, Credit Fair Value Gain (Other Income).
  - GL (Loss): Debit Fair Value Loss (Expense), Credit Investment Property.

## Key Methods

### `RegisterInvestmentPropertyAsync`
Records the initial purchase.

### `RecordFairValueChangeAsync`
Updates the book value based on new valuation and recognizes gain/loss in P&L.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
