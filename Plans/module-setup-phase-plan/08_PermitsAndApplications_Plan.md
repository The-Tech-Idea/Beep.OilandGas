# PermitsAndApplications ModuleSetup Plan

## Current state
- ModuleSetupBase implementation exists in `PermitsModule`.
- Domain has broad permit lifecycle data and jurisdiction configuration support.

## Target state
- Add PermitsModule : ModuleSetupBase for permits/compliance schema ownership.
- EntityTypes should include only project-owned table classes.
- SeedAsync should bootstrap permit workflow statuses, required-form defaults, and jurisdiction metadata owned by this project.

## Phase tasks
- [x] Phase 1: define ownership scope for permits setup.
- [x] Phase 2: classify all data classes into table/contract/projection.
- [x] Phase 3: implement module with local EntityTypes only.
- [x] Phase 4: refactor local seed bootstraps to idempotent inserts.
- [x] Phase 5: build and integration validation.
- [ ] Phase 6: add governance checks to avoid PPDM leakage.

## Audit snapshot
- Local table ownership currently registered: 17 entity types under `Data/PermitsAndApplications/Tables`.
