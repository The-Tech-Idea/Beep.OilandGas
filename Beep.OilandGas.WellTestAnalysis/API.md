# Beep.OilandGas.WellTestAnalysis — API overview

## Namespaces

| Namespace | Role |
|-----------|------|
| `Beep.OilandGas.WellTestAnalysis` | **`WellTestAnalyzer`** static entry points |
| `Beep.OilandGas.WellTestAnalysis.Calculations` | Horner / MDH / drawdown / derivative / gas build-up implementations |
| `Beep.OilandGas.WellTestAnalysis.Validation` | **`WellTestDataValidator`** |
| `Beep.OilandGas.WellTestAnalysis.Rendering` | **`WellTestRenderer`**, **`WellTestRendererConfiguration`** |
| `Beep.OilandGas.WellTestAnalysis.Services` | **`WellTestAnalysisService`** — implements **`IWellTestAnalysisService`** (logging, **`ANALYSIS_ID`**, **`WELL_UWI`**, audit fields) |
| `Beep.OilandGas.WellTestAnalysis.Exceptions` | **`InvalidWellTestDataException`**, **`AnalysisConvergenceException`**, … |
| `Beep.OilandGas.Models.Data.WellTestAnalysis` | **`WELL_TEST_DATA`**, **`WELL_TEST_ANALYSIS_RESULT`**, **`PRESSURE_TIME_POINT`**, **`WellTestType`** |

## Main types

### `WellTestAnalyzer`

- **`AnalyzeBuildUp(WELL_TEST_DATA)`** — Horner build-up.  
- **`AnalyzeBuildUpMDH(WELL_TEST_DATA)`** — MDH build-up.  
- **`AnalyzeDrawdown(WELL_TEST_DATA)`** — drawdown (`TEST_TYPE` = **`Drawdown`**).  
- **`AnalyzeGasBuildUp(...)`** — gas pseudo-pressure route.  
- **`CalculateDerivative(List<PRESSURE_TIME_POINT>, double smoothing)`** — diagnostic derivative series.  
- **`DerivativeAnalysis.CalculateBourdetDerivative`** — default log-window **`L`** matches **`WellTestConstants.DefaultDerivativeSmoothing`**.  
- **`IdentifyReservoirModel(List<PRESSURE_TIME_POINT>)`** — simple model hint from derivative shape.

### `WellTestAnalysisService`

- **`AnalyzeBuildUpHornerAsync`** / **`AnalyzeBuildUpMDHAsync`** — call **`WellTestAnalyzer`** then stamp **`ANALYSIS_ID`**, **`WELL_UWI`**, **`ANALYSIS_DATE`**, **`ANALYSIS_BY_USER`**.  
- **`AnalyzeDrawdownAsync`** — delegates to **`WellTestAnalyzer.AnalyzeDrawdown`** (same audit stamping as build-up).  
- **`CalculateDerivativeAsync`** — default **`smoothingFactor`** matches **`WellTestConstants.DefaultDerivativeSmoothing`** (**0.1**), aligned with **`IWellTestAnalysisService`**.  
- **`ValidateTestDataAsync`** — runs **`WellTestDataValidator.Validate`**; returns **`IsValid`**, **`Errors`**, and **`DATA_QUALITY_SCORE`** (**1.0** when passed, **0** when failed) without throwing for invalid input.  
- **Not implemented in the default class** (throws **`NotImplementedException`** after a warning log): type-curve matching, multi-rate, deconvolution, boundary detection, persistence (**`Save`/`Get`/`Update`**), reporting/export, plot bytes, and method comparison. Hosts should register a custom **`IWellTestAnalysisService`** for those features.

### `WELL_TEST_DATA` (analysis-relevant)

Scalar PPDM-oriented fields used by calculators include **`FLOW_RATE`**, **`WELLBORE_RADIUS`**, **`FORMATION_THICKNESS`**, **`POROSITY`**, **`TOTAL_COMPRESSIBILITY`**, **`OIL_VISCOSITY`**, **`OIL_FORMATION_VOLUME_FACTOR`**, **`TEST_TYPE`**, **`PRODUCTION_TIME`** (required for build-up), **`RESERVOIR_TEMPERATURE`**, plus optional gas fields for gas workflows.

Transient series use parallel lists **`Time`** and **`Pressure`** (`List<double>`, hours and psi).

**`TEST_TYPE`** is matched to **`WellTestType`** with **case-insensitive** parsing (for example **`BuildUp`**, **`BUILDUP`**, **`drawdown`**).

### `WELL_TEST_ANALYSIS_RESULT`

Primary outputs include **`PERMEABILITY`**, **`SKIN_FACTOR`**, **`RESERVOIR_PRESSURE`**, **`PRODUCTIVITY_INDEX`**, **`FLOW_EFFICIENCY`**, **`ANALYSIS_METHOD`**, and other fields populated by the specific analysis path.

## See also

- [`README.md`](README.md) — quick start  
- [`.plans/01_Calculations_Validation_Services_And_PPDM_Alignment.md`](.plans/01_Calculations_Validation_Services_And_PPDM_Alignment.md) — limits and PPDM alignment  
