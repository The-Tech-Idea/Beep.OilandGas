# ARService

## Overview

The `ARService` manages Accounts Receivable, including direct customer invoices, payments, and credit memos.

## Workflow

1.  **Invoice Creation**: Create invoice (DRAFT).
2.  **Approval**: Approve invoice (ISSUED). Posts Revenue/AR.
3.  **Payment**: Receive and apply payment. Posts Cash/AR.
4.  **Adjustments**: Create Credit Memos for returns/disputes. Posts Revenue Reversal/AR.

## Key Methods

### `CreateInvoiceAsync`
Creates a DRAFT invoice.

### `ApproveInvoiceAsync`
Validates and approves the invoice.
*   **GL Impact**: Debit AR (1110), Credit Revenue (4001).

### `CreatePaymentAsync` / `ApplyPaymentAsync`
Records receipt of cash and applies it to specific invoices.
*   **GL Impact**: Debit Cash (1000), Credit AR (1110).

### `CreateCreditMemoAsync`
Reduces the invoice balance via a credit memo.
*   **GL Impact**: Debit Revenue (4001), Credit AR (1110).

### `GetARAgingAsync`
Standard AR Aging report.

## Data Models
- `AR_INVOICE`
- `AR_PAYMENT`
- `AR_CREDIT_MEMO`
