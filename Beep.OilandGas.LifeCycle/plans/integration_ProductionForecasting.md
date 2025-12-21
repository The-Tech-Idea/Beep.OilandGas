# Beep.OilandGas.ProductionForecasting - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.ProductionForecasting** is a comprehensive library for production forecasting in oil and gas engineering applications. It provides industry-standard methods for forecasting production rates and cumulative production based on reservoir properties and operating conditions.

### Key Capabilities
- **Pseudo-Steady State (Single-Phase)**: For oil wells above bubble point
- **Pseudo-Steady State (Two-Phase)**: For oil wells below bubble point
- **Transient Flow**: Early-time production forecasting
- **Gas Well Forecasting**: Specialized gas well production forecasting
- Production rate and cumulative production forecasting
- Reservoir pressure decline calculations

### Current Status
✅ **Fully Integrated** - Used in `PPDMCalculationService` for centralized physics-based production forecasting

### Architecture Decision
**Service:** `PPDMCalculationService` (Centralized Calculation Service)  
**Rationale:** Pure calculation/analysis project - performs mathematical production forecasting without operational data management. Centralized service provides consistent data mapping and result storage.

---

## Key Classes and Interfaces

### Main Classes

#### `PseudoSteadyStateForecast`
Pseudo-steady state production forecasting.

**Key Methods:**
```csharp
public static class PseudoSteadyStateForecast
{
    public static ForecastResult GenerateSinglePhaseForecast(
        ReservoirForecastProperties reservoir,
        decimal bottomHolePressure,
        decimal forecastDuration,
        int timeSteps);
    
    public static ForecastResult GenerateTwoPhaseForecast(
        ReservoirForecastProperties reservoir,
        decimal bottomHolePressure,
        decimal bubblePointPressure,
        decimal forecastDuration,
        int timeSteps);
}
```

#### `TransientForecast`
Transient flow production forecasting.

**Key Methods:**
```csharp
public static class TransientForecast
{
    public static ForecastResult GenerateTransientForecast(
        ReservoirForecastProperties reservoir,
        decimal bottomHolePressure,
        decimal forecastDuration,
        int timeSteps);
}
```

#### `GasWellForecast`
Gas well production forecasting.

**Key Methods:**
```csharp
public static class GasWellForecast
{
    public static ForecastResult GenerateGasWellForecast(
        ReservoirForecastProperties reservoir,
        decimal bottomHolePressure,
        decimal forecastDuration,
        int timeSteps);
}
```

#### `ReservoirForecastProperties`
Reservoir properties for forecasting.

**Key Properties:**
```csharp
public class ReservoirForecastProperties
{
    public decimal InitialPressure { get; set; }        // psia
    public decimal Permeability { get; set; }            // md
    public decimal Thickness { get; set; }               // feet
    public decimal DrainageRadius { get; set; }          // feet
    public decimal WellboreRadius { get; set; }          // feet
    public decimal FormationVolumeFactor { get; set; }   // RB/STB
    public decimal OilViscosity { get; set; }            // cp
    public decimal TotalCompressibility { get; set; }    // 1/psi
    public decimal Porosity { get; set; }
    public decimal SkinFactor { get; set; }
    public decimal Temperature { get; set; }             // Rankine
    public decimal? GasSpecificGravity { get; set; }     // For gas wells
}
```

#### `ForecastResult`
Forecast calculation results.

**Key Properties:**
```csharp
public class ForecastResult
{
    public decimal InitialProductionRate { get; set; }      // bbl/day or Mscf/day
    public decimal FinalProductionRate { get; set; }
    public decimal TotalCumulativeProduction { get; set; }
    public List<ForecastPoint> ForecastPoints { get; set; }
    public List<PressurePoint> PressurePoints { get; set; }
}
```

### Validation

#### `ForecastValidator`
Input validation for forecast parameters.

**Key Methods:**
```csharp
public static class ForecastValidator
{
    public static void ValidateForecastParameters(
        ReservoirForecastProperties reservoir,
        decimal bottomHolePressure,
        decimal forecastDuration,
        decimal timeSteps);
}
```

---

## Integration with LifeCycle Services

### Current Integration

**Service:** `PPDMCalculationService`  
**Location:** `Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService`

### Integration Points

1. **Physics-Based Forecast Method**
   - Method: `PerformPhysicsBasedForecastAsync(DCARequest request)`
   - Uses `PseudoSteadyStateForecast`, `TransientForecast`, or `GasWellForecast`
   - Retrieves reservoir properties from PPDM database
   - Stores results in `DCA_CALCULATION` table (shared with DCA)

2. **Data Flow**
   ```
   Reservoir Properties (PPDM) → Forecast Calculator → ForecastResult → DCAResult DTO → PPDM Database
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
    // Physics-based forecast is triggered via DCARequest with ForecastType parameter
}
```

