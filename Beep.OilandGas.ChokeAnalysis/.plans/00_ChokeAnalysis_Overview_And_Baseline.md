# Phase 0 — Overview and baseline (ChokeAnalysis)

## Purpose

Establish **what ChokeAnalysis owns**, what it depends on, and what “done” means before changing contracts or math.

## Scope

| In scope | Out of scope (delegate elsewhere) |
|----------|-------------------------------------|
| Gas choke flow equations, discharge coefficient helpers, performance curves, erosion/sizing helpers under this project | Well CRUD, raw `WELL_STATUS` queries without WellServices |
| `ChokeAnalysisService` (+ partials), validators, `Calculations/*` | Blazor UI pages (Web project) unless explicitly tasked |
| Consumption of `CHOKE_*` / `GAS_CHOKE_*` types from **Models** | Duplicating table entities inside ChokeAnalysis |

## Baseline inventory (verify / update during work)

| Area | Paths / artifacts |
|------|-------------------|
| Canonical interface | `Beep.OilandGas.Models/Core/Interfaces/IChokeAnalysisService.cs` |
| Implementation | `Services/ChokeAnalysisService.cs`, `ChokeAnalysisService.Advanced.cs`, `ChokeAnalysisService.Multiphase.cs` |
| Core math | `Calculations/GasChokeCalculator.cs`, `MultiphaseChokeCalculator.cs`, `ChokePerformanceCurveCalculator.cs`, `ChokeErosionAndSizing.cs`, `DischargeCoefficient.cs` |
| Validation | `Validation/ChokeValidator.cs` |
| Constants | `Constants/ChokeConstants.cs` |
| Shared entities | `Beep.OilandGas.Models/Data/ChokeAnalysis/*` (PPDM-aligned names) |
| API surface | `ICalculationService.PerformChokeAnalysisAsync` → `CalculationsController` POST `api/calculations/choke` |
| Project file | `Beep.OilandGas.ChokeAnalysis.csproj` (packaging, README, nullable) |
| User docs | `README.md`, `IMPLEMENTATION_SUMMARY.md` |
| Scenarios & industry reference | [07_Scenarios_Best_Practices_And_Industry_Reference.md](07_Scenarios_Best_Practices_And_Industry_Reference.md) — operational scenarios, choke hardware types, fluid/reservoir matrix, model-selection guidance |

## Governance checklist (every change set)

- [ ] Interfaces only in `Beep.OilandGas.Models.Core.Interfaces`.
- [ ] No new `*.DTO` namespaces; use `Beep.OilandGas.Models.Data` slices.
- [ ] Table classes: `ModelEntityBase`, scalars only; projections may contain collections.
- [ ] Well mutations / status queries: route through **WellServices** (no raw `PPDMGenericRepository` for `WELL`/`WELL_STATUS`/`WELL_XREF` in this library).
- [ ] Service registration: factory pattern; confirm order in `Program.cs` if adding dependencies.

## Risks / assumptions

| Risk | Mitigation |
|------|------------|
| Default pressures/Z-factor placeholders in PPDM path | Document assumptions; phase 3 tracks replacing with measured/calculated values where data exists |
| Duplication between API request models and `CHOKE_PROPERTIES` | Map explicitly in calculation layer; keep single semantic source in Models |
| Numerical edge cases (sonic boundary, zero area) | Phase 2: explicit tests and guardrails |
| Correlation/regime mismatch (e.g. Gilbert used subcritical) | See **07** — regime-aware model selection; unit tests per scenario matrix |

## Exit criteria (Phase 0)

- [ ] Inventory table above reviewed against repo (paths still accurate).
- [ ] Stakeholders agree whether upcoming work is **contract**, **math**, **PPDM mapping**, or **API** heavy (drives phase ordering).
