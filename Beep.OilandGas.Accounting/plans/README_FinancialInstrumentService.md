# FinancialInstrumentService

## Overview

The `FinancialInstrumentService` handles the recognition, measurement, and fair value adjustments of financial instruments under **IAS 32** and **IAS 39** (and IFRS 9).

## Key Functionality

- **Initial Recognition**: Records the financial asset or liability at fair value (usually transaction price).
  - Liability: Debit Cash, Credit Financial Instrument Liability.
  - Asset: Debit Financial Instrument Asset, Credit Cash.
- **Fair Value Remeasurement**: Updates the carrying amount to reflect current fair value.
  - Gain/Loss is recognized in Profit & Loss (FVTPL) unless designated otherwise.

## Key Methods

### `RecordInstrumentAsync`
Records the initial purchase or issuance.

### `RecordFairValueChangeAsync`
Updates value and posts gain/loss.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
