# Beep.OilandGas.FlashCalculations — implementation summary

This file is a **short inventory**. Authoritative usage, units, and screening vs PVT limits are in **[`README.md`](README.md)**; phased work is under **[`.plans/`](.plans/)** and **[`MASTER-TODO-TRACKER.md`](MASTER-TODO-TRACKER.md)**.

## Project

- **Target:** `net10.0` (`Beep.OilandGas.FlashCalculations.csproj`)
- **References:** `Beep.OilandGas.Models`, `Beep.OilandGas.GasProperties`, `Beep.OilandGas.PPDM39.DataManagement`, `SkiaSharp` (+ extended/svg), project `Beep.OilandGas.PPDM.Models` (same repo)
- **Package:** `README.md` packed via **`PackageReadmeFile`**

## Code layout

| Area | Location |
|------|-----------|
| Rachford–Rice / Wilson / convenience flash | `Calculations/FlashCalculator.cs` |
| Other calc / EOS paths | `Calculations/` (e.g. `MultiComponentFlash`, `PhaseEnvelope`, `AdvancedEOS` as applicable) |
| Validation | `Validation/FlashValidator.cs` |
| Multi-stage feed merge | `Services/FlashFeedCatalogMerge.cs` (liquid → feed + original **Tc/Pc/ω/MW**) |
| Exceptions | `Exceptions/FlashException.cs` (all flash exception types) |
| Constants | `Constants/FlashConstants.cs` |
| Module + LOV seed | `Modules/FlashCalculationsModule.cs`, `Data/Constants/FlashReferenceSets.cs`, `FlashReferenceCodeSeed.cs`, `Data/Tables/R_FLASH_CALCULATION_REFERENCE_CODE*` |
| Service / persistence | `Services/FlashCalculationService.cs`, `Services/FlashCalculationService.Advanced.cs` |

## Shared models (not in this assembly)

Wire and result types live in **`Beep.OilandGas.Models`**: e.g. **`Beep.OilandGas.Models.Data.FlashCalculations`** (`FLASH_CONDITIONS`, `FLASH_COMPONENT`, `FlashResult`, …) and **`Beep.OilandGas.Models.Data.Calculations`** (`FlashCalculationRequest`, `FlashCalculationOptions`, `PhasePropertiesData`).

## Build

```bash
dotnet build Beep.OilandGas.FlashCalculations/Beep.OilandGas.FlashCalculations.csproj
dotnet test Beep.OilandGas.FlashCalculations.Tests/Beep.OilandGas.FlashCalculations.Tests.csproj
```
