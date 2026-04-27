# ProductionOperations Execution Plan

## Phase 0 - Baseline (completed)

- [x] Inventory services, interfaces, and module setup.
- [x] Capture current architecture and risks in `00_Context_Map.md`.
- [x] Create project-level `MASTER-TODO-TRACKER.md`.
- [x] Build class-level implementation map in `01_Behavior_Map.md` (implemented vs placeholder vs overlap).

## Phase 1 - Interface/Surface Normalization

### Goal
Reduce overlap and make ownership explicit.

### Target Files

- `Services/IProductionOperationsService.cs`
- `Services/IProductionManagementService.cs`
- `Services/IFacilityManagementService.cs`
- `Services/ProductionOperationsService*.cs`

### Tasks

- [x] Decide canonical boundary for production operations facade vs specialized services (documented in `02_Interface_Status_Map.md`).
- [x] Classify each `IProductionOperationsService` method as `implemented`, `placeholder`, or `defer` in `02_Interface_Status_Map.md`.
- [x] Align controller exposure and API references with the classification map.
- [x] Mark non-implemented methods with explicit staged status (documented in `Services/IProductionOperationsService.cs`).
- [x] Ensure one file path per interface/type (verified in `03_Canonical_Type_File_Map.md`; no duplicate physical files found).

### Verification

- [x] Interface contracts map 1:1 to implementation ownership.
- [ ] No duplicate interface/type definitions remain.

## Phase 2 - Data Access Convergence

### Goal
Use consistent, documented data access patterns.

### Target Files

- `Services/ProductionOperationsService.cs`
- `Services/ProductionManagementService.cs`
- `Services/ProductionManagementService.PdenFacilityQueries.cs`

### Tasks

- [x] Decide repository strategy by service area (`PPDMGenericRepository` or UnitOfWork wrapper) in `07_Phase2_Data_Access_Strategy_Matrix.md`.
- [x] Keep `FacilityManagementService` on `PPDMGenericRepository` pattern and document as canonical for facility lifecycle paths.
- [x] Refactor inconsistencies and document chosen pattern in code comments (service summaries updated for canonical vs legacy paths).
- [ ] Ensure ID formatting and audit-column handling are consistent.

### Verification

- [ ] CRUD/query paths behave consistently for equivalent operations.
- [ ] No accidental mixed strategy in same workflow path.

## Phase 3 - Module Seed Expansion

### Goal
Support facility workflows with required reference data.

### Target Files

- `Modules/FacilityManagementModuleSetup.cs`

### Tasks

- [ ] Add idempotent seed sets for required facility `R_*` tables used by status/license/maintenance flows.
- [ ] Keep seeding incremental and safe for re-run.
- [ ] Add concise `SeedScope` comment block for maintainers.

### Verification

- [ ] Seeding executes without duplicate-row explosions.
- [ ] Facility workflows can resolve required reference values.

## Phase 4 - API/DI Alignment

### Goal
Ensure API consumes updated services correctly.

### Target Files

- `Beep.OilandGas.ApiService/Program.cs`
- Relevant production/facility controllers in `Beep.OilandGas.ApiService`

### Tasks

- [ ] Validate DI registration order and factory wiring for production/facility services.
- [ ] Confirm controllers depend on intended canonical interfaces.
- [ ] Remove/avoid dead registrations.

### Verification

- [ ] API starts and resolves production/facility services without runtime DI errors.

## Phase 5 - Tests and Cleanup

### Goal
Lock behavior and remove stale paths.

### Target Files

- `Beep.OilandGas.ApiService.Tests/*Production*`
- `Beep.OilandGas.ProductionOperations/*Tests*` (if present/new)

### Tasks

- [ ] Add focused tests for implemented production/facility happy paths.
- [ ] Add explicit tests for compatibility endpoints (`ProductionOperationsController`) to confirm translation behavior.
- [ ] Add guard/failure tests for field/facility resolution and status/license rules.
- [ ] Prune deprecated placeholders once replacements are in place.

### Verification

- [ ] Targeted tests pass.
- [ ] No new regressions in existing API tests.

