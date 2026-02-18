# TaxCalculationService

## Overview

The `TaxCalculationService` performs calculations for income tax provisions and estimated payments. It handles the mathematical side of tax compliance, including effective rates, deductions, and credits.

## Key Functionality

- **Annual Provision**: Calculates Current and Deferred tax expense based on taxable income and effective rate.
- **Estimated Payments**: Calculates quarterly estimated tax payments.
- **Deferral Analysis**: Analyzes opportunities for tax deferral (e.g., 401(k), Depreciation).

## Key Methods

### `CalculateTaxProvisionAsync`
Calculates the total tax provision, considering:
- Taxable Income
- Deductions
- Credits
- Alternative Minimum Tax (AMT)

### `CalculateQuarterlyEstimatedTaxAsync`
Breaks down the annual liability into quarterly payments.

### `AnalyzeTaxDeferralOpportunitiesAsync`
Provides recommendations (e.g., Section 179) to reduce current tax liability.
