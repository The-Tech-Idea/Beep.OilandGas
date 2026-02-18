# GaapRevenueRecognitionService

## Overview

The `GaapRevenueRecognitionService` implements **ASC 606** revenue recognition principles (Revenue from Contracts with Customers) for US GAAP. It focuses on contract assets and liabilities.

## Key Functionality

- **Revenue Recognition**: Recognizes revenue when performance obligations are satisfied.
  - From Contract Asset: Debit Revenue, Credit Contract Asset.
  - From Contract Liability: Debit Contract Liability, Credit Revenue.
- **Contract Billing**: Records billings against the contract.
  - Debit A/R (or Contract Asset), Credit Contract Liability.

## Key Methods

### `RecognizeContractRevenueAsync`
Recognizes revenue based on progress/performance.

### `RecordContractBillingAsync`
Records the billing event.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
