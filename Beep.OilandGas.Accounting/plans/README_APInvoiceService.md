# APInvoiceService

## Overview

The `APInvoiceService` manages the Accounts Payable lifecycle for vendor bills. It handles creating bills, receiving them (posting expense), and recording payments (posting cash/liability reduction).

## Responsibilities

- **Bill Lifecycle**: Create (DRAFT) -> Receive (RECEIVED/POSTED) -> Pay (PARTIALLY_PAID/PAID).
- **GL Posting**:
  - **Receipt**: Dr. Expense (6001), Cr. Accounts Payable (2000).
  - **Payment**: Dr. Accounts Payable (2000), Cr. Cash (1000).
- **Reporting**: Generates AP Aging reports.

## Key Methods

### `CreateBillAsync`
Creates a new bill in DRAFT status. No GL impact.

### `ReceiveBillAsync`
Transitions bill to RECEIVED. Posts the expense recognition entry to the GL.
*   **GL Impact**: Debit Expense, Credit AP.

### `RecordPaymentAsync`
Records a payment against the bill. Deducts from Balance Due.
*   **GL Impact**: Debit AP, Credit Cash.
*   **Status Update**: Updates status to PARTIALLY_PAID or PAID based on remaining balance.

### `GetAPAgingAsync`
Returns a breakdown of outstanding bills by age bucket (Current, 31-60, 61-90, 90+).

## Dependencies
- `AccountingBasisPostingService`: For creating GL entries.
- `IAccountMappingService`: For resolving Expense/AP/Cash accounts.
