# Phase 1 Interface Status Map

## Canonical Intent

- Keep `Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService` as the API-facing core interface for currently live operations.
- Treat `Beep.OilandGas.ProductionOperations.Services.IProductionOperationsService` as a legacy/expanded local surface that contains both live and staged members.
- Use `IProductionManagementService` and `IFacilityManagementService` for specialized workflows that are not represented in the core interface.

## Method Status Classification

## A) `Models.Core.Interfaces.IProductionOperationsService` (API-facing)

- `GetProductionDataAsync` -> `implemented`
- `RecordProductionDataAsync` -> `implemented`
- `CreateOperationAsync` -> `implemented`
- `GetOperationStatusAsync` -> `implemented`
- `UpdateOperationAsync` -> `implemented`
- `OptimizeProductionAsync` -> `implemented` (heuristic recommendation engine)

## B) `ProductionOperations.Services.IProductionOperationsService` (expanded/local)

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

### Placeholder / staged (keep out of active API promotion)

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
- No new controller action is added for a `placeholder` method without an explicit staged marker.

