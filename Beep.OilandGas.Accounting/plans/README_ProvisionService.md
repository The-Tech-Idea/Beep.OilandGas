# ProvisionService

## Overview

The `ProvisionService` manages provisions under **IAS 37**, with a specific focus on Asset Retirement Obligations (ARO). It handles initial recognition, estimated cost updates, and accretion expense.

## Key Functionality

- **Initial Recognition**: Records the ARO liability and the corresponding asset cost.
  - GL: Debit Asset Retirement Cost, Credit Asset Retirement Obligation.
- **Estimate Updates**: Adjusts the liability and asset for changes in estimates or discount rates.
- **Accretion**: Unwinds the discount on the liability over time.
  - GL: Debit Accretion Expense, Credit Asset Retirement Obligation.

## Key Methods

### `RecordAssetRetirementObligationAsync`
Initial setup of an ARO.

### `UpdateAssetRetirementEstimateAsync`
Adjusts an existing ARO.

### `RecordAccretionAsync`
Records periodic accretion expense.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
- `ASSET_RETIREMENT_OBLIGATION` table
