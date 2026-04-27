# ProductionOperations Master Tracker

## Phase Rollup

- [x] Phase 0 - Baseline context + inventory
- [x] Phase 0.1 - Full behavior map (implemented vs placeholder vs overlap)
- [x] Phase 1 - Interface/surface normalization
- [x] Phase 2 - Data access convergence
- [ ] Phase 3 - Module seed expansion
- [ ] Phase 4 - API/DI alignment verification
- [ ] Phase 5 - Tests + cleanup

## Current Snapshot

- `IProductionOperationsService` contains broad workflow DTO-heavy surface with mixed implementation maturity.
- `IProductionManagementService` + `IFacilityManagementService` are PPDM-focused; facility service is largely implemented.
- `ProductionOperationsService` has real persistence for well/facility/core operations but keeps several placeholder methods for cost/reporting/export/validation flows.
- `FacilityManagementModuleSetup` now seeds `CAT_EQUIPMENT` plus monitoring reference sets for measurements and equipment activity.
- `.plans` now includes `01_Behavior_Map.md`, `02_Interface_Status_Map.md`, and `03_Canonical_Type_File_Map.md` for behavior, method status, and canonical type ownership.
- Added vertical monitoring slice (`FACILITY_MEASUREMENT`, `FACILITY_EQUIPMENT_ACTIVITY`, `R_FACILITY_MONITORING_CODE`) and API endpoints under `/api/facility/{facilityId}/monitoring`.

## Active TODOs

- [x] Decide canonical service boundary and capture it in `02_Interface_Status_Map.md`.
- [x] Align controller exposure and API references to the completed interface status map (`02_Interface_Status_Map.md`).
- [x] Verify canonical file ownership per type and confirm no duplicate physical files in `Services`.
- [x] Standardize repository strategy (`PPDMGenericRepository` vs UnitOfWork usage) by service.
- [x] Define and document Phase 2 data-access strategy matrix (`07_Phase2_Data_Access_Strategy_Matrix.md`).
- [ ] Add reference data seeding plan for facility `R_*` tables used by facility status/maintenance/license workflows.
- [x] Add vertical tracking support for facility measurements and equipment install/uninstall/move lifecycle.
- [x] Add focused API tests for facility monitoring endpoints (happy paths + guard/failure paths).
- [x] Migrate ProductionManagementService read/query methods to PPDMGenericRepository (first Phase 2 convergence slice).
- [x] Migrate remaining ProductionManagementService operation-by-id/create methods to PPDMGenericRepository.
- [x] Add focused tests for implemented flows before expanding additional methods.

## Verification Criteria

- Build compiles with no new warnings/errors from `Beep.OilandGas.ProductionOperations`.
- DI resolves all production/facility interfaces in `Beep.OilandGas.ApiService`.
- Seed phase is idempotent and safe on re-run.
- Tests cover at least one happy path + one guard/failure path per active service area.

*Last updated: 2026-04-27*
