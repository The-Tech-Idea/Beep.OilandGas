# Beep.OilandGas.WellTestAnalysis — usage examples

Wire types: **`Beep.OilandGas.Models.Data.WellTestAnalysis`**. Library API: **`Beep.OilandGas.WellTestAnalysis`**.

See **[`README.md`](README.md)** for a minimal Horner build-up sample using **`WELL_TEST_DATA`** and **`WellTestAnalyzer.AnalyzeBuildUp`**.

## Pressure / derivative series

Assume **`testData`** is a validated **`WELL_TEST_DATA`** instance (see **[`README.md`](README.md)**).

```csharp
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis.Rendering;
using SkiaSharp;

var pressurePoints = testData.Time.Zip(testData.Pressure, (t, p) => new PRESSURE_TIME_POINT { TIME = t, PRESSURE = p }).ToList();
var derivativePoints = WellTestAnalyzer.CalculateDerivative(pressurePoints);

var renderer = new WellTestRenderer(new WellTestRendererConfiguration { Title = "Build-up — pressure & derivative" });
renderer.SetPressureData(pressurePoints);
renderer.SetDerivativeData(derivativePoints);
// renderer.SetAnalysisResult(analysisResult);

renderer.ExportToPng("welltest_plot.png", width: 1200, height: 800);
```

## Validation only

```csharp
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis.Validation;

WellTestDataValidator.Validate(testData);
```

For phased test vectors and golden-file strategy, see **[`.plans/02_Tests_Verification_And_Golden_Vectors.md`](.plans/02_Tests_Verification_And_Golden_Vectors.md)**.
