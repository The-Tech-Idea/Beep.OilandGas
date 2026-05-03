# Phase 4 — API and LifeCycle orchestration

## Goal

Single physics path: **HTTP** (if dedicated flash controller exists) and **`PPDMCalculationService.PerformFlashCalculationAsync`** (or equivalent) must agree on **EOS** string, **options**, and **units**.

## Target files

- **`Beep.OilandGas.ApiService/Program.cs`** — **`IFlashCalculationService`** registration
- **`PPDMCalculationService`** — flash branches
- **`FlashCalculationsMapper`**

## TODO checklist

- [ ] Map **`FlashCalculationOptions.EquationOfState`** to **`FLASH_EOS_MODEL`** codes in **`R_FLASH_CALCULATION_REFERENCE_CODE`**.
- [ ] Rethrow **`OperationCanceledException`** on packaged runs.
- [ ] Align **`FlashReferenceSets`** literals with any **`FlashCalculationWellKnown`**-style wire enums in **Models**.

## Verification

- `dotnet build Beep.OilandGas.ApiService`
