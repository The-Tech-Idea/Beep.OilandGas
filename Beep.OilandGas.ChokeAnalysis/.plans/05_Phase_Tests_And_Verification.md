# Phase 5 — Tests and verification

## Goal

Protect choke math and HTTP contracts with **automated tests** and a repeatable **build gate**.

## Recommended test layers

| Layer | Location | Purpose |
|-------|-----------|---------|
| Unit | **`Beep.OilandGas.ChokeAnalysis.Tests`** (`GasChokeCalculatorRegressionTests`, `ChokePerformanceCurveCalculatorTests`) | Single-phase regime selection, throat area from diameter, CPC gas curve (°R), sizing sanity |
| API / integration | `Beep.OilandGas.ApiService.Tests` (`ChokeCalculationsControllerTests`) | `CalculationsController` choke action, cancellation, null body |

Naming: align with existing tests — filter examples:

```bash
dotnet test Beep.OilandGas.ApiService.Tests --filter "FullyQualifiedName~ChokeCalculationsControllerTests"
```

Further choke math tests may live in a dedicated test project once golden vectors from [07 §6](07_Scenarios_Best_Practices_And_Industry_Reference.md) are codified.

## Minimum scenarios

Extend with the matrix in [07_Scenarios_Best_Practices_And_Industry_Reference.md](07_Scenarios_Best_Practices_And_Industry_Reference.md) §6 (single-phase critical/subcritical, multiphase branch, integration).

- [ ] **Valid** request → 200, structured result matches expected non-null core fields.
- [ ] **Invalid** input (negative diameter, impossible pressure ordering) → 400 or documented validation path.
- [ ] **Sonic vs subsonic** representative inputs produce distinct regime outcomes (assert regime enum or downstream behavior).
- [ ] Optional: snapshot tolerances for numerical outputs if regression suite uses approved reference vectors.

## Build gates

```bash
dotnet build Beep.OilandGas.sln
dotnet test Beep.OilandGas.ChokeAnalysis.Tests/Beep.OilandGas.ChokeAnalysis.Tests.csproj
dotnet test Beep.OilandGas.ApiService.Tests --no-build
```

For choke-only iteration:

```bash
dotnet build Beep.OilandGas.ChokeAnalysis/Beep.OilandGas.ChokeAnalysis.csproj
dotnet test <test-project> --filter "FullyQualifiedName~Choke"
```

## TODO checklist

- [ ] Discover existing choke-related tests via grep `Choke` in `ApiService.Tests`.
- [ ] Fill gaps for any public `IChokeAnalysisService` method without coverage.
- [ ] Document flaky thresholds if any tests use floating comparisons (delta documented in test).

## Exit criteria

- [ ] Solution builds with **0 warnings** for touched projects (or justified `NoWarn` scoped narrowly).
- [ ] Choke-related tests green in CI/local before merge.
