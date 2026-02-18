# GovernmentGrantService

## Overview

The `GovernmentGrantService` manages the accounting for government grants under **IAS 20**. It supports the deferral method, recognizing income over the periods necessary to match them with the related costs.

## Key Functionality

- **Award**: Records the grant receivable or cash and the deferred liability.
  - GL: Debit Grant Receivable/Cash, Credit Deferred Grant Liability.
- **Income Recognition**: Amortizes the deferred income to P&L.
  - GL: Debit Deferred Grant Liability, Credit Grant Income.

## Key Methods

### `RecordGrantAwardAsync`
Records the initial grant.

### `RecordGrantReceivableSettlementAsync`
Records cash receipt for a receivable.

### `RecognizeGrantIncomeAsync`
Recognizes a portion of the grant as income.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
