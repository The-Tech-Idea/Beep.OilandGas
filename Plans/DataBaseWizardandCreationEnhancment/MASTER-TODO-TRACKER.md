# MASTER TODO TRACKER - PPDM39 Setup, Consolidation, and MigrationManager

## Scope
- Consolidate duplicate PPDM39 contracts and ownership between `Beep.OilandGas.PPDM39` and `Beep.OilandGas.PPDM39.DataManagement`.
- Make MigrationManager the only schema create/update authority for setup and demo paths.
- Keep compatibility routes until canonical wizard migration is complete.

## Phase Status
- [ ] Phase 01 - Setup Surface And Contracts
- [ ] Phase 02 - Connection And Environment Foundation
- [ ] Phase 03 - Migration Manager Schema Creation
- [ ] Phase 04 - Wizard Orchestration And UX
- [ ] Phase 05 - Demo, Seed, And LocalDB Lifecycle
- [ ] Phase 06 - Validation, Governance, And Rollout

## Cross-Phase Critical Findings To Resolve
- [ ] Remove contract duplication drift (`IPPDMRepository`, metadata/defaults/tree contracts, DTO placement).
- [ ] Remove script-first schema assumptions from demo/setup orchestration.
- [ ] Eliminate sync-over-async hotspots (`GetAwaiter().GetResult()`) in runtime setup/data paths.
- [ ] Replace non-refreshable metadata cache behavior in `PPDMMetadataService`.
- [ ] Reduce repeated `Type.GetType` and repeated `new PPDMGenericRepository(...)` patterns.

## Deliverable Ledger
- [ ] Canonical setup contract set under `Beep.OilandGas.Models/Data/PPDM39Setup`.
- [ ] Canonical schema orchestration service with plan/policy/preflight/approve/execute/resume.
- [ ] Canonical wizard route and compatibility redirect map.
- [ ] Demo/seed/localdb lifecycle policy integrated with migration orchestration.
- [ ] CI and operations artifact model with plan hash and approval provenance.

## Dependency Order
1. Phase 01 must complete before endpoint and wizard stabilization.
2. Phase 02 must complete before production-safe phase 03 orchestration.
3. Phase 03 must complete before phase 05 demo lifecycle migration off scripts.
4. Phase 04 depends on finalized contracts and orchestration endpoints.
5. Phase 06 validates all prior phases with governance and CI gates.

## Definition Of Done
- [ ] No setup path creates schema outside MigrationManager orchestration.
- [ ] All setup and wizard payloads use typed shared contracts.
- [ ] No uncontrolled contract drift between PPDM39 and DataManagement.
- [ ] Setup operations produce auditable artifacts with masked sensitive values.
