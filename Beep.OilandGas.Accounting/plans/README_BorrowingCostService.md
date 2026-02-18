# BorrowingCostService

## Overview

The `BorrowingCostService` manages interest and other borrowing costs in accordance with **IAS 23**. It determines whether borrowing costs must be capitalized as part of the cost of a qualifying asset or expensed.

## Key Functionality

- **Capitalization**: Adds borrowing costs to the asset's value during its construction period.
  - GL: Debit Asset (WIP/Construction in Progress), Credit Interest Payable.
- **Expensing**: Recognizes interest as an expense for non-qualifying assets or periods outside active construction.
  - GL: Debit Interest Expense, Credit Interest Payable.

## Key Methods

### `RecordBorrowingCostAsync`
Records the cost and directs it to the appropriate account (Asset or Expense) based on the `capitalize` flag.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
