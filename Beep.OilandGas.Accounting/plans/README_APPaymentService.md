# APPaymentService

## Overview

The `APPaymentService` manages the outbound payment process for vendor bills.

## Responsibilities

- **Record Payment**: Tracks payment details (Amount, Date, Method, Check #).
- **GL Posting**: Creates the journal entry to reduce liability and cash.
- **Linkage**: Associates the payment with specific `AP_INVOICE` records.

## Key Methods

### `RecordPaymentAsync`
Records a payment against a specific invoice.
*   **GL Impact**: Debit Accounts Payable (2000), Credit Cash (1000).

### `GetInvoicePaymentsAsync`
Retrieves history of payments for a given bill.

### `GetTotalPaidAsync`
Helper to quickly see how much has been paid on a bill.
