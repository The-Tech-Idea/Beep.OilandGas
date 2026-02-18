# InvoiceService

## Overview

The `InvoiceService` is a more generic invoicing service compared to `ARService`, often used for non-standard billing or consolidated invoicing workflows. It uses the `INVOICE` entity family.

## Key Differences from ARService

- Uses `INVOICE`, `INVOICE_LINE_ITEM`, `INVOICE_PAYMENT` entities.
- Supports detailed line items (`INVOICE_LINE_ITEM`).
- Handles Tax calculations (rudimentary) and Subtotal/Total logic.

## Workflow

1.  **Draft**: Create invoice with line items.
2.  **Issue**: Approve invoice.
    *   **GL Posting**: Debit AR, Credit Revenue.
3.  **Payment**: Record payment.
    *   **GL Posting**: Debit Cash, Credit AR.

## Responsibilities

- **Line Item Management**: Tracking individual goods/services sold.
- **Tax Handling**: Storing calculated tax amounts.
- **Lifecycle Management**: Draft -> Issued -> Paid/Partially Paid.
