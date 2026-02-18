# RetirementBenefitPlanService

## Overview

The `RetirementBenefitPlanService` manages post-employment benefits (pensions) under **IAS 26**. It handles contributions to the plan and benefit payments from the plan.

## Key Functionality

- **Contributions**: Records cash paid into the retirement plan.
  - GL: Debit Retirement Plan Asset, Credit Cash.
- **Benefit Payments**: Records payments made to retirees.
  - GL: Debit Benefit Liability, Credit Cash.

## Key Methods

### `RecordPlanContributionAsync`
Records funding of the plan.

### `RecordBenefitPaymentAsync`
Records distribution from the plan.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
