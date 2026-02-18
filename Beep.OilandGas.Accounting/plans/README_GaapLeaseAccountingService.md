# GaapLeaseAccountingService

## Overview

The `GaapLeaseAccountingService` handles **ASC 842** lease accounting (US GAAP). It recognizes Right-of-Use (ROU) Assets and Lease Liabilities for both operating and finance leases.

## Key Functionality

- **Commencement**: Records the initial ROU Asset and Lease Liability.
  - GL: Debit ROU Asset, Credit Lease Liability.
- **Lease Payments**: Reduces the liability.
  - GL: Debit Lease Liability, Credit Cash.

## Key Methods

### `RecordLeaseCommencementAsync`
Initial recognition of the lease.

### `RecordLeasePaymentAsync`
Records a periodic lease payment.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
