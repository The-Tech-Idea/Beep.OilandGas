# ReconciliationService

## Overview

The `ReconciliationService` performs internal audits by reconciling subledger balances against General Ledger (GL) control accounts. This ensures data integrity between operational modules (AR, AP, Inventory) and the financial core.

## Key Functionality

- **AR Reconciliation**: Compares `AR_INVOICE` balance vs. AR GL Account.
- **AP Reconciliation**: Compares `AP_INVOICE` balance vs. AP GL Account.
- **Inventory Reconciliation**: Compares `INVENTORY_ITEM` valuation vs. Inventory GL Account.

## Key Methods

### `ReconcileAccountsReceivableAsync`
### `ReconcileAccountsPayableAsync`
### `ReconcileInventoryAsync`

## Dependencies
- `GLAccountService`
- `AR_INVOICE`, `AP_INVOICE`, `INVENTORY_ITEM` tables
