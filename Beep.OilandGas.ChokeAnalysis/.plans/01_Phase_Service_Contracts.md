# Phase 1 — Service contracts and public surface

## Goal

Keep **`IChokeAnalysisService`** the single canonical contract for choke calculations used by the solution, aligned with **`Beep.OilandGas.Models`** types and consistent async signatures.

## Canonical contract

**File:** `Beep.OilandGas.Models/Core/Interfaces/IChokeAnalysisService.cs`

Current methods (baseline):

- `CalculateDownholeChokeFlowAsync(CHOKE_PROPERTIES, GAS_CHOKE_PROPERTIES)`
- `CalculateUpholeChokeFlowAsync(CHOKE_PROPERTIES, GAS_CHOKE_PROPERTIES)`
- `CalculateDownstreamPressureAsync(CHOKE_PROPERTIES, GAS_CHOKE_PROPERTIES, decimal flowRate)`
- `CalculateRequiredChokeSizeAsync(GAS_CHOKE_PROPERTIES, decimal flowRate)`
- `ValidateChokeConfigurationAsync(CHOKE_PROPERTIES, GAS_CHOKE_PROPERTIES)`
- `CalculatePerformanceCurveAsync(...)` → `ChokePerformanceCurve[]`

**Implementation:** `Beep.OilandGas.ChokeAnalysis/Services/ChokeAnalysisService*.cs`

Partial classes may add **additional** methods (e.g. PPDM `WELL`-based overloads). Rules:

- Any method intended for **cross-project** use must appear on **`IChokeAnalysisService`** (add to interface + implement).
- Internal helpers stay `private` or `internal` on the service or static calculators.

## Models alignment

| Rule | Detail |
|------|--------|
| Location | Shared choke entities: `Beep.OilandGas.Models/Data/ChokeAnalysis/` |
| Naming | Match PPDM / domain naming already in Models (`CHOKE_PROPERTIES`, `GAS_CHOKE_PROPERTIES`, `CHOKE_FLOW_RESULT`) |
| Enums | `Beep.OilandGas.Models` / `Enums/ChokeAnalysis/` where applicable |
| No duplicate entity definitions | `Models/ChokeModels.cs` in this project is migration notes only — do not reintroduce parallel types |

## TODO checklist

- [ ] Diff `IChokeAnalysisService` against all **public** methods on `ChokeAnalysisService`; either add to interface or reduce visibility.
- [ ] Ensure request/response types used by API (`ChokeAnalysisRequest` / `ChokeAnalysisResult` in Models) map clearly to `CHOKE_*` inputs — document mapping in code comments at the calculation entry point if non-trivial.
- [ ] Confirm `ChokeValidationResult` and curve types live in **Models**, not only inside ChokeAnalysis.
- [ ] XML docs on interface match behavior (sonic vs subsonic, units: psi, Rankine, MMSCFD as documented in README).

## Verification

```bash
dotnet build Beep.OilandGas.Models/Beep.OilandGas.Models.csproj
dotnet build Beep.OilandGas.ChokeAnalysis/Beep.OilandGas.ChokeAnalysis.csproj
```

- [ ] No duplicate choke entity classes introduced under ChokeAnalysis `Models/` beyond the stub file.

## References

- [09_Interface_Surface_Canonical_vs_Extended.md](09_Interface_Surface_Canonical_vs_Extended.md) — which methods belong on `IChokeAnalysisService` vs concrete-only extensions.
- [07_Scenarios_Best_Practices_And_Industry_Reference.md](07_Scenarios_Best_Practices_And_Industry_Reference.md) — when extending `IChokeAnalysisService` for additional correlations or regime outputs, align naming with §4–§5.
- [CLAUDE.md](../../CLAUDE.md) — Data class shape, interfaces location.
- [.cursor/commands/naming-conventions.md](../../.cursor/commands/naming-conventions.md)
