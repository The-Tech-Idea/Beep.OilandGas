# InventoryService

## Overview

The `InventoryService` manages the lifecycle of inventory items, including creation, stock receipt, and usage.

## Responsibilities

- **Master Data**: CRUD for `INVENTORY_ITEM`.
- **Stock Movements**:
  - **Receipt**: Increases stock. Posts GL (Audit/AP).
  - **Usage/Sale**: Decreases stock. Posts GL (COGS/Inventory).
- **Valuation**: Maintains weighted average cost.

## Key Methods

### `ReceiveStockAsync`
Records the arrival of goods.
*   **GL Impact**: Debit Inventory (1300), Credit AP (2000).
*   **Costing**: Updates Weighted Average Cost.

### `UseStockAsync`
Records the consumption or sale of goods.
*   **GL Impact**: Debit COGS (5000), Credit Inventory (1300).
*   **Validation**: Ensures sufficient stock exists.

### `GetItemTransactionsAsync`
Returns audit trail of all movements for an item.

## Data Models
- `INVENTORY_ITEM`
- `INVENTORY_TRANSACTION`
