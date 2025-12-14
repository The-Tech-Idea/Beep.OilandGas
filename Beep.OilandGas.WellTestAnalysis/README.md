# Beep.WellTestAnalysis - Pressure Transient Analysis Library

A comprehensive .NET library for well test analysis and pressure transient analysis (PTA) in oil and gas operations.

## Features

### Core Functionality
- **Build-up Analysis**: Pressure build-up test interpretation
- **Drawdown Analysis**: Pressure drawdown test interpretation
- **Diagnostic Plots**: Log-log, semi-log, and derivative plots
- **Reservoir Model Identification**: Automatic model recognition
- **Permeability Calculation**: Formation permeability estimation
- **Skin Factor Calculation**: Wellbore damage/improvement quantification
- **Boundary Detection**: Reservoir boundaries and faults
- **Multi-rate Analysis**: Variable rate test analysis
- **Type Curve Matching**: Automated type curve matching

### Analysis Methods
- **Horner Method**: Semi-log analysis for build-up tests
- **Miller-Dyes-Hutchinson (MDH)**: Alternative build-up analysis
- **Agarwal Equivalent Time**: Rate normalization
- **Derivative Analysis**: Pressure derivative for model identification
- **Superposition**: Multi-rate superposition principle
- **Deconvolution**: Rate normalization and deconvolution

### Visualization
- SkiaSharp-based rendering
- Interactive plots with zoom/pan
- Multiple plot types (log-log, semi-log, derivative)
- Export capabilities (PNG, SVG)

## Quick Start

```csharp
using Beep.WellTestAnalysis;
using Beep.WellTestAnalysis.Analysis;

// Load test data
var testData = new WellTestData
{
    Time = new double[] { 0, 0.1, 0.5, 1, 2, 5, 10, 20, 50, 100 },
    Pressure = new double[] { 3000, 2950, 2900, 2850, 2800, 2750, 2720, 2700, 2680, 2670 },
    FlowRate = 1000, // BPD
    WellboreRadius = 0.25, // feet
    FormationThickness = 50, // feet
    Porosity = 0.20,
    TotalCompressibility = 1e-5, // psi^-1
    OilViscosity = 1.5, // cp
    OilFormationVolumeFactor = 1.2 // RB/STB
};

// Perform build-up analysis
var analysis = WellTestAnalyzer.AnalyzeBuildUp(testData);
Console.WriteLine($"Permeability: {analysis.Permeability} md");
Console.WriteLine($"Skin Factor: {analysis.SkinFactor}");
Console.WriteLine($"Reservoir Pressure: {analysis.ReservoirPressure} psi");
```

## Installation

```bash
dotnet add package Beep.WellTestAnalysis
```

## Documentation

See the [API Documentation](API.md) for detailed information about all classes and methods.

## License

MIT License

