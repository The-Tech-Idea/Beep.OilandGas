# HSE ModuleSetup Plan

## Current state
- Module file: Beep.OilandGas.HSE/Modules/HseModule.cs
- ModuleSetupBase exists with intentionally empty EntityTypes.

## Target state
- HseModule should register only HSE project-owned table classes when available.
- Keep shared PPDM reference seeding in PPDM39.DataManagement.

## Phase tasks
- [x] Phase 1: define HSE schema ownership list.
- [x] Phase 2: classify HSE data classes into Tables/Contracts/Projections.
- [x] Phase 3: confirm no local tables exist and keep EntityTypes empty.
- [x] Phase 4: keep seed scope to HSE-owned defaults.
- [x] Phase 5: build and validate setup path.
- [ ] Phase 6: enforce no PPDM39.Models table leakage.

## Audit snapshot
- No local persisted table classes found in project.
- Empty EntityTypes is expected and now explicitly documented.
