# InventoryLcmService

## Overview

The `InventoryLcmService` implements **IAS 2 / GAAP Lower of Cost or Market (LCM)** rules. It ensures inventory is not carried at a value higher than its Net Realizable Value (NRV).

## Functionality

- **Validation**: Compares current Weighted Average Cost vs. Market Price (from `PRICE_INDEX`).
- **Adjustment**: If Market < Cost, creates a write-down transaction.
- **GL Posting**: Debit COGS (Loss), Credit Inventory.

## Key Methods

### `ApplyLowerOfCostOrMarketAsync`
Evaluates a specific item for a specific date.
1.  Gets current cost (from `INVENTORY_VALUATION` or Item master).
2.  Calculates Market Value (Commodity Price - Transport - Quality Ded).
3.  If Market < Cost, posts write-down.

### `GetMarketValueAsync`
Helper to calculate NRV based on commodity prices.

## Dependencies
- `AccountingBasisPostingService` (for GL entries)
- `PRICE_INDEX` table (for market data)
