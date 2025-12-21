# Beep.OilandGas.DCA - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.DCA** is a comprehensive .NET library for performing Decline Curve Analysis (DCA) on oil and gas production data. It provides multiple decline curve methods, statistical analysis, and advanced features for production forecasting.

### Key Capabilities
- Multiple Decline Methods: Exponential, Harmonic, and Hyperbolic decline curves
- Nonlinear Regression: Advanced curve fitting using iterative optimization
- Gas & Oil Wells: Support for both oil and gas well analysis
- Statistical Analysis: R², RMSE, MAE, AIC, BIC, confidence intervals
- Multi-Segment Decline: Different decline characteristics over time periods
- Async Processing: Asynchronous operations for large datasets
- Parallel Processing: Multi-well batch analysis

### Current Status
✅ **Fully Integrated** - Used in `PPDMCalculationService` for centralized production forecasting and decline analysis

### Architecture Decision
**Service:** `PPDMCalculationService` (Centralized Calculation Service)  
**Rationale:** Pure calculation/analysis project - performs mathematical decline curve analysis without operational data management. Centralized service provides consistent data mapping and result storage.

---

## Key Classes and Interfaces

### Main Classes

#### `DCAManager`
High-level manager for DCA operations.

**Key Methods:**
```csharp
public class DCAManager
{
    public DCAFitResult AnalyzeWithStatistics(
        List<double> productionData, 
        List<DateTime> timeData, 
        double initialQi, 
        double initialDi, 
        double confidenceLevel = 0.95);
    
    public async Task<DCAFitResult> AnalyzeAsync(
        List<double> productionData, 
        List<DateTime> timeData, 
        double initialQi, 
        double initialDi);
    
    public static double[] GenerateDCA(
        List<double> productionData, 
        List<DateTime> timeData);
}
```

#### `DCAGenerator`
Static methods for decline curve calculations.

**Key Methods:**
```csharp
public static class DCAGenerator
{
    public static double ExponentialDecline(double qi, double di, double t);
    public static double HarmonicDecline(double qi, double di, double t);
    public static double HyperbolicDecline(double qi, double di, double t, double b);
}
```

#### `DCAFitResult`
Comprehensive fit results with statistics.

**Key Properties:**
```csharp
public class DCAFitResult
{
    public double InitialProductionRate { get; set; }  // qi
    public double DeclineRate { get; set; }            // di
    public double DeclineExponent { get; set; }         // b
    public double RSquared { get; set; }               // R²
    public double AdjustedRSquared { get; set; }
    public double RMSE { get; set; }
    public double MAE { get; set; }
    public bool Converged { get; set; }
    public int Iterations { get; set; }
}
```

### Advanced Features

#### `MultiSegmentDecline`
Multi-segment decline curves for different time periods.

#### `AsyncDCACalculator`
Asynchronous DCA calculations for performance.

#### `MultiWellAnalyzer`
Batch processing and type curve generation.

---

## Integration with LifeCycle Services

### Current Integration

**Service:** `PPDMCalculationService`  
**Location:** `Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService`

### Integration Points

1. **DCA Analysis Method**
   - Method: `PerformDCAAnalysisAsync(DCARequest request)`
   - Retrieves production data from PPDM database
   - Performs DCA using `DCAManager`
   - Stores results in `DCA_CALCULATION` table

2. **Data Flow**
   ```
   Production Data (PPDM) → Extract → DCAManager → DCAFitResult → DCAResult DTO → PPDM Database
   ```

3. **Storage**
   - Results stored in `DCA_CALCULATION` table
   - Uses `PPDMGenericRepository` for data access
   - Links to `WELL`, `POOL`, or `FIELD` via IDs

### Service Methods

```csharp
public interface ICalculationService
{
    Task<DCAResult> PerformDCAAnalysisAsync(DCARequest request);
}
```

---

## Usage Examples

### Example 1: Basic DCA Analysis via LifeCycle Service

