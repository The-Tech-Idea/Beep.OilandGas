# Beep.DCA - Decline Curve Analysis Library

A comprehensive .NET library for performing Decline Curve Analysis (DCA) on oil and gas production data. This library provides multiple decline curve methods, statistical analysis, and advanced features for production forecasting.

## Features

### Core Functionality
- **Multiple Decline Methods**: Exponential, Harmonic, and Hyperbolic decline curves
- **Nonlinear Regression**: Advanced curve fitting using iterative optimization
- **Gas & Oil Wells**: Support for both oil and gas well analysis
- **Material Balance Method**: MBM-based parameter estimation

### Statistical Analysis
- **R² and Adjusted R²**: Coefficient of determination metrics
- **RMSE & MAE**: Error metrics for model evaluation
- **AIC & BIC**: Model comparison criteria
- **Confidence Intervals**: Parameter uncertainty quantification
- **Prediction Intervals**: Forecast uncertainty bounds
- **Residual Analysis**: Model diagnostics

### Advanced Features
- **Power-Law Exponential Decline**: Extended decline model
- **Stretched Exponential Decline**: Alternative decline formulation
- **Multi-Segment Decline**: Different decline characteristics over time periods
- **Async Processing**: Asynchronous operations for large datasets
- **Parallel Processing**: Multi-well batch analysis
- **Data Import/Export**: CSV and JSON support
- **Visualization Support**: Plot data generation

## Quick Start

### Basic Usage

```csharp
using Beep.DCA;
using System;
using System.Collections.Generic;

// Prepare production data
var productionData = new List<double> { 1000, 900, 810, 730, 657, 591 };
var timeData = new List<DateTime>
{
    new DateTime(2020, 1, 1),
    new DateTime(2020, 2, 1),
    new DateTime(2020, 3, 1),
    new DateTime(2020, 4, 1),
    new DateTime(2020, 5, 1),
    new DateTime(2020, 6, 1)
};

// Perform DCA analysis
var parameters = DCAManager.GenerateDCA(productionData, timeData);
Console.WriteLine($"Initial Production Rate (qi): {parameters[0]:F2}");
Console.WriteLine($"Decline Exponent (b): {parameters[1]:F4}");
```

### Comprehensive Analysis with Statistics

```csharp
using Beep.DCA;
using Beep.DCA.Results;

var manager = new DCAManager();
var result = manager.AnalyzeWithStatistics(productionData, timeData);

Console.WriteLine(result.GetSummary());
// Output:
// Fit Results:
//   Converged: True
//   Iterations: 100
//   R²: 0.9985
//   Adjusted R²: 0.9982
//   RMSE: 12.34
//   MAE: 9.87
//   AIC: 45.67
//   BIC: 48.90
```

### Using Different Decline Methods

```csharp
// Exponential decline
double q_exp = DCAGenerator.ExponentialDecline(qi: 1000, di: 0.1, t: 30);

// Harmonic decline
double q_harm = DCAGenerator.HarmonicDecline(qi: 1000, di: 0.1, t: 30);

// Hyperbolic decline
double q_hyp = DCAGenerator.HyperbolicDecline(qi: 1000, di: 0.1, t: 30, b: 0.5);
```

### Advanced Decline Methods

```csharp
using Beep.DCA.AdvancedDeclineMethods;

// Power-Law Exponential Decline
double q_ple = PowerLawExponentialDecline.CalculateProductionRate(
    qi: 1000, di: 0.1, t: 30, n: 0.7);

// Stretched Exponential Decline
double q_se = StretchedExponentialDecline.CalculateProductionRate(
    qi: 1000, di: 0.1, t: 30, beta: 0.8);
```

### Multi-Segment Decline

```csharp
using Beep.DCA.MultiSegment;

var segments = new List<DeclineSegment>
{
    new DeclineSegment
    {
        StartTime = 0,
        EndTime = 365,
        InitialProductionRate = 1000,
        DeclineRate = 0.2,
        DeclineExponent = 0.3,
        DeclineType = DeclineType.Hyperbolic
    },
    new DeclineSegment
    {
        StartTime = 365,
        EndTime = 730,
        InitialProductionRate = 500,
        DeclineRate = 0.1,
        DeclineExponent = 0.5,
        DeclineType = DeclineType.Hyperbolic
    }
};

double production = MultiSegmentDecline.CalculateProductionRate(segments, t: 400);
```

