# FairValueMeasurementService

## Overview

The `FairValueMeasurementService` supports **IFRS 13** fair value measurement and disclosure. It handles the recording of fair value changes regardless of the specific asset type, ensuring consistency.

## Key Functionality

- **Valuation Recording**: Records gains or losses from fair value changes.
- **Hierarchy Support**: Captures the Fair Value Hierarchy Level (1, 2, or 3) for disclosure purposes.

## Key Methods

### `RecordFairValueChangeAsync`
Generic method to record a fair value adjustment for any asset account.

## Dependencies
- `AccountingBasisPostingService`
- `IAccountMappingService`