```csharp
// In a controller or service
var calculationService = serviceProvider.GetRequiredService<ICalculationService>();

var dcaRequest = new DCARequest
{
    WellId = "WELL-001",
    ProductionFluidType = "OIL",
    AdditionalParameters = new Dictionary<string, object>
    {
        { "InitialQi", 1000.0 },
        { "InitialDi", 0.1 },
        { "GenerateForecast", true },
        { "ForecastMonths", 60 },
        { "ConfidenceLevel", 0.95 }
    },
    UserId = "user123"
};

var result = await calculationService.PerformDCAAnalysisAsync(dcaRequest);

Console.WriteLine($"R²: {result.R2:F4}");
Console.WriteLine($"RMSE: {result.RMSE:F2}");
Console.WriteLine($"Initial Rate: {result.InitialProductionRate:F2} bbl/day");
Console.WriteLine($"Decline Rate: {result.DeclineRate:P2}");
```

### Example 2: Multi-Well DCA Analysis

```csharp
var wellIds = new[] { "WELL-001", "WELL-002", "WELL-003" };
var results = new List<DCAResult>();

foreach (var wellId in wellIds)
{
    var request = new DCARequest
    {
        WellId = wellId,
        ProductionFluidType = "OIL",
        UserId = "user123"
    };
    
    var result = await calculationService.PerformDCAAnalysisAsync(request);
    results.Add(result);
}

// Compare results
var bestFit = results.OrderByDescending(r => r.R2).First();
Console.WriteLine($"Best fit well: {bestFit.WellId}, R²: {bestFit.R2:F4}");
```

### Example 3: Direct DCA Library Usage (Advanced)

```csharp
using Beep.OilandGas.DCA;
using Beep.OilandGas.DCA.Results;

// If you need direct access to DCA library
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

var dcaManager = new DCAManager();
var fitResult = dcaManager.AnalyzeWithStatistics(
    productionData, 
    timeData, 
    initialQi: 1000, 
    initialDi: 0.1, 
    confidenceLevel: 0.95);

// Use fit result
var forecastRate = DCAGenerator.HyperbolicDecline(
    fitResult.InitialProductionRate,
    fitResult.DeclineRate,
    t: 365,
    b: fitResult.DeclineExponent);
```

---

## Integration Patterns

### Adding DCA to a New Service

1. **Add Dependency**
   ```csharp
   using Beep.OilandGas.DCA;
   using Beep.OilandGas.DCA.Results;
   ```

2. **Inject Service**
   ```csharp
   public class MyService
   {
       private readonly ICalculationService _calculationService;
       
       public MyService(ICalculationService calculationService)
       {
           _calculationService = calculationService;
       }
   }
   ```

3. **Use DCA**
   ```csharp
   public async Task<DCAResult> AnalyzeWellAsync(string wellId)
   {
       var request = new DCARequest
       {
           WellId = wellId,
           ProductionFluidType = "OIL",
           UserId = GetCurrentUserId()
       };
       
       return await _calculationService.PerformDCAAnalysisAsync(request);
   }
   ```

### Error Handling

```csharp
try
{
    var result = await calculationService.PerformDCAAnalysisAsync(request);
    
    if (result.R2 < 0.8)
    {
        _logger.LogWarning("Low R² value: {RSquared} for well {WellId}", 
            result.R2, request.WellId);
    }
}
catch (InvalidOperationException ex)
{
    // Handle insufficient data
    _logger.LogError(ex, "Insufficient production data for DCA");
}
catch (Exception ex)
{
    // Handle other errors
    _logger.LogError(ex, "Error performing DCA analysis");
    throw;
}
```

---

## Data Storage

### PPDM Tables

#### New Table Required: `DCA_CALCULATION`

**Status:** ⚠️ **New Table Needed** - Does not exist in PPDM39, must be created following PPDM patterns.

