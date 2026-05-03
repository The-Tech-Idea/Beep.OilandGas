# Phase 0 — FlashCalculations overview and baseline

## Purpose

**Phase equilibrium**, **isothermal flash**, **multi-stage separator trains**, and **EOS-backed** rigor (PR/SRK paths) for oil and gas PVT workflows. This library implements math and **`IFlashCalculationService`**; shared wire types live in **`Beep.OilandGas.Models.Data.Calculations`** / **`Models.Data.FlashCalculations`**.

## Inventory

| Area | Location |
|------|-----------|
| Contract | **`Beep.OilandGas.Models.Core.Interfaces.IFlashCalculationService`** |
| Service | **`FlashCalculationService`** (+ **`FlashCalculationService.Advanced.cs`**) |
| Calculations | **`Calculations/FlashCalculator.cs`**, **`MultiComponentFlash`**, **`PhaseEnvelope`**, **`AdvancedEOS`** |
| Validation | **`Validation/FlashValidator.cs`** |
| Numeric constants | **`Constants/FlashConstants.cs`** |
| Extension LOV table | **`Data/Tables/R_FLASH_CALCULATION_REFERENCE_CODE`** |
| Reference seed | **`Data/Constants/FlashReferenceSets.cs`**, **`FlashReferenceCodeSeed.cs`** |
| Module | **`Modules/FlashCalculationsModule.cs`** (**`Order` 73**) |
| LifeCycle | **`PPDMCalculationService`**, **`FlashCalculationsMapper`** |

## Baseline gaps

| Gap | Notes | Phase |
|-----|--------|--------|
| README examples reference legacy **`FlashCalculations.Models`** | Resolved — examples use **`FLASH_CONDITIONS`** / **`FLASH_COMPONENT`** | 6 |
| Persisted **flash run** history | Optional extension entity — not required for screening | 1 / 3 |
| Full **compositional** parity with external PVT packages | Document limits; tighten tests where feasible | 2 / 7 |

## Exit criteria

- [x] **`FlashCalculationsModule`** + **`R_FLASH_CALCULATION_REFERENCE_CODE`** documented and seeded.
- [ ] Golden regression tests for core flash path (phase 5).
