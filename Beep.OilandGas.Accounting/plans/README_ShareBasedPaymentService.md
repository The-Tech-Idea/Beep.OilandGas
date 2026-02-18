# ShareBasedPaymentService

## Overview

The `ShareBasedPaymentService` handles the accounting for share-based payment transactions (e.g., stock options, RSUs) under **IFRS 2**.

## Key Functionality

- **Expense Recognition**: Recognizes the fair value of goods or services received in exchange for equity instruments.
  - GL: Debit Share-Based Compensation Expense, Credit Equity Reserve (APIC).

## Key Methods

### `RecordShareBasedExpenseAsync`
Records the compensation expense.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
