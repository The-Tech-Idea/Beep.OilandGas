# Beep.OilandGas.WellTestAnalysis — pressure transient / well test library

A .NET library for well test analysis and pressure transient analysis (PTA) in oil and gas operations.

**Planning:** [`.plans/README.md`](.plans/README.md) · **Tracker:** [`MASTER-TODO-TRACKER.md`](MASTER-TODO-TRACKER.md) · **API:** [`API.md`](API.md)

## `WellTestAnalysisService` (default)

The in-library **`WellTestAnalysisService`** implements **`IWellTestAnalysisService`** for **Horner / MDH / drawdown**, **derivatives**, **`IdentifyReservoirModel`**, and **`ValidateTestDataAsync`**. It throws **`NotImplementedException`** for persistence, type-curve matching, multi-rate/deconvolution, exports, and plot bytes — register your own **`IWellTestAnalysisService`** in DI when you need those (see **`API.md`**).

## Features

### Core functionality

- **Build-up analysis** (Horner, MDH)
- **Drawdown analysis** (constant-rate semi-log)
- **Diagnostic plots** and **derivative** helpers (SkiaSharp rendering)
- **Permeability**, **skin**, **reservoir pressure**, and related indicators on **`WELL_TEST_ANALYSIS_RESULT`**

### Visualization

- SkiaSharp-based plots, zoom/pan, PNG export (see **`WellTestRenderer`**)

## Quick start

Wire types live in **`Beep.OilandGas.Models.Data.WellTestAnalysis`**. Analysis entry points are under **`Beep.OilandGas.WellTestAnalysis`**.

```csharp
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis;

var testData = new WELL_TEST_DATA
{
    FLOW_RATE = 1000m,
    WELLBORE_RADIUS = 0.25m,
    FORMATION_THICKNESS = 50m,
    POROSITY = 0.20m,
    TOTAL_COMPRESSIBILITY = 1e-5m,
    OIL_VISCOSITY = 1.5m,
    OIL_FORMATION_VOLUME_FACTOR = 1.2m,
    RESERVOIR_TEMPERATURE = 200m,
    TEST_TYPE = WellTestType.BuildUp.ToString(),
    PRODUCTION_TIME = 720m,
    Time = [0.01, 0.1, 0.5, 1, 2, 5, 10, 20, 50, 100],
    Pressure = [3000, 2990, 2980, 2970, 2960, 2950, 2940, 2930, 2920, 2910]
};

var result = WellTestAnalyzer.AnalyzeBuildUp(testData);
Console.WriteLine($"Permeability: {result.PERMEABILITY} md");
Console.WriteLine($"Skin: {result.SKIN_FACTOR}");
Console.WriteLine($"Reservoir pressure: {result.RESERVOIR_PRESSURE} psi");
```

Build-up analysis requires **`TEST_TYPE`** parseable as **`WellTestType.BuildUp`** (case-insensitive, e.g. **`BUILDUP`**) and a positive **`PRODUCTION_TIME`** (hours). Use strictly increasing **`Time`** (hours) and matching **`Pressure`** (psi) lists.

## Installation

```bash
dotnet add package Beep.OilandGas.WellTestAnalysis
```

*(Package id matches the project assembly name.)*

## Documentation

- [`API.md`](API.md) — public surface overview  
- [`USAGE_EXAMPLES.md`](USAGE_EXAMPLES.md) — Skia rendering and derivatives  
- [`.plans/`](.plans/README.md) — phased engineering and test plans  

## License

MIT License