### Triggering Physics-Based Forecast

Physics-based forecasting is triggered when `DCARequest.AdditionalParameters` contains:
- `ForecastType = "PHYSICS_BASED"` or `"PSEUDO_STEADY_STATE"` or `"TRANSIENT"` or `"GAS_WELL"`

---

## Usage Examples

### Example 1: Single-Phase Pseudo-Steady State Forecast

```csharp
var calculationService = serviceProvider.GetRequiredService<ICalculationService>();

var request = new DCARequest
{
    WellId = "WELL-001",
    ProductionFluidType = "OIL",
    AdditionalParameters = new Dictionary<string, object>
    {
        { "ForecastType", "PSEUDO_STEADY_STATE" },
        { "BottomHolePressure", 1500m },      // psia
        { "ForecastDuration", 365m },         // days
        { "TimeSteps", 100 },
        // Reservoir properties (if not in database)
        { "InitialPressure", 3000m },
        { "Permeability", 100m },
        { "Thickness", 50m },
        { "DrainageRadius", 1000m },
        { "WellboreRadius", 0.25m },
        { "FormationVolumeFactor", 1.2m },
        { "OilViscosity", 1.5m },
        { "TotalCompressibility", 0.00001m },
        { "Porosity", 0.2m },
        { "SkinFactor", 0m },
        { "Temperature", 580m }
    },
    UserId = "user123"
};

var result = await calculationService.PerformDCAAnalysisAsync(request);

Console.WriteLine($"Initial Rate: {result.InitialProductionRate:F2} bbl/day");
Console.WriteLine($"Final Rate: {result.FinalProductionRate:F2} bbl/day");
Console.WriteLine($"Total Cumulative: {result.TotalCumulativeProduction:F2} bbl");
```

### Example 2: Two-Phase Forecast (Below Bubble Point)

```csharp
var request = new DCARequest
{
    WellId = "WELL-002",
    ProductionFluidType = "OIL",
    AdditionalParameters = new Dictionary<string, object>
    {
        { "ForecastType", "PSEUDO_STEADY_STATE" },
        { "BottomHolePressure", 1500m },
        { "BubblePointPressure", 2000m },     // Required for two-phase
        { "ForecastDuration", 365m },
        { "TimeSteps", 100 },
        // ... reservoir properties
    },
    UserId = "user123"
};

var result = await calculationService.PerformDCAAnalysisAsync(request);
```

### Example 3: Gas Well Forecast

```csharp
var request = new DCARequest
{
    WellId = "WELL-003",
    ProductionFluidType = "GAS",
    AdditionalParameters = new Dictionary<string, object>
    {
        { "ForecastType", "GAS_WELL" },
        { "BottomHolePressure", 1000m },
        { "ForecastDuration", 365m },
        { "TimeSteps", 100 },
        { "GasSpecificGravity", 0.65m },
        // ... other reservoir properties
    },
    UserId = "user123"
};

var result = await calculationService.PerformDCAAnalysisAsync(request);
// Gas rates are in Mscf/day
Console.WriteLine($"Initial Rate: {result.InitialProductionRate:F2} Mscf/day");
```

### Example 4: Transient Flow Forecast

```csharp
var request = new DCARequest
{
    WellId = "WELL-004",
    ProductionFluidType = "OIL",
    AdditionalParameters = new Dictionary<string, object>
    {
        { "ForecastType", "TRANSIENT" },
        { "BottomHolePressure", 1500m },
        { "ForecastDuration", 365m },
        { "TimeSteps", 100 },
        // ... reservoir properties
    },
    UserId = "user123"
};

var result = await calculationService.PerformDCAAnalysisAsync(request);
```

### Example 5: Direct Library Usage (Advanced)

```csharp
using Beep.OilandGas.ProductionForecasting.Calculations;
using Beep.OilandGas.ProductionForecasting.Models;

var reservoir = new ReservoirForecastProperties
{
    InitialPressure = 3000m,
    Permeability = 100m,
    Thickness = 50m,
    DrainageRadius = 1000m,
    WellboreRadius = 0.25m,
    FormationVolumeFactor = 1.2m,
    OilViscosity = 1.5m,
    TotalCompressibility = 0.00001m,
    Porosity = 0.2m,
    SkinFactor = 0m,
    Temperature = 580m
};

// Validate inputs
ForecastValidator.ValidateForecastParameters(
    reservoir, 
    bottomHolePressure: 1500m, 
    forecastDuration: 365m, 
    timeSteps: 100);

// Generate forecast
var forecast = PseudoSteadyStateForecast.GenerateSinglePhaseForecast(
    reservoir,
    bottomHolePressure: 1500m,
    forecastDuration: 365m,
    timeSteps: 100);

// Access results
Console.WriteLine($"Initial Rate: {forecast.InitialProductionRate:F2} bbl/day");
Console.WriteLine($"Total Cumulative: {forecast.TotalCumulativeProduction:F2} bbl");
```

