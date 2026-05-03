# Phase 1 — Calculators, services, validation, and contracts

## Objectives

1. **Single unit story** — Document and enforce **US field units** (GPM, ft, hp, SG) vs **SI** where dual support exists; conversion helpers belong next to **`PumpConstants`**.
2. **Service vs static calculators** — **`PumpPerformanceService`** should delegate to **`Calculations/*`** only (no duplicated magic numbers); audit **`CalculateCFactorAsync`** and curve generation for physical consistency.
3. **Validation depth** — Extend **`PumpDataValidator`** (or split partials) for: non-negative flows, monotonicity hints for H–Q where applicable, **NPSHa** vs **NPSHr** ordering, **SG** bands for oil/water/mixed.
4. **Contracts** — If **API** needs a stable interface, add **`IPumpPerformanceService`** (or pump-specific subset) under **`Beep.OilandGas.Models.Core.Interfaces`** *only if* multiple hosts must share it; otherwise document **feature-local** contract and DI registration pattern.

## TODO checklist

| # | Task | Target files |
|---|------|----------------|
| 1.1 | Audit **every** public method on **`PumpPerformanceService`** for delegation to **`Calculations`** and consistent exceptions (`InvalidInputException` vs `ArgumentException`). | `Services/PumpPerformanceService.cs` |
| 1.2 | Document **C-Factor** definition used in code vs petroleum “C” symbols; rename or XML-doc to disambiguate. | Service + `IPumpPerformanceService` |
| 1.3 | Centralize **3960 / 1714** style factors — ensure **`PumpConstants`** is the single source. | **Done** — `HorsepowerConversionFactor`, `HorsepowerFromGpmPsiFactor`; **AffinityLaws**, **ViscosityCorrectionCalculator**, **PositiveDisplacementPump** use constants |
| 1.4 | **GasProperties** reference: grep usage; remove from **`.csproj`** if no `.cs` import. | `.csproj` |
| 1.5 | Optional **partial** split: `PumpPerformanceService.HQ.cs`, `PumpPerformanceService.ESP.cs` if service grows. | `Services/` |
| 1.6 | **Headless** path: ensure core calculations compile **without** Skia references in test project (project ref excludes rendering if needed). | `.csproj`, tests |

## Verification criteria

- [ ] `dotnet build Beep.OilandGas.PumpPerformance/Beep.OilandGas.PumpPerformance.csproj` — 0 errors.
- [ ] No duplicate horsepower / SG constants outside **`PumpConstants`** (allowlist small literals only where documented).

## Exit criteria

Phase 1 “done” when **1.1**, **1.3**, and **1.4** are complete or explicitly deferred with rationale in **`MASTER-TODO-TRACKER.md`**.
