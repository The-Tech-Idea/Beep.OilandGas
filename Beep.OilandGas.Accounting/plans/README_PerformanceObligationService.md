# PerformanceObligationService

## Overview

The `PerformanceObligationService` supports **IFRS 15** by tracking distinct performance obligations within a contract and allocating the transaction price to them.

## Key Functionality

- **Obligation Creation**: Defines distinct deliverables.
- **Price Allocation**: Distributes total contract value across obligations based on standalone selling prices.
- **Satisfaction**: Recognizes revenue when an obligation is met.
  - GL: Debit Contract Liability (or AR/Contract Asset), Credit Revenue.

## Key Methods

### `CreateObligationAsync`
Defines a new obligation.

### `AllocateTransactionPriceAsync`
Allocates revenue potential to obligations.

### `SatisfyObligationAsync`
Triggers revenue recognition for a specific obligation.

### `RecordContractLiabilityAsync` / `RecognizeRevenueFromLiabilityAsync`
Manages the liability side of advance payments.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
