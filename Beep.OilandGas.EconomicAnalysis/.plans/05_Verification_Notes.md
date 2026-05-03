# Phase 4 Verification Notes

## Objective
Define concrete verification gates for EconomicAnalysis canonicalization and rollout readiness.

## Build Gates
- [x] `dotnet build Beep.OilandGas.EconomicAnalysis/Beep.OilandGas.EconomicAnalysis.csproj`
- [x] `dotnet build Beep.OilandGas.ApiService/Beep.OilandGas.ApiService.csproj`
- [x] `dotnet build Beep.OilandGas.sln`

## Test Gates
- [x] Add and run `Beep.OilandGas.ApiService.Tests/EconomicAnalysisControllerTests.cs`
- [x] Add and run `EconomicAnalysisReferenceSeedCatalogTests` if module-local seed catalog is introduced.
- [x] Add interface-boundary tests when canonical/advanced contract split is introduced.

## Suggested Command Set
- [x] `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~EconomicAnalysisControllerTests"`
- [x] `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~EconomicAnalysisReferenceSeedCatalogTests"`
- [x] `dotnet test Beep.OilandGas.ApiService.Tests/Beep.OilandGas.ApiService.Tests.csproj --filter "FullyQualifiedName~EconomicAnalysisInterfaceContractTests"`

## Functional Verification Matrix
| Area | Check | Expected |
|---|---|---|
| Core calculations | NPV/IRR/analyze endpoints return deterministic results | 200 + valid payload |
| Result persistence | Save/Get analysis result roundtrip | Persisted values match request |
| Error handling | Invalid inputs and missing ids | 400/404 paths validated |
| DI wiring | Service resolution at startup | No missing dependency failures |
| Seed reruns | Repeat module seed execution | No duplicate reference rows |

## Data Contract Verification
- [x] Required columns populated for `ECONOMIC_ANALYSIS_RESULT`.
- [x] No projection classes passed directly to repository write operations.
- [ ] Any newly introduced tables satisfy minimal contract section in `03_Data_Access_And_Seed_Strategy.md`.

## API Boundary Verification
- [x] Controller routes map only to canonical active methods.
- [ ] Advanced methods remain non-promoted unless all promotion criteria are met.
- [ ] Compatibility behavior documented for unchanged legacy consumers.

## Evidence Snapshot
- EconomicAnalysis build passes with zero errors.
- ApiService build passes with zero errors.
- Focused EconomicAnalysis tests pass (`Controller`, `FieldController`, `InterfaceContract`, `ReferenceSeedCatalog`) with latest run total `18` passed.
- Full solution build passes with zero errors after Web-layer Razor fixes and warning cleanup.

## Exit Criteria
- All build gates green.
- Focused tests added and green.
- Seed idempotency confirmed.
- Contract boundaries validated and documented.
