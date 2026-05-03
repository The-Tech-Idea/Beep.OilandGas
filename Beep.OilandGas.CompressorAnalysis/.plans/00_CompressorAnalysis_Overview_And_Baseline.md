# Phase 0 — Overview and baseline (CompressorAnalysis)

## Purpose

Define what **CompressorAnalysis** owns, what it depends on, and known gaps before contract, API, or module work.

## Scope

| In scope | Out of scope (delegate elsewhere) |
|----------|-----------------------------------|
| Centrifugal / reciprocating **`Calculations/*`**, **`CompressorAnalysisService`**, validation, constants | **EF Core** migrations; hand-written per-feature SQL in `Models/Scripts` (use entities + **ModuleSetup** + tooling per **CLAUDE.md**) |
| **`Beep.OilandGas.CompressorAnalysis.Data`** table entities (`COMPRESSOR_*`, **`R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`**) | Full **facility** equipment catalog CRUD (PPDM **FACILITY** / **EQUIPMENT**) — map at integration layer |
| HTTP surface in **`CompressorController`**, calculation packaging via **`ICalculationService`** | Blazor page work unless explicitly tasked |

## Baseline inventory (update as the repo changes)

| Area | Paths / artifacts |
|------|-------------------|
| Calculators | `Calculations/CentrifugalCompressorCalculator.cs`, `ReciprocatingCompressorCalculator.cs`, `CompressorPressureCalculator.cs`, `MultistageCompressor.cs`, `AdvancedCompressorCalculator.cs`, `CompressorOptimization.cs` |
| Service | `Services/CompressorAnalysisService.cs`, `Services/CompressorAnalysisService.Advanced.cs` |
| Validation / exceptions | `Validation/CompressorValidator.cs`, `Exceptions/CompressorException.cs` |
| Constants | `Data/Constants/*` (`CompressorConstants`, reference seed / sets) |
| Feature table entities | `Beep.OilandGas.CompressorAnalysis/Data/Tables/*` |
| Shared wire DTOs | `Beep.OilandGas.Models/Data/Calculations/*` (`CompressorAnalysisRequest`, **`CompressorAnalysisWellKnown`**, etc.) |
| Facility orchestration | `Beep.OilandGas.LifeCycle/.../PPDMCalculationService.Facilities.cs` → **`PerformCompressorAnalysisAsync`** |
| Direct API | `Beep.OilandGas.ApiService/Controllers/CompressorController.cs` |
| Packaged API | `Beep.OilandGas.ApiService/Controllers/CalculationsController.cs` (compressor region) |
| Client | `Beep.OilandGas.Client/.../AnalysisService.Compressor.cs` |
| Project | `Beep.OilandGas.CompressorAnalysis.csproj` — references **Models**, **GasProperties**, **PPDM39**, **PPDM39.DataManagement** |

## Known gaps (baseline) — refresh periodically

| Resolved / status | Notes |
|-------------------|--------|
| **`ICompressorAnalysisService`** | **`Beep.OilandGas.CompressorAnalysis.Core.Interfaces`**; **`CompressorAnalysisService`** + ApiService DI |
| **`CompressorAnalysisModule`** | **`IModuleSetup`** + entity types registered |
| **`FormatIdForTable`** | Uses **`IPPDM39DefaultsRepository`** when supplied by DI |
| **LifeCycle vs API** | **`PerformCompressorAnalysisAsync`** branches **`AnalysisType`**: **`PRESSURE`** → pressure calculator; **`POWER`**/**`EFFICIENCY`** → centrifugal/recip via **`ICompressorAnalysisService`** (aligned with **`CompressorController`** physics) |
| **Tests** | **`Beep.OilandGas.CompressorAnalysis.Tests`** + **`PPDMCalculationServiceCompressorTests`** |
| **Models cleanup** | Compressor **table** types live in **`Beep.OilandGas.CompressorAnalysis.Data`**; legacy **`Models/Data/CompressorAnalysis`** and **`*.bak`** files removed |
| **Wire constants** | **`CompressorAnalysisWellKnown`**, **`CalculationRunStatus`** for orchestration |

| Remaining | Notes |
|-----------|--------|
| **SkiaSharp / rendered performance maps** | Curve math is tested; chart rendering stays optional separate boundary |

## Governance checklist (every change set)

- [ ] New **shared** cross-library API → interface in **`Beep.OilandGas.Models.Core.Interfaces`** (unless signatures depend only on **`CompressorAnalysis.Data`** — then **`Beep.OilandGas.CompressorAnalysis.Core.Interfaces`**)
- [ ] No new `DTO` namespaces; use **`Beep.OilandGas.Models.Data`**
- [ ] **ModuleSetup** for extension tables only — not standard PPDM core tables
- [ ] **Program.cs** factory registration and order if adding services

## Exit criteria (Phase 0)

- [x] Inventory matches repository layout.
- [ ] Stakeholders agree priority: **contract (phase 1)**, **module (phase 3)**, **test project (phase 5)**, or **API parity (phase 4)**.
