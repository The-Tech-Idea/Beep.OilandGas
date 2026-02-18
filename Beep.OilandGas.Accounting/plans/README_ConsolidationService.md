# ConsolidationService

## Overview

The `ConsolidationService` manages intercompany eliminations and adjustments required for preparing consolidated financial statements under **IFRS 10**.

## Key Functionality

- **Intercompany Eliminations**: Removes transactions between entities within the same group to prevent double-counting.
  - Payable/Receivable Elimination: Debit Payable, Credit Receivable.

## Key Methods

### `RecordIntercompanyEliminationAsync`
Posts the elimination entry to the GL.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
