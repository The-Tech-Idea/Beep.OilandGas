# Phase 4 — API and LifeCycle orchestration

## Goal

Single physics path: **HTTP** (if dedicated flash controller exists) and **`PPDMCalculationService.PerformFlashCalculationAsync`** (or equivalent) must agree on **EOS** string, **options**, and **units**.

## Target files

- **`Beep.OilandGas.ApiService/Program.cs`** — **`IFlashCalculationService`** registration
- **`PPDMCalculationService`** — flash branches
- **`FlashCalculationsMapper`**

## TODO checklist

- [x] Map **`FlashCalculationOptions.EquationOfState`** → **`FLASH_EOS_MODEL`** via **`FlashEquationOfStateMapping.ToReferenceCode`** (seeded **`PR` / `SRK` / `SRK_MODIFIED` / `IDEAL_K`**); surfaced on **`FlashCalculationAdditionalResults.EosModelReferenceCode`** for **`RunRigorousFlashAsync`** and **`PerformFlashCalculationAsync`**.
- [x] Rethrow **`OperationCanceledException`** in **`RunRigorousFlashAsync`** and **`PerformFlashCalculationAsync`**.
- [x] **`FlashReferenceSets`** / **`FlashReferenceCodeSeed`** documented against **`FlashCalculationOptions.EquationOfState`** + **`FlashEquationOfStateMapping`** (no separate **`FlashCalculationWellKnown`** enum required).

## Verification

- `dotnet build Beep.OilandGas.ApiService`
