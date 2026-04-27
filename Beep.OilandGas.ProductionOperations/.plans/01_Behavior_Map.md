# ProductionOperations Behavior Map

## Service Surface vs Actual Behavior

## 1) `ProductionOperationsService` (`PPDMGenericRepository` path)

### Implemented with real persistence

- Well production:
  - `RecordWellProductionAsync`
  - `GetWellProductionAsync`
  - `CalculateWellUptimeAsync` (computed from production rows)
  - `GetWellStatusAsync` (reads `WELL`)
- Equipment maintenance:
  - `RecordEquipmentMaintenanceAsync`
  - `GetEquipmentMaintenanceHistoryAsync`
  - `ScheduleMaintenanceAsync`
  - `GetUpcomingMaintenanceAsync`
- Facility operations (delegated to facility service for core persistence):
  - `RecordFacilityProductionAsync`
  - `GetFacilityProductionAsync`
  - `UpdateFacilityStatusAsync`
  - `GetFacilityStatusAsync`
- Production cost row operations:
  - `CreateOperationAsync`
  - `GetOperationStatusAsync`
  - `UpdateOperationAsync`
- Optimization analysis helper:
  - `OptimizeProductionAsync` (heuristic based on recent production history)

### Placeholder/stub-like behavior

- `RecordOperationalCostsAsync` (no persistence)
- `GetOperationalCostsAsync` (returns empty list)
- `CalculateCostAnalysisAsync` (returns zeroed/default object)
- `GenerateOperationsReportAsync` (returns scaffold/default report)
- `IdentifyOptimizationOpportunitiesAsync` (returns empty list)
- `ImplementOptimizationAsync` (no persistence/effect)
- `MonitorOptimizationEffectivenessAsync` (synthetic/default values)
- `GetProductionOperationsSummaryAsync` (synthetic/default values)
- `ExportOperationsDataAsync` (returns empty bytes)
- `ValidateOperationsDataAsync` (returns optimistic default)
- `CalculateEquipmentReliabilityAsync` (synthetic/default values)

## 2) `ProductionManagementService` (`UnitOfWork` path)

### Implemented

- `GetProductionOperationsAsync`
- `GetProductionOperationAsync`
- `CreateProductionOperationAsync`
- `GetProductionReportsAsync`
- `GetWellOperationsAsync`
- `GetFacilityOperationsAsync`
- `ListFacilityPdenDeclarationsAsync` (partial in `.PdenFacilityQueries.cs`)

### Observations

- Uses UnitOfWork and `AppFilter`, not `PPDMGenericRepository`.
- Overlaps conceptually with parts of `ProductionOperationsService` but at different abstraction level and table focus (`PDEN`, `FACILITY`).

## 3) `FacilityManagementService` (PPDM facility lifecycle)

### Implemented with persistence

- Facility master:
  - list/get/create/update
  - optional `FACILITY_FIELD` link on create
- Facility class/component/status/rate:
  - list/add operations
- Equipment links:
  - list/link
- Maintenance:
  - list/create
- Work orders:
  - list/create + linking through `WORK_ORDER_COMPONENT`
- Facility PDEN lifecycle:
  - ensure PDEN (`PDEN` + `PDEN_FACILITY`)
  - list/record production volumes (`PDEN_VOL_SUMMARY`)
- Licenses:
  - list/create
  - active-license check
- Reliability aggregate:
  - simple aggregate from maintenance + work-order counts

### Behavior notes

- Status insertion enforces active-license check for operational statuses (configurable via method parameter).
- Most methods follow consistent `PPDMGenericRepository` + common column handler pattern.

## 4) API consumption map

- `ProductionOperationsController`
  - Uses `Models.Core.Interfaces.IProductionOperationsService` + `IProductionManagementService`.
  - Contains compatibility endpoints that bridge legacy DTOs (`PRODUCTION_ALLOCATION`, `ProductionOperation`) and newer service calls.
- `FacilityProductionController`
  - Uses `IFacilityManagementService` for facility PDEN and volume workflows.
- `ProductionController` (field-scoped variants)
  - Primarily uses lifecycle `PPDMProductionService` (separate from ProductionOperations services).

## 5) Boundary and risk summary

- Interface overlap exists:
  - broad DTO-heavy `IProductionOperationsService` vs narrower PPDM-centric `IProductionManagementService` and `IFacilityManagementService`.
- Data-access strategy overlap:
  - `PPDMGenericRepository` (operations/facility) and `UnitOfWork` (management) both active.
- Not all exposed operations are production-grade:
  - several methods return synthetic or placeholder values but are reachable through interfaces/controllers.

## 6) Planning implication

- Phase 1 should explicitly mark canonical boundaries and stage non-implemented methods out of active API surface (or annotate as preview/internal).
- Phase 2 should decide and document per-workflow data access pattern.
- Phase 5 should prioritize tests only for implemented flows first; avoid locking placeholder behavior unless intentionally part of contract.

