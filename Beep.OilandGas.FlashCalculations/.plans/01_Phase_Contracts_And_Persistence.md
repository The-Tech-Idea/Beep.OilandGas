# Phase 1 — Contracts and persistence

## Goal

Keep **`IFlashCalculationService`** stable for ApiService/LifeCycle; align **`FlashCalculationService`** with **`IPPDM39DefaultsRepository.FormatIdForTable`** when persisting; avoid storing non-scalar graphs on table entities.

## Target files

- **`Beep.OilandGas.Models/Core/Interfaces/IFlashCalculationService.cs`**
- **`Services/FlashCalculationService.cs`** — repository usage for any persisted **`FLASH_*`** types
- **`LifeCycle`** — **`FlashCalculationsMapper`**, **`PerformFlashCalculationAsync`**

## TODO checklist

- [x] Audit **`FLASH_CONDITIONS`** / result types: confirm **projection vs table** usage per **CLAUDE.md** (feed + conditions are projections; **`FLASH_CALCULATION_RESULT`** is the persisted table path via **`FlashCalculationService.SaveFlashResultAsync`** / LifeCycle insert mapping).
- [x] When saving runs: use **`FormatIdForTable`** for new IDs — **`PerformFlashCalculationAsync`** / **`MapFlashResultToDTO`** and **`RunRigorousFlashAsync`** use **`FormatIdForTable("FLASH_CALCULATION", …)`** for **`CalculationId`** (aligned with existing **`SaveFlashResultAsync`** pattern).
- [x] **`OperationCanceledException`** — **`RunRigorousFlashAsync`** and **`PerformFlashCalculationAsync`** rethrow before generic logging paths.

## Verification

- `dotnet build Beep.OilandGas.FlashCalculations`
- `dotnet build Beep.OilandGas.ApiService`
