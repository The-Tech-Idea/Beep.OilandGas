# CeclService

## Overview

The `CeclService` implements the **Current Expected Credit Losses (CECL)** methodology under **ASC 326** (US GAAP). It estimates and records lifetime expected credit losses for financial assets.

## Key Functionality

- **Allowance Adjustment**: Records increases or decreases to the CECL allowance.
  - Increase (Bad Debt Expense): Debit CECL Expense, Credit CECL Allowance.
  - Decrease (Recovery): Debit CECL Allowance, Credit CECL Expense.
- **GAAP Specific**: This service is specifically tailored for US GAAP requirements.

## Key Methods

### `RecordCeclAdjustmentAsync`
Posts the allowance adjustment entry.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
