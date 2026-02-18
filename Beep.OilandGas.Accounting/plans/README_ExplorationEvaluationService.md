# ExplorationEvaluationService

## Overview

The `ExplorationEvaluationService` manages exploration and evaluation assets in accordance with **IFRS 6**. It is critical for the Oil & Gas industry, handling the capitalization of exploration costs pending determination of technical feasibility and commercial viability.

## Key Functionality

- **Capitalization**: Records exploration costs as an asset (E&E Asset) when the "successful efforts" or "full cost" method allows.
  - GL: Debit Exploration Asset, Credit Cash/AP.
- **Expensing**: immediately expenses costs that do not qualify for capitalization (e.g., pre-license costs, or dry holes under successful efforts).
  - GL: Debit Exploration Expense, Credit Cash/AP.

## Key Methods

### `CapitalizeExplorationCostAsync`
Records a cost as an asset.

### `ExpenseExplorationCostAsync`
Records a cost directly to the P&L.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
