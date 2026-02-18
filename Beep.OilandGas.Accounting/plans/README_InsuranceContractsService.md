# InsuranceContractsService

## Overview

The `InsuranceContractsService` manages insurance contracts under **IFRS 17**. It handles the recognition of insurance liabilities, premiums, claims, and the Contractual Service Margin (CSM).

## Key Functionality

- **Inception**: Records initial liability and CSM.
- **Premium Receipt**: Records cash inflow and liability update.
- **Revenue Recognition**: Allocates revenue over the coverage period.
- **Claims**: Records insurance service expenses.
- **CSM Adjustments**: Adjusts the unearned profit margin.
- **Finance Expense**: Accretes interest on the liability.

## Key Methods

### `RecordContractInceptionAsync`
Initial recognition.

### `RecordPremiumReceiptAsync`
Premium collection.

### `RecognizeInsuranceRevenueAsync`
Revenue recognition.

### `RecordClaimExpenseAsync`
Claim incurrence.

### `AdjustContractualServiceMarginAsync`
CSM adjustment.

### `RecordInsuranceFinanceExpenseAsync`
Finance cost recognition.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
