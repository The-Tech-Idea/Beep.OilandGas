# Phase 2 — Tests, verification, and golden vectors

## Objectives

1. **`Beep.OilandGas.WellTestAnalysis.Tests`** (xUnit, **net10.0**) is in the solution with a project reference to **`Beep.OilandGas.WellTestAnalysis`** (wire types come transitively via **`Beep.OilandGas.Models`**). Expand coverage per rows **2.2–2.5** below.
2. **Synthetic datasets** — Small noise-free pressure vs time series where **Horner** slope / intercept or **drawdown** line source can be checked within tolerance.
3. **Derivative** — Monotonic increasing time; verify length preservation or documented trimming; smoothing factor edge cases.
4. **Gas path** — Smoke test **`AnalyzeGasBuildUp`** with distinct pressures and Rankine **T** (see **`WellTestAnalyzerAnalysisTests`**).

## TODO checklist

| # | Task | Verification |
|---|------|----------------|
| 2.1 | ~~Create test project; add to **solution**.~~ **Done** — baseline tests committed; keep green in CI. | `dotnet test Beep.OilandGas.WellTestAnalysis.Tests/Beep.OilandGas.WellTestAnalysis.Tests.csproj` |
| 2.2 | **`WellTestDataValidator`** — invalid null / empty pressure list → throws or returns documented outcome. | Tests (null radius, time order, **`PRODUCTION_TIME`**, case-insensitive **`TEST_TYPE`**) |
| 2.3 | **`DerivativeAnalysis`** — linear pressure ramp → derivative sign/trend sanity. | Tests (length / empty edge cases) |
| 2.4 | **`BuildUpAnalysis.AnalyzeHorner`** — one closed-form or literature-simplified case. | **Done** — synthetic semi-log Horner line (**`WellTestAnalyzerAnalysisTests`**) |
| 2.5 | **`WellTestAnalyzer.IdentifyReservoirModel`** — known synthetic derivative shape if deterministic. | Optional |

## Exit criteria

- [x] ≥ **8** meaningful assertions across validator + derivative path (**current tests** meet this; extend for **2.4** / **2.5**).
- [x] Local **`dotnet test`** for **`WellTestAnalysis.Tests`** documented in **`08`** and **`MASTER-TODO-TRACKER.md`** (add CI job only if solution pipeline should gate on this project explicitly).
