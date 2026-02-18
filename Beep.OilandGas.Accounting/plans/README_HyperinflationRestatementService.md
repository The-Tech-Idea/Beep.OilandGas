# HyperinflationRestatementService

## Overview

The `HyperinflationRestatementService` implements **IAS 29** financial reporting in hyperinflationary economies. It restates non-monetary items and calculates gains/losses on the net monetary position.

## Key Functionality

- **Non-Monetary Restatement**: Adjusts historical cost to current purchasing power.
  - GL: Debit/Credit Asset/Liability, Credit/Debit Restatement Reserve.
- **Monetary Gain/Loss**: Calculates the impact of inflation on net monetary assets.
  - GL: Debit/Credit Inflation Loss/Gain, Credit/Debit Restatement Reserve.

## Key Methods

### `RestateNonMonetaryBalanceAsync`
Restates a specific non-monetary item.

### `RecordMonetaryGainLossAsync`
Records the net monetary gain or loss for the period.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
