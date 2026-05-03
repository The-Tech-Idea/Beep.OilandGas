# Phase 2 — Calculation engine and validation surface

## Goal

Maintain **deterministic**, numerically stable choke calculations with explicit validation and clear separation between:

- **Pure calculators** (`Calculations/*.cs`) — prefer static/pure functions where possible.
- **Orchestration** (`ChokeAnalysisService`) — wires inputs, logging, `Task.Run` where used.
- **Validation** (`Validation/ChokeValidator.cs`) — centralized bounds and consistency checks.

## Engineering guidelines

| Topic | Requirement |
|-------|-------------|
| Units | Document and enforce consistently (psia, °R, MMscf/d or field units per README). |
| Flow regime | Sonic vs subsonic selection must be testable; avoid silent fallback without logging. |
| Division by zero | Guard choke area, density, and pressure ratios before divide. |
| Defaults | Explicit defaults (e.g. discharge coefficient) must be logged or surfaced in validation warnings where engineering risk exists. |
| Partial classes | Keep cohesion: core gas choke in one partial, multiphase/advanced in others — avoid circular dependencies between partials. |

## Files to treat as critical

- `Calculations/GasChokeCalculator.cs`
- `Calculations/MultiphaseChokeCalculator.cs`
- `Calculations/ChokePerformanceCurveCalculator.cs`
- `Calculations/ChokeErosionAndSizing.cs`
- `Calculations/DischargeCoefficient.cs`
- `Validation/ChokeValidator.cs`
- `Constants/ChokeConstants.cs`

## TODO checklist

- [ ] Inventory all public entry points from service into calculators; ensure each path calls `ChokeValidator` (or equivalent) where inputs can be invalid.
- [ ] Add or extend **golden-value** style tests for at least: sonic boundary, subsonic branch, sizing inversion, curve monotonicity samples (phase 5 executes tests).
- [ ] Review `Task.Run` usage — ensure cancellation pattern if API ever passes `CancellationToken` (future interface change).
- [ ] Align `IMPLEMENTATION_SUMMARY.md` / `README.md` with actual file list if drift exists.

## Verification

```bash
dotnet build Beep.OilandGas.ChokeAnalysis/Beep.OilandGas.ChokeAnalysis.csproj
```

- [ ] Build **0 warnings** for ChokeAnalysis project (nullable enabled).

## References

- [07_Scenarios_Best_Practices_And_Industry_Reference.md](07_Scenarios_Best_Practices_And_Industry_Reference.md) — critical vs subcritical, multiphase correlation families (Gilbert / Ros / Sachdeva / etc.), \(C_D\) bands by hardware, parameter checklist, validation scenario matrix.
- Industry choke equations as cited in project README.
- `.cursor/commands/best-practices.md` where financial/safety margins apply to operational recommendations.
