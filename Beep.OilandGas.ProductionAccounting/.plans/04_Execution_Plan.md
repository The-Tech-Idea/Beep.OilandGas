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
- [ ] Mark compatibility-only entry points in service XML docs and planning docs.
- [ ] Update API reference docs to separate active vs compatibility accounting endpoints.

### Verification

- [x] `ApiService` builds with updated DI/service registrations.
- [ ] No controller depends on unstable/staged methods without explicit compatibility labeling.

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
- [ ] Seeded values are discoverable from accounting service/API paths.

## Phase 3 - Service Hardening

### Goals

- Make orchestration and compatibility behavior deterministic, testable, and explicit.

### TODO Checklist

- [ ] Add guard/failure tests for `ProcessProductionCycleAsync` (measurement/allocation/royalty/revenue/GL fail branches).
- [ ] Add tests for accounting summary and period close behavior.
- [ ] Replace no-op compatibility methods or mark as staged with explicit contract behavior.
- [ ] Ensure cancellation and logging consistency for long-running service paths.

### Verification

- [ ] New tests pass in `ApiService.Tests` and/or project test suites.
- [ ] No silent failure paths remain without logging and status contracts.

## Phase 4 - Documentation and Final Cleanup

### TODO Checklist

- [ ] Refresh `.plans` status after implementation phases.
- [ ] Update `MASTER-TODO-TRACKER.md` phase rollup/checklists.
- [ ] Update `.cursor/commands/api-endpoints-reference.md` accounting section with active/compatibility split.

