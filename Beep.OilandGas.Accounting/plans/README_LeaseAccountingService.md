# LeaseAccountingService

## Overview

The `LeaseAccountingService` implements **IFRS 16** lease accounting. It handles the single lessee accounting model, recognizing ROU Assets and Lease Liabilities, with separate recognition of interest expense and amortization.

## Key Functionality

- **Commencement**: Initial recognition.
  - GL: Debit ROU Asset, Credit Lease Liability.
- **Payments**: Splits payment between Principal (Liability reduction) and Interest Expense.
  - GL: Debit Lease Liability, Debit Interest Expense, Credit Cash.
- **Amortization**: Depreciates the ROU Asset.
  - GL: Debit Amortization Expense, Credit ROU Asset.
- **Measurement**: Calculates Present Value of future lease payments.

## Key Methods

### `MeasureInitialLeaseAsync`
Calculates PV and records commencement entry.

### `RecordLeasePaymentAsync`
Records payment and calculates interest portion.

### `RecordLeaseAmortizationAsync`
Records periodic amortization of the ROU asset.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
- `LEASE_CONTRACT` / `LEASE_PAYMENT` / `LEASE_ACCOUNTING_ENTRY` tables
