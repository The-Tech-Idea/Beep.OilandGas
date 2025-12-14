# Beep.DCA Visualization Guide

## Overview

The Beep.DCA visualization module provides comprehensive plotting capabilities for decline curve analysis results. It supports multiple plot types, comparison charts, residual analysis, and various export formats.

## Plot Types

### 1. Decline Curve Plot

Basic decline curve visualization with observed, predicted, and forecast data.

```csharp
using Beep.DCA.Visualization;
using Beep.DCA.Results;

// Create plot from fit result
var plot = DeclineCurvePlot.FromFitResult(result, timeData, forecastDays: 365);

// Customize plot
plot.Title = "Well A - Production Decline";
plot.XAxisLabel = "Days Since Start";
plot.YAxisLabel = "Production Rate (bbl/day)";

// Export to CSV
plot.ExportToCsv("decline_curve.csv", includeMetadata: true);

// Create logarithmic version
var logPlot = plot.ToLogScale();
logPlot.ExportToCsv("decline_curve_log.csv");
```

### 2. Plot with Prediction Intervals

Includes uncertainty bounds around predictions.

```csharp
// Create plot with intervals
var plotWithIntervals = PlotWithIntervals.FromFitResult(
    result, 
    timeData, 
    forecastDays: 365,
    confidenceLevel: 0.95);

plotWithIntervals.Title = "Decline Curve with 95% Prediction Intervals";
plotWithIntervals.ExportToCsv("plot_with_intervals.csv");
```

### 3. Residual Plot

Model diagnostics including residual vs predicted, residual vs time, and Q-Q plots.

```csharp
// Generate residual plot
var residualPlot = ResidualPlot.FromFitResult(result, timeData);

residualPlot.Title = "Model Residual Analysis";
residualPlot.ExportToCsv("residuals.csv");

// Or use PlotGenerator
var residualPlot2 = PlotGenerator.GenerateResidualPlot(result, timeData);
```

### 4. Comparison Plot

Compare multiple wells or decline methods side-by-side.

```csharp
// Compare multiple wells
var results = new Dictionary<string, (DCAFitResult result, List<DateTime> timeData)>
{
    ["Well-A"] = (resultA, timeDataA),
    ["Well-B"] = (resultB, timeDataB),
    ["Well-C"] = (resultC, timeDataC)
};

var comparisonPlot = PlotGenerator.GenerateComparisonPlot(
    results, 
    includeForecast: true, 
    forecastDays: 730);

// Customize series
comparisonPlot.Series[0].Color = "#FF0000"; // Red
comparisonPlot.Series[1].LineStyle = "dashed";
comparisonPlot.Series[2].MarkerStyle = "circle";

// Toggle visibility
comparisonPlot.ToggleSeriesVisibility("Well-B");

// Export
comparisonPlot.ExportToCsv("comparison.csv");
```

### 5. Custom Series

Add custom data series to any plot.

```csharp
var plot = new DeclineCurvePlot();

// Add custom series
var customPoints = new List<PlotPoint>
{
    new PlotPoint { Time = 0, ProductionRate = 1000, Label = "Start" },
    new PlotPoint { Time = 100, ProductionRate = 800, Label = "Day 100" },
    new PlotPoint { Time = 200, ProductionRate = 600, Label = "Day 200" }
};

plot.AddSeries(customPoints, "Custom");
```

## Plot Configuration

Customize plot appearance with `PlotConfiguration`:

```csharp
var config = new PlotConfiguration
{
    Width = 1200,
    Height = 800,
    UseLogScale = false,
    ShowGrid = true,
    ShowLegend = true,
    BackgroundColor = "#FFFFFF",
    GridColor = "#E0E0E0",
    ObservedColor = "#2196F3",    // Blue
    PredictedColor = "#FF5722",   // Red
    ForecastColor = "#FF9800",    // Orange
    PointSize = 5.0,
    LineWidth = 2.5,
    FontSize = 14.0,
    ShowPredictionIntervals = true,
    PredictionIntervalConfidence = 0.95
};

var plot = DeclineCurvePlot.FromFitResult(result, timeData);
plot.Configuration = config;
```

## Export Formats

### CSV Export

```csharp
// Basic CSV export
plot.ExportToCsv("plot.csv");

// With metadata
plot.ExportToCsv("plot.csv", includeMetadata: true);

// Multiple plots to single file
var plots = new Dictionary<string, object>
{
    ["DeclineCurve"] = declinePlot,
    ["Residuals"] = residualPlot,
    ["Comparison"] = comparisonPlot
};

PlotGenerator.ExportMultiplePlotsToCsv(plots, "all_plots.csv");
```

### JSON Export

