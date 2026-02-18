# FinancialStatementService

## Overview

The `FinancialStatementService` is the engine for generating the "Big Three" financial statements: **Income Statement (P&L)**, **Balance Sheet**, and **Cash Flow Statement**. It aggregates GL data into standard financial reporting formats.

## Key Methods

### `GenerateIncomeStatementAsync`
Calculates Net Income by aggregating:
- `REVENUE`
- `COGS` (Cost of Goods Sold)
- `EXPENSE` (Operating Expenses)
- `OTHER_INCOME`/`OTHER_EXPENSE`

### `GenerateBalanceSheetAsync`
Produces the Statement of Financial Position at a specific point in time.
- Categorizes **Assets** (Current, Fixed, Other).
- Categorizes **Liabilities** (Current, Long-Term).
- Categorizes **Equity**.
- Validates that Assets = Liabilities + Equity.

### `GenerateCashFlowStatementAsync`
(Note: Distinct from `CashFlowService`). This method attempts to generate a cash flow statement based on *period changes* in account balances (Indirect Method), whereas `CashFlowService` might use a different approach (Direct Method or Transaction Analysis). *Clarification needed on which is the primary source for reporting.*

## Dependencies
- `TrialBalanceService`: Used as the source of truth for all account balances.
