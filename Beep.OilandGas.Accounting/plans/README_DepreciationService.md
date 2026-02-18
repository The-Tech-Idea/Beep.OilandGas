# DepreciationService

## Overview

The `DepreciationService` calculates and manages depreciation schedules for fixed assets. It supports multiple methods including Straight-Line, Double Declining Balance, and MACRS.

## Functionality

- **Schedule Generation**: Automatically creates a full-life depreciation schedule upon asset creation.
- **Methods**:
  - `StraightLine`: Equal expense over useful life.
  - `DoubleDeclining`: Accelerated depreciation.
  - `MACRS`: Tax-focused accelerated method (Modified Accelerated Cost Recovery System).
  - `UnitsOfProduction`: (Placeholder) Based on usage.

## Key Methods

### `CreateFixedAssetAsync`
Initializes a new asset and generates its depreciation schedule.

### `GenerateDepreciationSummaryAsync`
Calculates total bookkeeping values (Cost, Accumulated Depreciation, Net Book Value) for the entire asset register.

## Models
- `FixedAsset`
- `DepreciationEntry`
