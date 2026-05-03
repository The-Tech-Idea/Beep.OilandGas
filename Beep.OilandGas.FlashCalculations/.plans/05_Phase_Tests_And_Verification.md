# Phase 5 — Tests and verification

## Goal

Guard **reference seed drift**, **module contract**, and **core flash numerics** with automated tests.

## Projects

- **`Beep.OilandGas.FlashCalculations.Tests`** — LOV catalog, module identity, future golden vectors.

## TODO checklist

- [x] Reference seed uniqueness + required **`REFERENCE_SET`** coverage (**`FlashReferenceSeedCatalogTests`**).
- [x] **`FlashCalculationsModule`** **`ModuleId`**, **`Order`**, **`EntityTypes`** (**`FlashCalculationsModuleContractTests`**).
- [x] Regression tests for **Rachford–Rice** (trivial all-liquid, two-phase interior) and **`PerformIsothermalFlash`** vs **`SolveRachfordRice`** (**`FlashCalculatorRachfordRiceTests`**).
- [ ] Golden-vector tests for **isothermal flash** (binary system) vs spreadsheet tolerance.
- [ ] Optional **`ApiService.Tests`** for flash orchestration when HTTP surface expands.

## Verification

```bash
dotnet test Beep.OilandGas.FlashCalculations.Tests/Beep.OilandGas.FlashCalculations.Tests.csproj
```
