# Phase 1 Interface Status Map

## Canonical Intent

- Keep `Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService` as the API-facing core interface for currently live operations.
- Keep `Beep.OilandGas.ProductionOperations.Services.IProductionOperationsService` as the canonical local surface for live members only.
- Isolate staged methods under `Beep.OilandGas.ProductionOperations.Services.IProductionOperationsAdvancedService`.
- Use `IProductionManagementService` and `IFacilityManagementService` for specialized workflows that are not represented in the core interface.

## Method Status Classification

## A) `Models.Core.Interfaces.IProductionOperationsService` (API-facing)

- `GetProductionDataAsync` -> `implemented`
- `RecordProductionDataAsync` -> `implemented`
- `CreateOperationAsync` -> `implemented`
- `GetOperationStatusAsync` -> `implemented`
- `UpdateOperationAsync` -> `implemented`
- `OptimizeProductionAsync` -> `implemented` (heuristic recommendation engine)

## B) `ProductionOperations.Services.IProductionOperationsService` (canonical local)

### Implemented

- `RecordWellProductionAsync`
- `GetWellProductionAsync`
- `CalculateWellUptimeAsync`
- `GetWellStatusAsync`
- `UpdateWellParametersAsync` (limited behavior; currently does not persist parameter model fields)
- `RecordEquipmentMaintenanceAsync`
- `GetEquipmentMaintenanceHistoryAsync`
- `ScheduleMaintenanceAsync`
- `GetUpcomingMaintenanceAsync`
- `RecordFacilityProductionAsync`
- `GetFacilityProductionAsync`
- `UpdateFacilityStatusAsync`
- `GetFacilityStatusAsync`
- `OptimizeProductionAsync` (well-oriented heuristic logic)
- `CreateOperationAsync`
- `GetOperationStatusAsync`
- `UpdateOperationAsync`

## C) `ProductionOperations.Services.IProductionOperationsAdvancedService` (staged)

### Placeholder / staged (not in canonical API promotion)

- `CalculateEquipmentReliabilityAsync`
- `RecordOperationalCostsAsync`
- `GetOperationalCostsAsync`
- `CalculateCostAnalysisAsync`
- `GenerateOperationsReportAsync`
- `IdentifyOptimizationOpportunitiesAsync`
- `ImplementOptimizationAsync`
- `MonitorOptimizationEffectivenessAsync`
- `GetProductionOperationsSummaryAsync`
- `ExportOperationsDataAsync`
- `ValidateOperationsDataAsync`

## Controller Exposure Alignment (Phase 1 target)

- `ProductionOperationsController` should continue exposing routes that map to API-facing core interface members.
- Any new route based on placeholder methods must be marked as `staged` and excluded from default production API references until implemented.
- Compatibility routes remain supported but should be tested as translation adapters, not as canonical business contracts.

## Definition of Done for Phase 1

- Every method in both interfaces has one of: `implemented`, `placeholder`, `deferred`.
- Trackers reference this map as the source-of-truth for endpoint exposure decisions.
- No new controller action is added for a staged method without explicit staged marker + `IProductionOperationsAdvancedService` usage.

## PPDM-First Mapping Matrix

- Canonical PPDM entities:
  - `PDEN`, `PDEN_FACILITY`, `PDEN_VOL_SUMMARY`
  - `FACILITY`, `FACILITY_STATUS`, `FACILITY_COMPONENT`, `FACILITY_RATE`
  - `FACILITY_EQUIPMENT`, `FACILITY_MAINTAIN`, `FACILITY_LICENSE`
  - `WORK_ORDER`, `WORK_ORDER_COMPONENT`, `CAT_EQUIPMENT`, `PRODUCTION_COSTS`
- Module extension entities:
  - `FACILITY_MEASUREMENT`, `FACILITY_EQUIPMENT_ACTIVITY`, `R_FACILITY_MONITORING_CODE`
- Compatibility adapter surface:
  - `IProductionManagementService` and legacy/compatibility controller routes.

