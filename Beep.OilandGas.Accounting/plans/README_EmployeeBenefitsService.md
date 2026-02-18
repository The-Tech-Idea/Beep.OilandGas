# EmployeeBenefitsService

## Overview

The `EmployeeBenefitsService` manages short-term and long-term employee benefits (excluding pensions and share-based payments) under **IAS 19**.

## Key Functionality

- **Accruals**: Records the expense and liability for benefits earned but not yet paid (e.g., bonuses, vacation pay).
  - GL: Debit Benefit Expense, Credit Benefit Liability.
- **Payments**: Records the settlement of the liability.
  - GL: Debit Benefit Liability, Credit Cash.

## Key Methods

### `RecordBenefitAccrualAsync`
Posts the accrual entry.

### `RecordBenefitPaymentAsync`
Posts the payment entry.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
