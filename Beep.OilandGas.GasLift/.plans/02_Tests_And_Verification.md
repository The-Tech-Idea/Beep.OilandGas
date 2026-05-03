# Phase 2 — Tests and verification

## Goal

Prevent **seed drift** and lock **module contract**; add **golden vectors** for calculators when trusted references exist.

## Projects

- **`Beep.OilandGas.GasLift.Tests`** — LOV catalog, module identity.

## TODO checklist

- [x] Reference seed uniqueness + required **`REFERENCE_SET`** coverage (**`GasLiftReferenceSeedCatalogTests`**).
- [x] **`GasLiftModule`** identity (**`GasLiftModuleContractTests`**).
- [ ] Golden-vector tests for **valve design** or **spacing** vs spreadsheet / API RP tolerance.
- [ ] Optional **`ApiService.Tests`** when gas lift HTTP contract expands.

## Verification

```bash
dotnet test Beep.OilandGas.GasLift.Tests/Beep.OilandGas.GasLift.Tests.csproj
```
