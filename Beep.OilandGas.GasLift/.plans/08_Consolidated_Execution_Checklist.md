# Gas lift — execution checklist

## Architecture

- [x] **CLAUDE.md** — extension **`R_GAS_LIFT_REFERENCE_CODE`** only on **`GasLiftModule.EntityTypes`**; wire DTOs in **`Beep.OilandGas.Models.Data.GasLift`**
- [x] **`PPDM39.DataManagement`** does not own gas-lift domain modules
- [x] **`Beep.OilandGas.PPDM.Models`** — **project reference** from this repo (same pattern as **`Beep.OilandGas.FlashCalculations`** / **`Beep.OilandGas.Models`**)

## Build

```bash
dotnet build Beep.OilandGas.GasLift/Beep.OilandGas.GasLift.csproj
```

## Tests

```bash
dotnet test Beep.OilandGas.GasLift.Tests/Beep.OilandGas.GasLift.Tests.csproj
```

## Module seed (runtime)

Run your existing **module setup / orchestration** path (same entry as other **`IModuleSetup`** implementations). **`GasLiftModule`** executes at **`Order` 74** (after **`FLASH_CALCULATIONS`**).

## DDL / metadata

Extension table **`R_GAS_LIFT_REFERENCE_CODE`** is **not** standard PPDM 3.9 — ensure entity-driven schema tooling picks up **`GasLiftModule.EntityTypes`**.
