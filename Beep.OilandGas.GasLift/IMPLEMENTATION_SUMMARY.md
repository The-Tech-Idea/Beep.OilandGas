# Beep.OilandGas.GasLift — implementation summary

Short inventory; usage and planning: **[`README.md`](README.md)**, **[`.plans/`](.plans/)**, **[`MASTER-TODO-TRACKER.md`](MASTER-TODO-TRACKER.md)**.

## Project

- **Target:** `net10.0`
- **References:** `Beep.OilandGas.Models`, `Beep.OilandGas.GasProperties`, `Beep.OilandGas.PPDM39.DataManagement`, `SkiaSharp` (+ extended/svg), package `Beep.OilandGas.PPDM.Models`
- **Package:** `README.md` via **`PackageReadmeFile`**

## Code layout

| Area | Location |
|------|-----------|
| Calculations | `Calculations/` (`GasLiftCalculator`, valve design/spacing/potential, …) |
| Services | `Services/GasLiftService*.cs` (**`IGasLiftService`**) |
| Validation | `Validation/GasLiftValidator.cs` |
| Exceptions | `Exceptions/GasLiftException.cs` |
| Constants | `Constants/GasLiftConstants.cs` |
| Module + LOV seed | `Modules/GasLiftModule.cs`, `Data/Constants/GasLiftReferenceSets.cs`, `GasLiftReferenceCodeSeed.cs`, `Data/Tables/R_GAS_LIFT_REFERENCE_CODE.cs` |

## Shared models

**`Beep.OilandGas.Models.Data.GasLift`** — **`GAS_LIFT_*`** types consumed by **`IGasLiftService`**.

## Build

```bash
dotnet build Beep.OilandGas.GasLift/Beep.OilandGas.GasLift.csproj
dotnet test Beep.OilandGas.GasLift.Tests/Beep.OilandGas.GasLift.Tests.csproj
```
