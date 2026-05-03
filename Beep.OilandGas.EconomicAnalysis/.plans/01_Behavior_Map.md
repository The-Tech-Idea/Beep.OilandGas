# Phase 1 Behavior Map

## Objective
Map endpoint-to-service behavior and separate canonical operational behavior from advanced/staged analytical behavior.

## API Route Behavior Map
| Route | Controller Action | Service Method | Status |
|---|---|---|---|
| `POST /api/EconomicAnalysis/npv` | `CalculateNPV` | `CalculateNPV` | Implemented |
| `POST /api/EconomicAnalysis/irr` | `CalculateIRR` | `CalculateIRR` | Implemented |
| `POST /api/EconomicAnalysis/analyze` | `Analyze` | `Analyze` | Implemented |
| `POST /api/EconomicAnalysis/npv-profile` | `GenerateNPVProfile` | `GenerateNPVProfile` | Implemented |
| `POST /api/EconomicAnalysis/result` | `SaveResult` | `SaveAnalysisResultAsync` | Implemented (persisted) |
| `GET /api/EconomicAnalysis/result/{analysisId}` | `GetResult` | `GetAnalysisResultAsync` | Implemented (persisted) |

## API Route Ownership Matrix
| Route | Contract Owner | Canonical/Advanced | Promotion Criteria |
|---|---|---|---|
| `POST /api/EconomicAnalysis/npv` | `IEconomicAnalysisService` | Canonical | Keep as-is; add regression tests |
| `POST /api/EconomicAnalysis/irr` | `IEconomicAnalysisService` | Canonical | Keep as-is; add regression tests |
| `POST /api/EconomicAnalysis/analyze` | `IEconomicAnalysisService` | Canonical | Keep as-is; add regression tests |
| `POST /api/EconomicAnalysis/npv-profile` | `IEconomicAnalysisService` | Canonical | Keep as-is; decide persistence defer/acceptance note |
| `POST /api/EconomicAnalysis/result` | `IEconomicAnalysisService` | Canonical | Keep as-is; validate save semantics |
| `GET /api/EconomicAnalysis/result/{analysisId}` | `IEconomicAnalysisService` | Canonical | Keep as-is; validate not-found/error paths |

Advanced methods currently have no API routes and must not be exposed until persistence, tests, and migration notes are complete.

## Service Behavior Classification

### Canonical Active
- Core financial calculations (`NPV`, `IRR`, `Analyze`, `NPVProfile`)
- Persist/retrieve base economic result (`ECONOMIC_ANALYSIS_RESULT`)
- Sensitivity/scenario/financial metrics/breakeven/risk/project comparison calculations

### Advanced or Provisionally Staged
- Monte Carlo simulation
- Real options analysis
- Decision tree analysis
- After-tax analysis
- DCF enterprise valuation
- Lease vs buy analysis
- Capital structure optimization
- Commodity sensitivity deep-analysis

## Ownership and Exposure Rules
- Canonical API must expose only methods with:
  1) deterministic behavior,
  2) clear persistence boundary when required,
  3) dedicated tests.
- Advanced methods can remain service-level until:
  - persistence strategy is approved,
  - API promotion tests are added,
  - compatibility impact is documented.

## Gap Notes
- Current controller exposes only core routes; advanced methods are not exposed (acceptable for now).
- No explicit documented promotion criteria existed previously; this plan defines it.

## Exit Criteria for Phase 1
- Behavior map complete and aligned with current controller.
- Canonical vs advanced classification explicitly documented.
- Promotion rule established for future advanced API endpoints.
