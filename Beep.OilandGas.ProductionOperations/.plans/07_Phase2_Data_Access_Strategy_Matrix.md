# Phase 2 Data Access Strategy Matrix

## Goal

Make data-access ownership explicit and consistent by workflow area, while minimizing risky refactors in one step.

## Canonical Strategy by Workflow

- `Facility lifecycle + monitoring` -> `PPDMGenericRepository` via `IFacilityManagementService`
  - includes facility master, equipment links, maintenance, status, licensing, work orders, production volumes, monitoring measurements/activity
  - rationale: already standardized with defaults/common-column handler and active API usage

- `Production operations core (core interface + compatibility cost/production endpoints)` -> `PPDMGenericRepository` via `ProductionOperationsService`
  - includes `Get/RecordProductionData`, operation cost create/get/update, optimization recommendation generation
  - rationale: API-facing service already uses this pattern and has current contract/tests

- `Legacy/compatibility PDEN browsing in management service` -> `UnitOfWork` (temporary hold)
  - includes `IProductionManagementService` methods (`GetProductionOperationsAsync`, `GetProductionReportsAsync`, `GetWellOperationsAsync`, etc.)
  - rationale: keep stable for now; migrate in controlled follow-up slice after parity checks

## Migration Sequence (low-risk)

1. Keep facility path on repository pattern (already done).
2. Keep API-facing production operations on repository pattern (already done).
3. Add parity tests for management queries vs expected behavior.
4. Migrate `ProductionManagementService` from `UnitOfWork` to `PPDMGenericRepository` behind same interface.
5. Remove stale `UnitOfWork` helpers from `ProductionManagementService`.

### Current implementation status

- Completed first migration slice:
- Completed convergence migration slice:
  - `GetProductionOperationsAsync` -> repository path
  - `GetProductionReportsAsync` -> repository path
  - `GetWellOperationsAsync` -> repository path
  - `GetFacilityOperationsAsync` -> repository path
  - `ListFacilityPdenDeclarationsAsync` -> repository path
  - `GetProductionOperationAsync` -> repository path
  - `CreateProductionOperationAsync` -> repository path
- Remaining cleanup:
  - remove stale UnitOfWork-specific helpers/comments from `ProductionManagementService`
  - add dedicated management-service parity tests for migrated methods

## Convergence Rules

- New production/facility features must use `PPDMGenericRepository` unless explicitly marked legacy.
- Any remaining `UnitOfWork` path must be documented in this matrix with an exit task.
- IDs and audit columns:
  - use `_defaults.FormatIdForTable(...)` for generated IDs
  - use `_commonColumnHandler.PrepareForInsert(...)` prior to inserts on PPDM entities

## Phase 2 Exit Criteria

- `IProductionManagementService` has no direct `UnitOfWorkFactory` usage.
- Equivalent query behavior is covered by tests.
- This matrix is reflected in service summaries/comments and tracker status.