---

## Integration Patterns

### Adding Production Forecasting to a New Service

1. **Add Dependency**
   ```csharp
   using Beep.OilandGas.ProductionForecasting.Calculations;
   using Beep.OilandGas.ProductionForecasting.Models;
   ```

2. **Use Calculation Service**
   ```csharp
   public class MyService
   {
       private readonly ICalculationService _calculationService;
       
       public async Task<DCAResult> ForecastProductionAsync(string wellId)
       {
           var request = new DCARequest
           {
               WellId = wellId,
               ProductionFluidType = "OIL",
               AdditionalParameters = new Dictionary<string, object>
               {
                   { "ForecastType", "PSEUDO_STEADY_STATE" },
                   { "BottomHolePressure", 1500m },
                   { "ForecastDuration", 365m },
                   { "TimeSteps", 100 }
               },
               UserId = GetCurrentUserId()
           };
           
           return await _calculationService.PerformDCAAnalysisAsync(request);
       }
   }
   ```

### Error Handling

```csharp
try
{
    // Validate reservoir properties first
    ForecastValidator.ValidateForecastParameters(
        reservoir, bottomHolePressure, forecastDuration, timeSteps);
    
    var result = await calculationService.PerformDCAAnalysisAsync(request);
}
catch (InvalidReservoirPropertiesException ex)
{
    _logger.LogError(ex, "Invalid reservoir properties for forecast");
    throw;
}
catch (ForecastParameterOutOfRangeException ex)
{
    _logger.LogError(ex, "Parameter {Parameter} out of range: {Message}", 
        ex.ParameterName, ex.Message);
    throw;
}
catch (ForecastConvergenceException ex)
{
    _logger.LogError(ex, "Forecast calculation failed to converge");
    throw;
}
```

---

## Data Storage

### PPDM Tables

#### Table: `DCA_CALCULATION` (Shared with DCA)

**Status:** ⚠️ **New Table Needed** - Does not exist in PPDM39, must be created following PPDM patterns. Shared with DCA for both decline curve analysis and physics-based forecasting results.

**Usage:**
- Store physics-based forecast results
- Store DCA analysis results
- Links to `WELL`, `POOL`, or `FIELD` via foreign keys
- Contains forecast parameters and results

**Note:** The `DCA_CALCULATION` table structure should include fields for both DCA and physics-based forecasting. See `integration_DCA.md` for complete table structure. The `FORECAST_TYPE` field distinguishes between "DCA" and physics-based forecast types ("PSEUDO_STEADY_STATE", "TRANSIENT", "GAS_WELL").

### Data Model

```csharp
public class DCAResult
{
    public string CalculationId { get; set; }
    public string? WellId { get; set; }
    public string? PoolId { get; set; }
    public string? FieldId { get; set; }
    public string ForecastType { get; set; }  // "PHYSICS_BASED", "PSEUDO_STEADY_STATE", etc.
    public decimal InitialProductionRate { get; set; }
    public decimal FinalProductionRate { get; set; }
    public decimal TotalCumulativeProduction { get; set; }
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

1. **Reservoir Properties**
   - Ensure all required properties are available
   - Validate property ranges before forecasting
   - Use appropriate forecast type for well conditions

2. **Forecast Type Selection**
   - **Single-Phase**: Oil wells above bubble point
   - **Two-Phase**: Oil wells below bubble point
   - **Transient**: Early-time or new wells
   - **Gas Well**: Gas wells

3. **Time Steps**
   - Use sufficient time steps (100+ recommended) for accuracy
   - Balance accuracy vs. performance
   - Consider forecast duration when selecting steps

4. **Validation**
   - Always validate inputs before forecasting
   - Check forecast results for reasonableness
   - Compare with historical data when available

---

## Future Enhancements

### Planned Integrations

1. **Production Service Integration**
   - Automatic forecasting when reservoir properties change
   - Real-time forecast updates
   - Comparison with actual production

2. **FieldOrchestrator Integration**
   - Field-level production forecasting
   - Portfolio forecasting
   - Scenario analysis

3. **Enhanced Features**
   - Integration with DCA for hybrid forecasts
   - Uncertainty quantification
   - Sensitivity analysis
   - Integration with economic analysis

### Potential Improvements

- Add support for multi-phase flow forecasting
- Integrate with nodal analysis for system optimization
- Add visualization support for forecast curves
- Support for decline curve transition from transient to pseudo-steady state

---

## References

- **Project Location:** `Beep.OilandGas.ProductionForecasting`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService`
- **Documentation:** `Beep.OilandGas.ProductionForecasting/README.md`
- **PPDM Table:** `DCA_CALCULATION` (shared with DCA)

---

**Last Updated:** 2024  
**Status:** ✅ Fully Integrated

