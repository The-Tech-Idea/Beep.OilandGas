# Phase 2 — Tests and verification

## Goal

Add **`Beep.OilandGas.GasProperties.Tests`** (or equivalent test project) so Z-factor, viscosity, and pseudo-pressure paths have **regression** coverage and optional **literature / spreadsheet** golden vectors.

**Scenario coverage** (well / facility / reservoir) should be reflected with **xUnit traits** or naming convention aligned with **[`04_Industry_Scenarios_Wells_Facilities_Reservoirs.md`](04_Industry_Scenarios_Wells_Facilities_Reservoirs.md)** so CI can filter or expand over time.

## Projects (target)

- **`Beep.OilandGas.GasProperties.Tests`** — new xUnit project referencing **`Beep.OilandGas.GasProperties`** only (avoid pulling **`ApiService`** unless needed for integration later).

## TODO checklist

- [ ] Create **`Beep.OilandGas.GasProperties.Tests`** + add to **`Beep.OilandGas.sln`**.
- [ ] **`ZFactorCalculator`** — 2–3 fixed (P, T, γg) points per correlation (Brill–Beggs, Hall–Yarborough, DAK) with tolerances documented in test comments.
- [ ] **`GasViscosityCalculator`** — at least one vector per published correlation (Carr–Kobayashi–Burrows, Lee–Gonzalez–Eakin) given Z from Brill–Beggs.
- [ ] **`GasPropertiesValidator`** — throws **`ParameterOutOfRangeException`** (or documented exception types) for negative pressure, γg ≤ 0, invalid composition sum.
- [ ] **`PseudoPressureCalculator`** — smoke test: monotonicity or boundedness vs pressure for a single γg strip (looser tolerance acceptable).
- [ ] Optional: snapshot tests for **`AveragePropertiesCalculator`** small grids.
- [ ] **Well pillar:** at least one test each for **dry gas**, **associated-gas-style** (separator **P, T**, lean **γg**), and **sour/acid** (composition with **CO₂+H₂S** non-zero) if composition model supports it.
- [ ] **Facility pillar:** **compressor-like** (high **Pr**) and **pipeline-like** (pressure-weighted average) smoke vectors.
- [ ] **Reservoir pillar:** depletion strip **Z(P)** or **m(p)** monotonicity for a **dry-gas** depletion path (documented **T** assumption).
- [ ] **Negative / out-of-scope:** explicit test or doc that **two-phase** or **EOS** problems are **not** claimed as covered by single-phase **Z** alone.

## Verification

```bash
dotnet test Beep.OilandGas.GasProperties.Tests/Beep.OilandGas.GasProperties.Tests.csproj
```

## Exit criteria

- [ ] **`dotnet test`** green on CI for the new project.
- [ ] At least **6** meaningful assertions across Z + viscosity + validator, with **≥1** tagged case per **well / facility / reservoir** pillar (see phase **4**).
