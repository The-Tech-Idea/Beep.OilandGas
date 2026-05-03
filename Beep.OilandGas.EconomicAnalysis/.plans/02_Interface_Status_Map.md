# Phase 1 Interface Status Map

## Objective
Establish canonical interface governance for EconomicAnalysis and define method status for implementation planning.

## Interface Inventory
- Core interface: `Beep.OilandGas.EconomicAnalysis/Core/Interfaces/IEconomicAnalysisService.cs`
- Main implementation: `Beep.OilandGas.EconomicAnalysis/Services/EconomicAnalysisService.cs`
- Advanced partial implementation: `Beep.OilandGas.EconomicAnalysis/Services/EconomicAnalysisService.Advanced.cs`

## Method Status Classification

### Implemented and Active
- `CalculateNPV`
- `CalculateIRR`
- `Analyze`
- `GenerateNPVProfile`
- `SaveAnalysisResultAsync`
- `GetAnalysisResultAsync`
- `PerformSensitivityAnalysisAsync`
- `PerformScenarioAnalysisAsync`
- `CalculateFinancialMetricsAsync`
- `PerformBreakevenAnalysisAsync`
- `AnalyzeRiskMetricsAsync`
- `CompareProjectsAsync`

### Implemented (Advanced Surface, Not API-Promoted Yet)
- `PerformMonteCarloSimulationAsync`
- `PerformRealOptionsAnalysisAsync`
- `PerformDecisionTreeAnalysisAsync`
- `PerformAfterTaxAnalysisAsync`
- `CalculateEnterpriseValueAsync`
- `PerformLeaseBuyAnalysisAsync`
- `AnalyzeOptimalCapitalStructureAsync`
- `AnalyzeCommodityPriceSensitivityAsync`

### Deferred (Design/Contract Pending)
- Dedicated advanced interface split (`IEconomicAnalysisAdvancedService`) if API promotion starts.
- Field-scoped EconomicAnalysis API routes (`/api/field/current/economic-analysis/*`) pending orchestration decision.

## API Ownership Matrix
| Contract Member | Owned By | API Exposure | Canonical Status |
|---|---|---|---|
| Core calculation and result persistence methods | `IEconomicAnalysisService` | Yes | Canonical |
| Advanced analytical methods | `EconomicAnalysisService.Advanced.cs` | No | Advanced/Deferred |

## Governance Rules
- No endpoint promotion for advanced methods without:
  - persistence expectation documented,
  - explicit tests,
  - migration note entry.
- Keep one-class-one-responsibility in future splits (core vs advanced contract files).

## Exit Criteria for Phase 1
- All current methods classified as active, advanced, or deferred.
- Controller exposure and interface ownership aligned.
- Promotion gate rules recorded for subsequent phases.
