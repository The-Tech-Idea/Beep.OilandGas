# Decommissioning ModuleSetup Plan

## Current state
- ModuleSetupBase implementation exists in `DecommissioningModule`.
- Project contains clear Data/Tables, Data/Contracts, Data/Projections segmentation.

## Target state
- Add DecommissioningModule : ModuleSetupBase for project-owned schema setup.
- Register only Decommissioning Data/Tables in EntityTypes.
- SeedAsync only for decommissioning workflow defaults.

## Phase tasks
- [x] Phase 1: define module ownership and ModuleId/Order.
- [x] Phase 2: confirm table inventory from Data/Tables.
- [x] Phase 3: implement module and local EntityTypes.
- [x] Phase 4: add idempotent decommissioning seed defaults.
- [x] Phase 5: build and validate setup orchestration.
- [ ] Phase 6: document boundaries and review checks.

## Audit snapshot
- Local table ownership currently registered: 2 entity types (`DECOMMISSIONING_STATUS`, `ABANDONMENT_STATUS`).
