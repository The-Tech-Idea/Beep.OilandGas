# Phase 0 — Pump performance overview and baseline

## Purpose

**Pump performance** screening and design support for oil and gas: **H–Q** relationships, **efficiency** and **power**, **affinity laws**, **NPSH**, **system curves**, **viscosity corrections**, **ESP** stage/curve generation (`ESPDesignCalculator` + **`Beep.OilandGas.Models.Data.PumpPerformance`**), and **Skia**-based **rendering** for curves. The assembly is **`Beep.OilandGas.PumpPerformance`** (README, package readme, and phase summaries use this root namespace consistently).

This project is typically a **calculation + visualization library** with an optional **`IPumpPerformanceService`** / **`PumpPerformanceService`** façade — **not** a **`ModuleSetupBase`** domain unless product adds **extension `R_*` pump catalog tables** in a feature module.

## Inventory (this assembly)

| Area | Location |
|------|-----------|
| Legacy entry | **`PumpPerformanceCalc.cs`** |
| Core math | **`Calculations/`** — `HeadQuantityCalculations`, `EfficiencyCalculations`, `PowerCalculations`, `SystemCurveCalculations`, `NPSHCalculations`, `AffinityLaws`, `ViscosityCorrectionCalculator`, `ESPDesignCalculator`, … |
| Constants | **`Constants/PumpConstants.cs`** |
| Validation | **`Validation/PumpDataValidator.cs`** |
| Pump families | **`PumpTypes/`** — centrifugal, PD, ESP, jet |
| System / selection | **`SystemAnalysis/`** — `PumpSelection`, `MultiPumpConfiguration` |
| Services | **`Services/IPumpPerformanceService.cs`**, **`PumpPerformanceService.cs`** |
| Rendering | **`Rendering/`** |
| Interaction | **`Interaction/PumpPerformanceInteractionHandler.cs`** |
| Exceptions | **`Exceptions/`** |

## Shared contracts (outside this assembly)

| Area | Location |
|------|----------|
| ESP / pump DTOs and entities | **`Beep.OilandGas.Models.Data.PumpPerformance`** (and related under **`Models.Data`**) |
| Related lift / pump domains | **`Beep.OilandGas.HydraulicPumps`**, **`Beep.OilandGas.SuckerRodPumping`**, **`Beep.OilandGas.NodalAnalysis`** — coordinate scenarios, do not duplicate lift physics |

## Project references (baseline)

| Reference | Notes |
|-----------|--------|
| **`Beep.OilandGas.PPDM.Models`** | Metadata / table alignment where used |
| **`Beep.OilandGas.GasProperties`** | Confirm **actual usage** in `.cs`; remove if unused to slim the package |
| **SkiaSharp** | Rendering only; core math must not **require** Skia for headless tests |

## Baseline gaps

| Gap | Notes | Phase |
|-----|--------|--------|
| **No `Beep.OilandGas.PumpPerformance.Tests`** | Regression risk for curves, NPSH, affinity, ESP | 2 |
| ~~README / summaries namespace drift~~ | Addressed in README + phase/ESP docs pass | **Done** |
| **`IPumpPerformanceService` in feature project** | Unlike **`IOilPropertiesService`** in **`Models.Core.Interfaces`** — decide if a **shared** contract is needed for **ApiService** / Web | 1, 3 |
| **`PumpPerformanceService.CalculateCFactorAsync`** | Formula vs industry naming — document and test (C-factor / IPR-related naming collisions) | 1, 2 |
| **Duplicate `<PropertyGroup>` / `<ImplicitUsings>`** in **`.csproj`** | Clean up build hygiene | 3 |
| **ENHANCEMENT_PLAN.md** vs implemented surface | Roadmap out of date vs rich **`Calculations/`** — reconcile or archive | 3 |
| **Industry scenario matrix** | README lists features; **phase 4** ties wells / ESP / pipeline / VSD to tests and applicability warnings | 4 |

## Exit criteria

- [ ] Phase docs **00–04** + **08** accepted as source of truth for PumpPerformance work.
- [ ] **`MASTER-TODO-TRACKER.md`** links **`.plans/README.md`**.
- [ ] Inventory row for **tests** moves from “missing” to “planned/started” in tracker.
