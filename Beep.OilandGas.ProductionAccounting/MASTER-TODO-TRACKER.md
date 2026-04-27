# ProductionAccounting Master Tracker

## Phase Rollup

- [x] Phase 0 - Baseline context + inventory
- [x] Phase 0.1 - Behavior map (implemented vs compatibility vs placeholder)
- [x] Phase 1 - Interface and API surface normalization plan
- [x] Phase 2 - Data access + seed strategy definition
- [ ] Phase 3 - Service hardening and consistency implementation
- [ ] Phase 4 - API alignment and DI cleanup
- [ ] Phase 5 - Tests, docs, and cleanup

## Current Snapshot

- `ProductionAccountingService` is the orchestrator with real cross-service workflow (`run ticket -> allocation -> royalty -> revenue -> GL -> close`).
- `ProductionAccountingService.ControllerFacade` exposes many compatibility managers with mixed persistence and in-memory fallbacks.
- `ProductionAccountingModuleSetup` now seeds project-owned production-accounting reference sets through `R_PRODUCTION_ACCOUNTING_REFERENCE_CODE`.
- API usage is currently concentrated in `ApiService/Controllers/Field/AccountingController.cs` plus accounting-domain controllers.
- There is strong functional breadth but uneven operational maturity across validation, cancellation, and persistence consistency.

## Active TODOs

- [x] Create initial phased planning docs in `.plans`.
- [ ] Define canonical repository usage rules for orchestrator and compatibility surfaces.
- [x] Define reference-value strategy (`R_`/`RA_`) and add missing reference tables + seed scope map.
- [x] Normalize interface ownership (`Models.Core.Interfaces` vs local/core interfaces) and controller dependencies.
- [x] Add parity tests for field accounting endpoints wired to `ProductionAccountingService`.
- [ ] Add focused service tests for `ProcessProductionCycleAsync` guard/failure paths.
- [ ] Document staged vs active capabilities in API reference and project plans.

## Verification Criteria

- Build compiles with no new `ProductionAccounting` warnings/errors.
- Module setup remains idempotent and safe to re-run.
- Each active API area has at least one happy path and one guard/failure test.
- Compatibility layers are clearly marked as active, staged, or fallback-only.

*Last updated: 2026-04-27*

