# TODO - ProductionAccounting Services

## Critical (correctness)
- [x] Add missing entity properties via partial extensions:
  - `MEASUREMENT_RECORD`: `WELL_ID`, `LEASE_ID`, `PROPERTY_ID`, `RUN_TICKET_ID`, `TANK_BATTERY_ID`
  - `IMBALANCE_ADJUSTMENT`: `PROPERTY_OR_LEASE_ID`
- [x] Fix `JournalEntryService.CreateEntryAsync` to create balanced entries and write `GL_ENTRY` rows.
- [x] Fix `JournalEntryService.GetAccountBalanceAsync` to calculate from `GL_ENTRY` for the requested account.

## High (behavior)
- [ ] Apply `start`/`end` date filters in:
  - `MeasurementService.GetByWellAsync`, `MeasurementService.GetByLeaseAsync`
  - `PricingService.GetHistoryAsync`
- [ ] Normalize constants:
  - Move cost type/category constants to `Beep.OilandGas.ProductionAccounting.Constants`.
  - Replace duplicated method constants with clear names (`AmortizationMethods`, etc.).

## Medium (robustness)
- [ ] Ensure all `Get*Async` methods always filter `ACTIVE_IND = 'Y'` consistently.
- [ ] Standardize "connection name defaulting" pattern (use `PPDM39` consistently).
- [ ] Improve exception consistency (wrap unexpected exceptions into `ProductionAccountingException`).

## Nice-to-have (future)
- [ ] Add optional adapter layer to delegate traditional accounting workflows to `Beep.OilandGas.Accounting.Services`.
- [ ] Add service-level unit tests (where the repo has a test harness).

## Doc Gaps (from GAP_MATRIX.md)
- [ ] Implement missing monitoring/exception management service (best practices).
- [ ] Implement partner/JV cash calls and partner statements service.
- [ ] Fill null-return paths in:
  - `DecommissioningService`
  - `LeasingService`
  - `CopasOverheadService`
  - `InventoryLcmService`
  - `TakeOrPayService`
  - `ProductionTaxService`
