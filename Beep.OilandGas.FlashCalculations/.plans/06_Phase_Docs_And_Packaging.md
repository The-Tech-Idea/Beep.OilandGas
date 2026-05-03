# Phase 6 — Documentation and packaging

## Goal

**README** reflects real namespaces (**`Models.Data.FlashCalculations`**), **module** registration, **limits** (screening vs external PVT — README subsection + link to phase 7), and **`PackageReadmeFile`** already set in **`.csproj`**.

## TODO checklist

- [x] Replace legacy **`FlashCalculations.Models`** examples with **`Models.Data.FlashCalculations`** types.
- [x] Document **`R_FLASH_CALCULATION_REFERENCE_CODE`** sets and alignment with **`FlashCalculationOptions.EquationOfState`**.
- [x] Link **[`MASTER-TODO-TRACKER.md`](../MASTER-TODO-TRACKER.md)** from README.

## Verification

- `dotnet build Beep.OilandGas.FlashCalculations/Beep.OilandGas.FlashCalculations.csproj`
- `dotnet pack Beep.OilandGas.FlashCalculations/Beep.OilandGas.FlashCalculations.csproj` (optional)