**Table Structure:**
```sql
CREATE TABLE DCA_CALCULATION (
    DCA_CALCULATION_ID VARCHAR(50) PRIMARY KEY,
    WELL_ID VARCHAR(50) NULL,
    POOL_ID VARCHAR(50) NULL,
    FIELD_ID VARCHAR(50) NULL,
    CALCULATION_DATE DATETIME,
    DECLINE_METHOD VARCHAR(50),         -- Exponential, Harmonic, Hyperbolic
    INITIAL_PRODUCTION_RATE DECIMAL(18,2),  -- qi
    DECLINE_RATE DECIMAL(10,4),         -- di
    DECLINE_EXPONENT DECIMAL(10,4),     -- b
    R_SQUARED DECIMAL(10,4),            -- R²
    ADJUSTED_R_SQUARED DECIMAL(10,4),
    RMSE DECIMAL(18,2),
    MAE DECIMAL(18,2),
    AIC DECIMAL(18,2),
    BIC DECIMAL(18,2),
    CONVERGED BIT,
    ITERATIONS INT,
    FORECAST_POINTS_JSON NVARCHAR(MAX),  -- JSON serialized forecast points
    -- Standard PPDM columns
    ROW_ID VARCHAR(50),
    ROW_CHANGED_BY VARCHAR(50),
    ROW_CHANGED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50),
    ROW_CREATED_DATE DATETIME
);
```

**Foreign Keys:**
- `DCA_CALCULATION.WELL_ID` → `WELL.WELL_ID`
- `DCA_CALCULATION.POOL_ID` → `POOL.POOL_ID`
- `DCA_CALCULATION.FIELD_ID` → `FIELD.FIELD_ID`

**Notes:**
- At least one of WELL_ID, POOL_ID, or FIELD_ID must be provided
- Store forecast points as JSON for flexibility
- Follow PPDM naming conventions and include standard PPDM audit columns

### Data Model

```csharp
public class DCAResult
{
    public string CalculationId { get; set; }
    public string? WellId { get; set; }
    public string? PoolId { get; set; }
    public string? FieldId { get; set; }
    public double InitialProductionRate { get; set; }
    public double DeclineRate { get; set; }
    public double DeclineExponent { get; set; }
    public double R2 { get; set; }
    public double RMSE { get; set; }
    public List<ForecastPoint>? ForecastPoints { get; set; }
    // ... other properties
}
```

### Relationships

- `DCA_CALCULATION.WELL_ID` → `WELL.WELL_ID`
- `DCA_CALCULATION.POOL_ID` → `POOL.POOL_ID`
- `DCA_CALCULATION.FIELD_ID` → `FIELD.FIELD_ID`

---

## Best Practices

1. **Data Quality**
   - Ensure at least 3 production data points
   - Validate production data before analysis
   - Check for outliers and anomalies

2. **Parameter Selection**
   - Use reasonable initial estimates (qi, di)
   - Consider well type (oil vs gas) for method selection
   - Use confidence intervals for uncertainty quantification

3. **Performance**
   - Use async methods for large datasets
   - Consider parallel processing for multi-well analysis
   - Cache results when appropriate

4. **Validation**
   - Check R² values (should be > 0.8 for good fit)
   - Review RMSE and MAE for model quality
   - Validate forecast points against historical trends

---

## Future Enhancements

### Planned Integrations

1. **Production Service Integration**
   - Automatic DCA when production data is updated
   - Real-time decline rate monitoring
   - Alert on significant decline changes

2. **FieldOrchestrator Integration**
   - Field-level DCA aggregation
   - Portfolio decline analysis
   - Comparative analysis across fields

3. **Enhanced Features**
   - Integration with `ProductionForecasting` for hybrid forecasts
   - Machine learning-based decline prediction
   - Automated decline method selection

### Potential Improvements

- Add support for water production decline
- Integrate with economic analysis for NPV calculations
- Add visualization support for decline curves
- Support for type curve matching

---

## References

- **Project Location:** `Beep.OilandGas.DCA`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService`
- **Documentation:** `Beep.OilandGas.DCA/README.md`
- **PPDM Table:** `DCA_CALCULATION`

---

**Last Updated:** 2024  
**Status:** ✅ Fully Integrated

