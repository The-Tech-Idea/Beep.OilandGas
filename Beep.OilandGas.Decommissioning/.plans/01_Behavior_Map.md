# Decommissioning Behavior Map

## Core Behaviors
- Abandon well by field with ownership validation.
- Decommission facility by field with ownership validation.
- Create/read environmental restoration records.
- Create/read decommissioning cost records.
- Estimate field-level decommissioning costs.

## WELL_STATUS Behavior Contract
- Status operations write `WELL_STATUS` with key parts:
  - `UWI`, `STATUS_TYPE`, `STATUS_ID`, `EFFECTIVE_DATE`
- `STATUS_TYPE` and `STATUS` are validated against:
  - `R_WELL_STATUS_TYPE`
  - `R_WELL_STATUS`
- Current status resolution is grouped by `STATUS_TYPE`, latest `EFFECTIVE_DATE` wins.

## Compatibility
- Optional-table fallback behavior remains explicit and deterministic.
- No PROJECT discriminator storage for decommissioning costs/restoration.
