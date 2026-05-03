# Phase 0 — Well test analysis overview and baseline

## Purpose

**Well test / pressure transient analysis (PTA)** supports drawdown and build-up interpretation, semi-log and derivative workflows, permeability and skin screening, gas **m(p)** build-up, type-curve references, and **Skia** visualization. The shipping assembly is **`Beep.OilandGas.WellTestAnalysis`**; wire types live under **`Beep.OilandGas.Models.Data.WellTestAnalysis`** and the shared contract is **`IWellTestAnalysisService`** in **`Beep.OilandGas.Models.Core.Interfaces`**.

## Inventory (this assembly)

| Area | Location |
|------|-----------|
| Entry / façade | **`WellTestAnalyzer.cs`**, **`Services/WellTestAnalysisService.cs`** |
| Build-up / drawdown | **`Calculations/BuildUpAnalysis.cs`**, **`DrawdownAnalysis.cs`** |
| Derivatives / typing | **`Calculations/DerivativeAnalysis.cs`**, **`TypeCurveLibrary.cs`** |
| Gas | **`Calculations/GasWellAnalysis.cs`** |
| Constants | **`Constants/WellTestConstants.cs`** |
| Validation | **`Validation/WellTestDataValidator.cs`** |
| Rendering | **`Rendering/WellTestRenderer.cs`**, **`WellTestRendererConfiguration.cs`** |
| Interaction | **`Interaction/WellTestInteractionHandler.cs`** |
| Exceptions | **`Exceptions/WellTestException.cs`** |

## Shared contracts

| Area | Location |
|------|----------|
| **`IWellTestAnalysisService`** | **`Beep.OilandGas.Models.Core.Interfaces`** |
| **`WELL_TEST_DATA`**, **`WELL_TEST_ANALYSIS_RESULT`**, **`PRESSURE_TIME_POINT`**, … | **`Beep.OilandGas.Models.Data.WellTestAnalysis`** |

## Project references (baseline)

| Reference | Role |
|-----------|------|
| **`Beep.OilandGas.Models`** | DTOs / entities for well test |
| **`Beep.OilandGas.PPDM.Models`** | PPDM alignment |
| **SkiaSharp** (+ Extended, Svg) | **`Rendering/`** |
| **Microsoft.Extensions.Logging.Abstractions** | **`WellTestAnalysisService`** |

## Baseline gaps

| Gap | Notes | Phase |
|-----|--------|--------|
| **No `Beep.OilandGas.WellTestAnalysis.Tests`** | Regression risk for Horner, MDH, derivative, gas path | 2 |
| **README / ENHANCEMENT_PLAN** use legacy **`Beep.WellTestAnalysis`** / wrong `using` | Conflicts with actual **`Beep.OilandGas.WellTestAnalysis`** | 3 |
| **`USAGE_EXAMPLES.md`** / **API.md** reference | Verify paths and namespaces | 3 |
| **`.csproj`** | No **`PackageReadmeFile`** / packed README pattern yet (compare **GasProperties** / **PumpPerformance**) | 3 |
| **Superposition / deconvolution / variable rate** | Roadmap items; confirm what is stub vs implemented | 1 |
| **Fluid PVT source of truth** | Prefer **`IOilPropertiesService`** / **`IGasPropertiesService`** (or flash) for μ, B, Z in workflows—not duplicated constants in UI-only paths | 4 |

## Exit criteria

- [ ] Phased docs **00–04** + **08** accepted as the working plan.
- [x] **`MASTER-TODO-TRACKER.md`** links **`.plans/README.md`** (phase table).
- [ ] Repo root **Related feature trackers** lists WellTestAnalysis (optional but recommended).
