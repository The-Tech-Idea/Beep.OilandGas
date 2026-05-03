# MASTER-TODO-TRACKER — Beep.OilandGas.ChokeAnalysis

Single rollup for **planning and execution** status. Detailed steps live under [.plans/](.plans/README.md).

## Phase rollup

| Phase | Document | Status |
|-------|----------|--------|
| 0 | [.plans/00_ChokeAnalysis_Overview_And_Baseline.md](.plans/00_ChokeAnalysis_Overview_And_Baseline.md) | Doc baseline |
| 1 | [.plans/01_Phase_Service_Contracts.md](.plans/01_Phase_Service_Contracts.md) | Doc baseline |
| 2 | [.plans/02_Phase_Calculation_And_Validation_Surface.md](.plans/02_Phase_Calculation_And_Validation_Surface.md) | Doc baseline |
| 3 | [.plans/03_Phase_PPDM_And_Data_Paths.md](.plans/03_Phase_PPDM_And_Data_Paths.md) | Doc baseline |
| 4 | [.plans/04_Phase_API_And_Orchestration_Integration.md](.plans/04_Phase_API_And_Orchestration_Integration.md) | Doc baseline |
| 5 | [.plans/05_Phase_Tests_And_Verification.md](.plans/05_Phase_Tests_And_Verification.md) | Doc baseline |
| 6 | [.plans/06_Phase_Packaging_Docs_And_Backlog.md](.plans/06_Phase_Packaging_Docs_And_Backlog.md) | Doc baseline |
| Reference | [.plans/07_Scenarios_Best_Practices_And_Industry_Reference.md](.plans/07_Scenarios_Best_Practices_And_Industry_Reference.md) | Doc baseline |
| Checklist | [.plans/08_Consolidated_Execution_Checklist.md](.plans/08_Consolidated_Execution_Checklist.md) | Doc baseline |
| Interface map | [.plans/09_Interface_Surface_Canonical_vs_Extended.md](.plans/09_Interface_Surface_Canonical_vs_Extended.md) | Doc baseline |

Update the **Status** column when a phase’s exit criteria are fully satisfied in the codebase (not just when documents exist).

## Repository standards (always)

- [CLAUDE.md](../CLAUDE.md) — three-layer architecture, table vs projection, WellServices, DI factory registration.
- **Extension choke tables** — register on `ChokeAnalysisModule.EntityTypes`; physical schema via **entity-driven / ModuleSetup tooling**, not new `.sql` under `Models/Scripts` (see CLAUDE.md *Schema for extension tables*).
- Choke-specific scenarios and industry practice: [.plans/07_Scenarios_Best_Practices_And_Industry_Reference.md](.plans/07_Scenarios_Best_Practices_And_Industry_Reference.md).

## Consolidated next actions (editable)

Copy items here when prioritizing a sprint; link PRs or commits when done.

| Priority | Action | Owner phase |
|----------|--------|-------------|
| Done | Trace `IChokeAnalysisService` vs extended methods — see [.plans/09_Interface_Surface_Canonical_vs_Extended.md](.plans/09_Interface_Surface_Canonical_vs_Extended.md) | 1 |
| Done | API regression tests for `POST /api/calculations/choke` — `ChokeCalculationsControllerTests` | 5 |
| Done | Align `IMPLEMENTATION_SUMMARY.md` with Models + GasProperties + orchestration | 6 |
| Done | Core `GasChokeCalculator` regression tests — `Beep.OilandGas.ChokeAnalysis.Tests` | 5 |
| Done | Lock `EnsureChokeThroatArea` (diameter-only input) + `GenerateGasCurve` Rankine (°R) smoke — same test project | 5 |
| Done | `ChokeAnalysisModule` (`IModuleSetup`) — entities `CHOKE_*`, `GAS_CHOKE_*`, `CHOKE_FLOW_RESULT`, `R_CHOKE_ANALYSIS_REFERENCE_CODE`; seed via `ChokeAnalysisReferenceCodeSeed` | 3 |
| Done | `ICalculationService.PerformChokeAnalysisAsync` — single-phase gas via `IChokeAnalysisService` when upstream/downstream set; `CorrelationMethod` `GILBERT`/`MULTIPHASE` retains Gilbert fallback (`PPDMCalculationService.ArtificialLift`) | 4 |
| Done | Gas CPC `CpcPoint.Regime` — `SONIC`/`SUBSONIC` via `ChokeAnalysisReferenceCodes`; README + `ChokePerformanceCurveCalculatorTests` | 2 / 6 |
| Done | `PerformChokeAnalysisAsync` Gilbert/MULTIPHASE fallback — `FlowRegime` uses `SONIC`/`SUBSONIC`; optimization recommendations only when regime matches; `ApiService.Tests` `PPDMCalculationServiceChokeTests` | 4 |
| Done | `ChokeAnalysisReferenceCodes.UseMultiphaseOrchestration` — Ros/Achong/… no longer misrouted to single-phase gas; `CorrelationMultiphase` + seed row; `ChokeAnalysisOrchestrationRulesTests` | 1 / 4 |
| Done | Multiphase orchestration — correlation-specific estimated upstream (`SelectCorrelationUpstreamPressure`), `CriticalPressureRatioOverride`, `CorrelationBaxendell` + seed; `MultiphaseChokeCalculatorCorrelationTests` + `PPDMCalculationServiceChokeTests` | 2 / 4 |
| Done | **Web** — `ChokeAnalysis.razor` correlation `MudSelect` → `ChokeAnalysisOptions.CorrelationMethod`; project ref `Beep.OilandGas.ChokeAnalysis` | 4 |
| — | Expand vectors per §6 in doc 07 (multiphase branches when implemented) | 5 |
| — | Optional: enrich multiphase fallback (explicit Ros/Sachdeva) + structured logs | 6 / 4 |
| — | Optional: Gilbert/Ros/Sachdeva correlation flags on requests (Models + API) | 1 / 4 |

## Verification

```bash
dotnet build Beep.OilandGas.ChokeAnalysis/Beep.OilandGas.ChokeAnalysis.csproj
dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj
```

Choke API controller tests:

```bash
dotnet test Beep.OilandGas.ApiService.Tests --filter "FullyQualifiedName~ChokeCalculationsControllerTests"
```

Calculator regression (library):

```bash
dotnet test Beep.OilandGas.ChokeAnalysis.Tests/Beep.OilandGas.ChokeAnalysis.Tests.csproj
```

Last library + API choke-related verification: **2026-05-01** — `ChokeAnalysis.Tests` (24) and `ApiService.Tests` (filter `Choke` + `OrchestrationRules` + `PPDMCalculationServiceChoke` + `MultiphaseChokeCalculatorCorrelation`, 14) passed locally.
