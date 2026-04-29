# ProductionAccounting Execution Plan

## Phase 0 - Baseline and Mapping (completed)

- [x] Inventory key services/interfaces/module setup.
- [x] Build behavior map for implemented vs compatibility vs staged surfaces.
- [x] Capture initial interface status map.

## Phase 1 - Interface and API Surface Normalization

### Goals

- Align controllers and service dependencies to canonical interfaces.
- Keep compatibility surfaces explicit and non-ambiguous.

### TODO Checklist

- [x] Decide whether `GetRevenueTransactionsAsync` belongs in canonical interface or a dedicated query interface.
- [x] Update accounting field controller dependencies to interface-first injection where feasible.
- [x] Mark compatibility-only entry points in service XML docs and planning docs (`ProductionAccountingService.ControllerFacade` manager/facade summaries).
- [x] Update API reference docs to separate active vs compatibility accounting endpoints.

### Verification

- [x] `ApiService` builds with updated DI/service registrations.
- [x] No controller depends on unstable/staged methods without explicit compatibility labeling.

## Phase 2 - Data Access + Seed Convergence

### Goals

- Standardize repository behavior and introduce reference-data seeding policy.

### TODO Checklist

- [x] Add reference seed sets to `ProductionAccountingModuleSetup` using PPDM `R_`/`RA_` tables where available.
- [x] Create project-owned reference table(s) only for missing PPDM domains.
- [x] Add a seed scope comment block in module setup documenting `Tables`, `Projections`, `Core`.
- [ ] Reduce sync-over-async repository calls in compatibility manager paths.

### Verification

- [ ] Module setup executes repeatedly without duplicate/unsafe side effects.
- [x] Seeded values are discoverable from accounting service/API paths.

## Phase 3 - Service Hardening

### Goals

- Make orchestration and compatibility behavior deterministic, testable, and explicit.

### TODO Checklist

- [x] Add guard/failure tests for `ProcessProductionCycleAsync` (measurement/allocation/royalty/revenue/GL fail branches).
- [x] Add tests for accounting summary and period close behavior.
- [x] Replace no-op compatibility methods or mark as staged with explicit contract behavior.
- [ ] Ensure cancellation and logging consistency for long-running service paths.

### Verification

- [x] New tests pass in `ApiService.Tests` and/or project test suites.
- [x] No silent failure paths remain without logging and status contracts.

## Phase 4 - Documentation and Final Cleanup

### TODO Checklist

- [x] Refresh `.plans` status after implementation phases.
- [x] Update `MASTER-TODO-TRACKER.md` phase rollup/checklists.
- [x] Update `.cursor/commands/api-endpoints-reference.md` accounting section with active/compatibility split.

### Verification

- [x] `dotnet test Beep.OilandGas.ApiService.Tests\Beep.OilandGas.ApiService.Tests.csproj` passes after hardening/test additions.
- [x] `dotnet build Beep.OilandGas.ApiService\Beep.OilandGas.ApiService.csproj --no-restore` succeeds with 0 warnings/0 errors.

