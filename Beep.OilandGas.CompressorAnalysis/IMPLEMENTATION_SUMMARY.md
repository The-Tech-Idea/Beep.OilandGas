# Beep.OilandGas.CompressorAnalysis — implementation summary

## Role in the solution

Library for **centrifugal and reciprocating** compressor power, head, temperature, and pressure-flow calculations.

- **Table-shaped extension entities** (`COMPRESSOR_OPERATING_CONDITIONS`, `CENTRIFUGAL_COMPRESSOR_PROPERTIES`, `RECIPROCATING_COMPRESSOR_PROPERTIES`, `COMPRESSOR_POWER_RESULT`, `COMPRESSOR_PRESSURE_RESULT`, **`R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`**) live in **`Beep.OilandGas.CompressorAnalysis.Data`** (`Data/Tables/`).
- **Wire / orchestration types** (`CompressorAnalysisRequest`, **`CompressorAnalysisResult`**, **`CompressorAnalysisWellKnown`**) stay in **`Beep.OilandGas.Models.Data.Calculations`** for cross-layer use.

Domain logic is in **`Calculations/`**, **`Validation/CompressorValidator`**, and **`Services/CompressorAnalysisService`** (+ **`CompressorAnalysisService.Advanced`** partial).

Packaged facility-level runs use **`ICalculationService.PerformCompressorAnalysisAsync`** in **LifeCycle** (`PPDMCalculationService.Facilities`), which delegates to **`ICompressorAnalysisService`** (`CalculateRequiredPressureAsync` / power paths — same physics as **`CompressorPressureCalculator`** where applicable).

## API and client

- **`Beep.OilandGas.ApiService/Controllers/CompressorController.cs`** — REST endpoints delegate to **`ICompressorAnalysisService`** (after normalization).
- **`CalculationsController`** — POST **`/api/calculations/compressor`** through **`ICalculationService`** (facility-oriented **`CompressorAnalysisRequest`**).
- **`Beep.OilandGas.Client`** — `AnalysisService.Compressor.cs` maps to **`/api/compressor/*`** routes.

## Canonical service interface

**`ICompressorAnalysisService`** — **`Beep.OilandGas.CompressorAnalysis.Core.Interfaces`** — **`CalculateCentrifugalPowerAsync`**, **`CalculateReciprocatingPowerAsync`**, **`CalculateRequiredPressureAsync`**. Implemented by **`CompressorAnalysisService`** (`Services/CompressorAnalysisService.Contract.cs`). Extended helpers remain on concrete partials only.

## Project layout (high level)

| Area | Purpose |
|------|---------|
| `Data/Tables/` | **`COMPRESSOR_*`**, **`R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`** |
| `Data/Constants/` | **`CompressorConstants`**, **`CompressorAnalysisReferenceSets`**, **`CompressorAnalysisReferenceCodeSeed`** |
| `Core/Interfaces/` | **`ICompressorAnalysisService`** |
| `Calculations/` | Core calculators, pressure, multistage, optimization helpers |
| `Services/` | **`CompressorAnalysisService`**, **`CompressorAnalysisService.Advanced`** |
| `Validation/` | **`CompressorValidator`** |
| `Exceptions/` | **`CompressorException`** hierarchy |
| `Modules/` | **`CompressorAnalysisModule`** — **`EntityTypes`** ×6, **`SeedAsync`** for **`R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`** |

## Dependencies

- **`Beep.OilandGas.Models`** — shared **`ModelEntityBase`**, **`CompressorAnalysisRequest`** / result types under **`Data/Calculations`**
- **`Beep.OilandGas.GasProperties`** — gas properties / Z-factor where applicable
- **`Beep.OilandGas.PPDM39`** / **`Beep.OilandGas.PPDM39.DataManagement`** — module setup patterns; **no** domain coupling to DataManagement implementation required for pure math paths

## Packaging

**`README.md`** is included in the NuGet package via **`PackageReadmeFile`**. **Nullable** reference types are enabled.

## Further reading

- [README.md](README.md)
- [.plans/README.md](.plans/README.md)
- [MASTER-TODO-TRACKER.md](MASTER-TODO-TRACKER.md)