### Async Processing

```csharp
using Beep.DCA.Performance;
using System.Threading.Tasks;

// Single well async analysis
var result = await AsyncDCACalculator.FitCurveAsync(
    productionData, timeData, qi: 1000, di: 0.1);

// Multiple wells in parallel
var wells = new Dictionary<string, (List<double>, List<DateTime>)>
{
    ["Well-1"] = (prod1, time1),
    ["Well-2"] = (prod2, time2),
    ["Well-3"] = (prod3, time3)
};

var results = await AsyncDCACalculator.ProcessMultipleWellsAsync(wells);
```

### Data Import/Export

```csharp
using Beep.DCA.DataImportExport;

// Import from CSV
var (productionData, timeData) = DataImporter.ImportFromCsv(
    "production_data.csv", hasHeader: true);

// Export results to CSV
DataExporter.ExportToCsv(result, "results.csv", includeStatistics: true);

// Export to JSON
DataExporter.ExportToJson(result, "results.json");
```

### Multi-Well Analysis

```csharp
using Beep.DCA.MultiWell;

var wells = new Dictionary<string, (List<double>, List<DateTime>)>
{
    ["Well-A"] = (prodA, timeA),
    ["Well-B"] = (prodB, timeB)
};

var analyses = await MultiWellAnalyzer.AnalyzeWellsAsync(wells);

// Generate type curve
var typeCurveParams = MultiWellAnalyzer.GenerateTypeCurve(analyses);

// Statistical summary
var summary = MultiWellAnalyzer.CalculateStatisticalSummary(analyses);
```

### Visualization

```csharp
using Beep.DCA.Visualization;

// Create plot from fit result
var plot = DeclineCurvePlot.FromFitResult(result, timeData, forecastDays: 365);

// Export plot data to CSV for external plotting tools
plot.ExportToCsv("plot_data.csv");
```

## Mathematical Background

### Exponential Decline
```
q(t) = qi * exp(-Di * t)
```
Where:
- `qi` = initial production rate
- `Di` = initial decline rate
- `t` = time

### Harmonic Decline
```
q(t) = qi / (1 + Di * t)
```

### Hyperbolic Decline
```
q(t) = qi / (1 + b * Di * t)^(1/b)
```
Where `b` is the decline exponent (0 < b < 1).

## API Reference

### Core Classes

- **DCAManager**: High-level manager for DCA operations
- **DCAGenerator**: Static methods for decline curve calculations
- **NonlinearRegression**: Nonlinear least-squares solver
- **DCAFitResult**: Comprehensive fit results with statistics

### Statistical Analysis

- **StatisticalAnalysis**: Static methods for R², RMSE, MAE, AIC, BIC, confidence intervals

### Advanced Methods

- **PowerLawExponentialDecline**: Power-law exponential decline model
- **StretchedExponentialDecline**: Stretched exponential decline model
- **MultiSegmentDecline**: Multi-segment decline curves

### Performance

- **AsyncDCACalculator**: Asynchronous DCA calculations

### Data Handling

- **DataImporter**: Import production data from CSV
- **DataExporter**: Export results to CSV/JSON

### Multi-Well

- **MultiWellAnalyzer**: Batch processing and type curve generation

### Visualization

- **DeclineCurvePlot**: Plot data generation for visualization

## Error Handling

The library uses custom exceptions for better error handling:

- **DCAException**: Base exception for all DCA-related errors
- **InvalidDataException**: Thrown when input data is invalid
- **ConvergenceException**: Thrown when numerical algorithms fail to converge

## Performance

- Single well analysis: < 100ms for datasets up to 1000 points
- Batch processing: < 1s per well for 100 wells (with parallel processing)
- Memory usage: < 100MB for typical workloads

## Requirements

- .NET 6.0 or later
- No external dependencies (pure .NET implementation)

## License

MIT License

## Contributing

Contributions are welcome! Please ensure all code follows the existing style and includes appropriate documentation.

## Support

For issues, questions, or contributions, please visit the project repository.

