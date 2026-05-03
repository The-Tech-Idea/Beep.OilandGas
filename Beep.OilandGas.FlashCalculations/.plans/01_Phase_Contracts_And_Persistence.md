# Phase 1 — Contracts and persistence

## Goal

Keep **`IFlashCalculationService`** stable for ApiService/LifeCycle; align **`FlashCalculationService`** with **`IPPDM39DefaultsRepository.FormatIdForTable`** when persisting; avoid storing non-scalar graphs on table entities.

## Target files

- **`Beep.OilandGas.Models/Core/Interfaces/IFlashCalculationService.cs`**
- **`Services/FlashCalculationService.cs`** — repository usage for any persisted **`FLASH_*`** types
- **`LifeCycle`** — **`FlashCalculationsMapper`**, **`PerformFlashCalculationAsync`**

## TODO checklist

- [ ] Audit **`FLASH_CONDITIONS`** / result types: confirm **projection vs table** usage per **CLAUDE.md**.
- [ ] When saving runs: use **`FormatIdForTable`** for new IDs; audit columns from **`ICommonColumnHandler`**.
- [ ] **`OperationCanceledException`** — propagate from long-running orchestrated flashes.

## Verification

- `dotnet build Beep.OilandGas.FlashCalculations`
- `dotnet build Beep.OilandGas.ApiService`
