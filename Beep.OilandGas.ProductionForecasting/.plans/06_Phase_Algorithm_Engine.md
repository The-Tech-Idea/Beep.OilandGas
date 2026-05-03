# Phase 6: Algorithm Engine Consolidation And Extension Backlog

## Goal

Finalize the merged forecasting engine in `Beep.OilandGas.ProductionForecasting` so `GenerateForecastAsync` routes through decline math, history fitting, and guardrails; then document post-release extensions.

## Delivered In This Phase

- Merged `Beep.OilandGas.DCA` source into `Beep.OilandGas.ProductionForecasting/DCA` and removed `Beep.OilandGas.DCA.csproj` references from solution consumers.
- Replaced placeholder forecast generation with request-driven orchestration via:
  - `Services/ProductionForecastingService.ForecastGeneration.cs`
  - `Services/ProductionHistoryLoader.cs`
  - `Services/ProductionForecastResultMapper.cs`
- Added history-fit path (`PDEN_WELL` → `PDEN_VOL_SUMMARY`) and minimum-history enforcement.
- Added guardrails:
  - Arps \(b\) bounds constants
  - optional `q_econ`
  - modified hyperbolic (`Dlim` terminal decline switch)
- Added regression coverage for modified hyperbolic and economic truncation in:
  - `Beep.OilandGas.ApiService.Tests/ProductionForecastingAlgorithmGuardrailTests.cs`

## Extended Backlog (Multi-Release)

- [ ] Add unconventional model families in merged DCA engine: Duong, SEPD, PLE.
- [ ] Add diagnostics-assisted model selection (flow-regime checks before Arps fit acceptance).
- [ ] Add multiphase coupling (oil/gas/water + GOR/CGR trend handling) and BOE pathways.
- [ ] Add curtailment and choke normalization before fit.
- [ ] Add segmentation/workover-aware multi-segment fitting for interrupted production histories.
- [ ] Add probabilistic envelopes (P10/P50/P90) with bounded service runtime.
- [ ] Add field/type-curve aggregation by completion cohort, not naive averaging.

## Verification

- `dotnet clean Beep.OilandGas.sln`
- `dotnet build Beep.OilandGas.sln`
- `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~ProductionForecastingControllerTests"`
- `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~ProductionForecastingAlgorithmGuardrailTests"`
