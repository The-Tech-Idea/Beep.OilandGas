# Production Forecasting Master Tracker

## Phase Rollup

- [x] Phase 0 - Canonical refresh overview documented (`.plans/00_*.md`)
- [x] Phase 1 - Service contracts documented (Models.Core vs feature-local)
- [x] Phase 2 - Data and calculation surface documented
- [x] Phase 3 - API hardening completed (`ProductionForecastingController`)
- [x] Phase 4 - Tests and verification completed
- [x] Phase 5 - Reference seeds (`R_PRODUCTION_FORECASTING_REFERENCE_CODE`, module seed, catalog tests)
- [x] Phase 6 - Algorithm engine revision (DCA merge, orchestration, history fit, guardrails)

## Active TODOs

- [x] Add `.plans` phased docs and this tracker.
- [x] XML remarks on canonical and feature-local `IProductionForecastingService`.
- [x] Harden `ProductionForecastingController` (null guards, exception mapping, cancellation).
- [x] Add `ProductionForecastingControllerTests`.
- [x] Run build and filtered test gate; record outcomes below.
- [x] Reference seed catalog, `ProductionForecastingModule`, and service alignment to `ProductionForecastingReferenceCodes`.
- [x] Merge `Beep.OilandGas.DCA` into `Beep.OilandGas.ProductionForecasting` and remove standalone project references.
- [x] Replace `GenerateForecastAsync` placeholder generation with decline-orchestrated request path.
- [x] Add PPDM history-fit loader (`PDEN_WELL` + `PDEN_VOL_SUMMARY`) with minimum-history validation.
- [x] Add guardrails for Arps `b`, modified hyperbolic `Dlim`, and optional economic limit.
- [x] Add and pass algorithm guardrail regression tests.

## Verification Outcomes

- [x] `dotnet build Beep.OilandGas.sln` (succeeded, 0 warnings / 0 errors)
- [x] `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~ProductionForecastingControllerTests"` (10/10 passed)
- [x] `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~ProductionForecastingReferenceSeedCatalogTests"` (4/4 passed)
- [x] `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~ProductionForecastingAlgorithmGuardrailTests"` (2/2 passed)
