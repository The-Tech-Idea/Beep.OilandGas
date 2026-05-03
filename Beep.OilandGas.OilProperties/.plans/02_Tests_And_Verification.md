# Phase 2 — Tests and verification

## Objectives

1. Add **`Beep.OilandGas.OilProperties.Tests`** (xUnit), project-referencing **`Beep.OilandGas.OilProperties`** and **`Beep.OilandGas.Models`** only unless persistence tests require a harness.
2. Lock **regression** vectors for **`OilPropertyCalculator`** after phase 1 unit fix (temperature basis may change expected numbers — update vectors once, then freeze).
3. Add **scenario tags** (trait/category) aligned with **`04_Industry_Black_Oil_PVT_Scenarios.md`**.

## TODO checklist

| # | Task | Verification |
|---|------|----------------|
| 2.1 | Create **`Beep.OilandGas.OilProperties.Tests`** project; add to **solution** if missing. | **Done** — `dotnet test ...OilProperties.Tests.csproj` |
| 2.2 | **`OilPropertyCalculator`** — golden cases: Pb, Rs, Bo, μ_dead, μ_sat for **at least two** published/synthetic checkpoints (document source in test comment). | Tests pass |
| 2.3 | **`OilPropertyValidator`** — throws on **P ≤ 0**, **API out of range**, **γg ≤ 0**. | Tests pass |
| 2.4 | **`CalculateBlackOilPropertiesAsync`** — smoke test with **`OIL_PROPERTY_CONDITIONS`** after validator + unit fix (Moq logger optional). | Tests pass |
| 2.5 | Edge cases: **Rs = 0**, **very high API**, **P >> Pb** (undersaturated path expectations documented). | Assert documented behavior |
| 2.6 | Optional: persistence integration test (same pattern as other features: test DB or skipped `[Fact(Skip=...)]`). | CI policy |

## Verification commands

See **[`08_Consolidated_Execution_Checklist.md`](08_Consolidated_Execution_Checklist.md)**.

## Exit criteria

- [ ] Minimum **8** meaningful assertions across calculator + validator + one service method.
- [ ] CI or local **`dotnet test`** documented in tracker.
