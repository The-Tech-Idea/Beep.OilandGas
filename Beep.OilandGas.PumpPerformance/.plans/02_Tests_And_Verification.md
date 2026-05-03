# Phase 2 — Tests and verification

## Objectives

1. Add **`Beep.OilandGas.PumpPerformance.Tests`** (xUnit), referencing **`Beep.OilandGas.PumpPerformance`** (and **`Beep.OilandGas.Models`** only if tests need DTOs).
2. Establish **golden vectors** for: overall efficiency, BHP from Q–H–η, affinity speed change, **NPSH** margin, one **system curve + pump curve** intersection (operating point).
3. **ESPDesignCalculator** — smoke/regression on generated point count and monotonic head trend where expected.

## TODO checklist

| # | Task | Verification |
|---|------|----------------|
| 2.1 | Create test project; add to **solution**. | `dotnet test ...PumpPerformance.Tests.csproj` |
| 2.2 | **`EfficiencyCalculations`** / **`PowerCalculations`** — 2–3 closed-form checks. | Tests pass |
| 2.3 | **`AffinityLaws`** — speed ratio 0.9 → head/power scaling within tolerance. | Tests pass |
| 2.4 | **`NPSHCalculations`** — known textbook point or manufacturer-style sample. | Tests pass |
| 2.5 | **`PumpDataValidator`** — throws / returns false on invalid arrays. | Tests pass |
| 2.6 | Optional: **Skia** rendering smoke (file output) — `[Fact(Skip=...)]` unless CI supports headless Skia. | Policy |

## Exit criteria

- [ ] ≥ **10** meaningful assertions across math + validator.
- [ ] CI or local test command documented in **`08`** and tracker.
