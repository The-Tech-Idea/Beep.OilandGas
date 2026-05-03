# MASTER-TODO-TRACKER — Beep.OilandGas.FlashCalculations

Rollup for **PVT / flash** enhancement. Details: **[`.plans/README.md`](.plans/README.md)**.

## Phase rollup

| Phase | Document | Status |
|-------|----------|--------|
| 0 | [.plans/00_FlashCalculations_Overview_And_Baseline.md](.plans/00_FlashCalculations_Overview_And_Baseline.md) | Baseline |
| 1 | [.plans/01_Phase_Contracts_And_Persistence.md](.plans/01_Phase_Contracts_And_Persistence.md) | Ongoing |
| 2 | [.plans/02_Phase_Numerics_Validation_And_Units.md](.plans/02_Phase_Numerics_Validation_And_Units.md) | Ongoing |
| 3 | [.plans/03_Phase_ModuleSetup_And_Reference_LOV.md](.plans/03_Phase_ModuleSetup_And_Reference_LOV.md) | Done — **`FlashCalculationsModule`** + expanded **`R_FLASH_CALCULATION_REFERENCE_CODE`** seed |
| 4 | [.plans/04_Phase_API_And_Orchestration.md](.plans/04_Phase_API_And_Orchestration.md) | Planned |
| 5 | [.plans/05_Phase_Tests_And_Verification.md](.plans/05_Phase_Tests_And_Verification.md) | In progress — **`FlashCalculations.Tests`** (10) LOV, module, merge, Rachford–Rice regression |
| 6 | [.plans/06_Phase_Docs_And_Packaging.md](.plans/06_Phase_Docs_And_Packaging.md) | Done — README usage/API aligned with **`Models.Data.FlashCalculations`** |
| Ref | [.plans/07_PVT_Best_Practices_And_Reference.md](.plans/07_PVT_Best_Practices_And_Reference.md) | Reference |
| Runbook | [.plans/08_Consolidated_Execution_Checklist.md](.plans/08_Consolidated_Execution_Checklist.md) | Active |

## Consolidated next actions

| Priority | Action |
|----------|--------|
| Done | **`FlashCalculationsModule`** — **`ModuleId` FLASH_CALCULATIONS**, **`Order` 73**, **`SeedAsync`** for **`R_FLASH_CALCULATION_REFERENCE_CODE`** |
| Done | Extended LOVs: EOS, category, solver, **specification**, **phase state**, **property kind** |
| Done | README **screening vs PVT** limits + **`IMPLEMENTATION_SUMMARY`** aligned with current tree |
| Done | **`PerformMultiStageFlash`** preserves **Tc/Pc/ω/MW** via **`FlashFeedCatalogMerge`** + unit tests |
| Done | **`IFlashCalculationService.RunRigorousFlashAsync`** + **`CancellationToken`**; **`POST /api/FlashCalculation/rigorous`** |
| P2 | Golden vectors for **Rachford–Rice** / **Wilson** vs trusted spreadsheet |
| P3 | Optional persisted **flash run history** extension table — ADR if needed |

## Verification

```bash
dotnet build Beep.OilandGas.FlashCalculations/Beep.OilandGas.FlashCalculations.csproj
dotnet test Beep.OilandGas.FlashCalculations.Tests/Beep.OilandGas.FlashCalculations.Tests.csproj
```
