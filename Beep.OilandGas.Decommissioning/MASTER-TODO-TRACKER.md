# Decommissioning Master Tracker

## Phase Rollup
- [x] Phase 0 - Planning baseline and tracker
- [x] Phase 1 - Canonical contracts and PPDM mapping
- [x] Phase 2 - Data-access canonicalization design
- [x] Phase 3 - Reference seed catalog and idempotent seeding
- [x] Phase 4 - Service/runtime path alignment and WELL_STATUS rules
- [x] Phase 5 - API/DI hardening
- [x] Phase 6 - Tests and verification hardening
- [x] Phase 7 - Rollout notes and exit reconciliation

## Active TODOs
- [x] Create module-local phased docs and verification gates.
- [x] Define PPDM-first canonical matrix including WELL_STATUS contracts.
- [x] Remove project-discriminator storage from decommissioning service paths.
- [x] Add idempotent decommissioning reference seed catalog.
- [x] Ensure decommissioning transitions write/read WELL_STATUS deterministically.
- [x] Add focused API + seed tests.

## Verification Criteria
- Canonical PPDM table strategy is documented and applied.
- No new manual SQL-script dependency was introduced.
- Decommissioning module setup seeding is idempotent.
- WELL_STATUS transitions are validated against reference tables and grouped by STATUS_TYPE.
- Decommissioning build/tests pass on changed surfaces.
- Full solution gate is green (`dotnet build Beep.OilandGas.sln`).
