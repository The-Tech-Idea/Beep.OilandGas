# Phase 2 — Tests and verification

## Goal

Prevent **seed drift** and lock **module contract**; add **golden vectors** for calculators when trusted references exist.

## Projects

- **`Beep.OilandGas.GasLift.Tests`** — LOV catalog, module identity.

## TODO checklist

- [x] Reference seed uniqueness + required **`REFERENCE_SET`** coverage (**`GasLiftReferenceSeedCatalogTests`**).
- [x] **`GasLiftModule`** identity (**`GasLiftModuleContractTests`**).
- [x] Regression tests for **spacing** (equal-depth golden depths, **`CalculateValveSpacing`** determinism, equal-pressure-drop depth cap) and **valve design** (**`DesignValvesUS`** invariants: ordered depths, port size, closing/opening ratio) — **`GasLiftSpacingAndDesignRegressionTests`**. (Full spreadsheet / API RP tolerance vectors remain optional.)
- [ ] Optional **`ApiService.Tests`** when gas lift HTTP contract expands.

## Verification

```bash
dotnet test Beep.OilandGas.GasLift.Tests/Beep.OilandGas.GasLift.Tests.csproj
```
