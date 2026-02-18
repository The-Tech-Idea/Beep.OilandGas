# PurchaseOrderService

## Overview

The `PurchaseOrderService` handles the procurement lifecycle. It works upstream of `APInvoiceService`.

## Workflow

1.  **Creation**: Create PO (DRAFT).
2.  **Approval**: Manager approves PO (APPROVED).
3.  **Receipt**: Goods arrive (PO_RECEIPT).
    *   *Note: Currently `PurchaseOrderService` does not auto-post GL entries for Accrued Liability upon receipt. This is a potential enhancement area (GR/IR).*
4.  **Closing**: PO marked as CLOSED.

## Key Methods

### `CreatePOAsync`
Creates a purchase order header.

### `ReceiveGoodsAsync`
Records a physical receipt of goods (`PO_RECEIPT`). This tracks quantity received vs ordered.

### `ClosePOAsync`
Finalizes the PO lifecycle.

## Integration
- **AP Invoice**: Typically, a Vendor Bill (`APInvoiceService`) is created *after* Goods Receipt, matching the PO number.
