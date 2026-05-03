# Phase 0 Context Map

## Objective
Establish current EconomicAnalysis architecture, persistence scope, and canonicalization gaps before implementation work.

## Current Module Topology
- Service implementation: `Beep.OilandGas.EconomicAnalysis/Services/EconomicAnalysisService.cs`
- Advanced service implementation: `Beep.OilandGas.EconomicAnalysis/Services/EconomicAnalysisService.Advanced.cs`
- Module setup: `Beep.OilandGas.EconomicAnalysis/Modules/EconomicsModule.cs`
- API controller: `Beep.OilandGas.ApiService/Controllers/Calculations/EconomicAnalysisController.cs`
- DI root: `Beep.OilandGas.ApiService/Program.cs`

## Current Persistence Behavior
- `ECONOMIC_ANALYSIS_RESULT` is explicitly persisted via `PPDMGenericRepository`.
- Most advanced analytics are computational/projection outputs only and are not persisted as PPDM rows.
- `EconomicsModule` currently has empty `EntityTypes`, signaling no module-owned table classes registered.

## PPDM Usage Inventory (Current vs Target)
| PPDM Table | Current Use | Intended Role | Gap |
|---|---|---|---|
| `ECONOMIC_ANALYSIS_RESULT` | Persist/retrieve base analysis metrics | Canonical persisted result header | Keep, formalize contract and verification |
| `NPV_PROFILE_POINT` | Returned as computed output shape | Optional persisted profile detail | Decide persistence boundary and document |
| `ECONOMIC_CASH_FLOW` | Not used in module service | Candidate for canonical cash flow persistence | Add or explicitly defer with compatibility note |

## Contract Surface Snapshot
- API-facing routes are under `api/EconomicAnalysis/*`, not field-scoped.
- Service includes a mixed surface: core persisted methods + advanced analytical methods.
- Current API exposes core methods (NPV/IRR/analyze/profile/save/get) but no staged marker strategy is documented.

## Seed and Module Setup Snapshot
- `EconomicsModule.SeedAsync` delegates to shared accounting reference seeding.
- No project-local reference constants catalog exists yet.
- No explicit module-local idempotent reference family matrix is documented.

## Canonicalization Gaps (Phase 0 Findings)
1. No project-local `.plans` execution documents existed before this pass.
2. No module `MASTER-TODO-TRACKER.md` existed before this pass.
3. Missing explicit table-vs-projection ownership map for EconomicAnalysis persistence.
4. No minimal required column-contract definition for planned PPDM tables in module scope.
5. No dedicated EconomicAnalysis controller/seed tests in `Beep.OilandGas.ApiService.Tests`.

## Exit Criteria for Phase 0
- Context map captured and approved.
- Current behavior and persistence boundaries explicitly documented.
- Gaps are translated into actionable tasks in following phase docs.
