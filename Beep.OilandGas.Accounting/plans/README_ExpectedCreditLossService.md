# ExpectedCreditLossService

## Overview

The `ExpectedCreditLossService` implements the **Expected Credit Loss (ECL)** model under **IFRS 9**. It calculates and records loss allowances for financial assets.

## Key Functionality

- **ECL Provisioning**: Recognizes credit losses based on 12-month or lifetime expected losses depending on credit risk staging.
  - GL: Debit ECL Expense, Credit Loss Allowance.

## Key Methods

### `RecordExpectedCreditLossAsync`
Records the provision or reversal of credit losses.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