For web-based visualization tools:

```csharp
// Export single plot to JSON
string json = PlotGenerator.ExportToJson(plot);
File.WriteAllText("plot.json", json);

// Use with JavaScript charting libraries like:
// - Chart.js
// - D3.js
// - Plotly.js
```

## Advanced Features

### Logarithmic Scale

```csharp
// Create log-scale version
var logPlot = plot.ToLogScale();
logPlot.ExportToCsv("log_plot.csv");
```

### Plot Statistics

```csharp
// Get plot statistics
var stats = plot.GetStatistics();
Console.WriteLine($"Time Range: {stats["TimeRange"]} days");
Console.WriteLine($"Average Rate: {stats["AverageProductionRate"]} bbl/day");
```

### Series Management

```csharp
// Add series to comparison plot
var series = comparisonPlot.AddSeries("Custom Series", customPoints, "#00FF00");

// Remove series
comparisonPlot.RemoveSeries("Well-B");

// Toggle visibility
comparisonPlot.ToggleSeriesVisibility("Well-A");
```

## Integration Examples

### With Excel

```csharp
// Export to CSV and open in Excel
plot.ExportToCsv("plot.csv", includeMetadata: true);
// CSV can be directly opened in Excel for charting
```

### With Python/Matplotlib

```csharp
// Export to CSV
plot.ExportToCsv("data.csv");

// Then in Python:
// import pandas as pd
// import matplotlib.pyplot as plt
// df = pd.read_csv('data.csv')
// plt.plot(df[df['Type']=='Observed']['Time'], 
//          df[df['Type']=='Observed']['ProductionRate'], 'o')
// plt.plot(df[df['Type']=='Predicted']['Time'], 
//          df[df['Type']=='Predicted']['ProductionRate'], '-')
// plt.show()
```

### With JavaScript/Chart.js

```csharp
// Export to JSON
string json = PlotGenerator.ExportToJson(plot);

// Use in JavaScript:
// const data = JSON.parse(jsonString);
// const ctx = document.getElementById('chart').getContext('2d');
// new Chart(ctx, {
//     type: 'line',
//     data: {
//         datasets: [{
//             label: 'Observed',
//             data: data.observed,
//             borderColor: 'blue'
//         }, {
//             label: 'Predicted',
//             data: data.predicted,
//             borderColor: 'red'
//         }]
//     }
// });
```

## Best Practices

1. **Use appropriate plot types**:
   - Use `DeclineCurvePlot` for single well analysis
   - Use `ComparisonPlot` for multiple wells
   - Use `ResidualPlot` for model diagnostics
   - Use `PlotWithIntervals` when uncertainty is important

2. **Configure plots appropriately**:
   - Set meaningful titles and axis labels
   - Choose appropriate colors for different series
   - Use log scale for wide production ranges

3. **Export formats**:
   - CSV for Excel/statistical software
   - JSON for web applications
   - Include metadata for better documentation

4. **Performance**:
   - For large datasets, consider downsampling before plotting
   - Use forecast points sparingly for very long forecasts

## Color Schemes

Recommended color schemes for different plot types:

- **Observed Data**: Blue (#2196F3)
- **Predicted/Fitted**: Red (#FF5722)
- **Forecast**: Orange (#FF9800)
- **Confidence Intervals**: Light red with transparency (#FF572220)
- **Multiple Series**: Use distinct colors from the palette

## Example: Complete Visualization Workflow

```csharp
using Beep.DCA;
using Beep.DCA.Visualization;
using Beep.DCA.Results;

// 1. Perform DCA analysis
var manager = new DCAManager();
var result = manager.AnalyzeWithStatistics(productionData, timeData);

// 2. Create main decline curve plot
var mainPlot = DeclineCurvePlot.FromFitResult(result, timeData, forecastDays: 365);
mainPlot.Title = "Well Production Decline Analysis";
mainPlot.Configuration.ShowPredictionIntervals = true;

// 3. Create residual plot for diagnostics
var residualPlot = PlotGenerator.GenerateResidualPlot(result, timeData);

// 4. Export both plots
mainPlot.ExportToCsv("main_plot.csv", includeMetadata: true);
residualPlot.ExportToCsv("residuals.csv");

// 5. Export to JSON for web visualization
string json = PlotGenerator.ExportToJson(mainPlot);
File.WriteAllText("plot.json", json);

// 6. Get plot statistics
var stats = mainPlot.GetStatistics();
Console.WriteLine($"Plot Statistics: {string.Join(", ", stats.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
```

## Future Enhancements

Potential future additions:
- Direct image export (PNG, SVG)
- Interactive HTML export
- Real-time plot updates
- 3D visualization
- Animation support

