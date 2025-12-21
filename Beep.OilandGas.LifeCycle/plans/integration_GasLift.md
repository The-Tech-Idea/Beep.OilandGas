# Beep.OilandGas.GasLift - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.GasLift** is a comprehensive library for gas lift analysis and design in oil and gas operations. It provides calculations for gas lift potential analysis, valve design, valve spacing, and system optimization.

### Key Capabilities
- **Gas Lift Potential Analysis**: Analyze production potential with gas lift
- **Gas Lift Valve Design**: Design valves for US field and SI units
- **Valve Spacing Calculations**: Equal pressure drop and equal depth spacing
- **Performance Curve Generation**: Generate performance curves
- **System Optimization**: Find optimal gas injection rate

### Current Status
⚠️ **Not Yet Integrated** - Should be integrated into `PPDMDevelopmentService` or a dedicated artificial lift service

---

## Key Classes and Interfaces

### Main Classes

#### `GasLiftPotentialCalculator`
Gas lift potential analysis.

**Key Methods:**
```csharp
public static class GasLiftPotentialCalculator
{
    public static GasLiftPotentialResult AnalyzeGasLiftPotential(
        GasLiftWellProperties wellProperties,
        decimal minGasInjectionRate,
        decimal maxGasInjectionRate,
        int numberOfPoints);
}
```

#### `GasLiftValveDesignCalculator`
Gas lift valve design.

**Key Methods:**
```csharp
public static class GasLiftValveDesignCalculator
{
    public static GasLiftValveDesignResult DesignValvesUS(
        GasLiftWellProperties wellProperties,
        decimal gasInjectionPressure,
        int numberOfValves);
    
    public static GasLiftValveDesignResult DesignValvesSI(
        GasLiftWellProperties wellProperties,
        decimal gasInjectionPressure,
        int numberOfValves);
}
```

#### `GasLiftValveSpacingCalculator`
Valve spacing calculations.

**Key Methods:**
```csharp
public static class GasLiftValveSpacingCalculator
{
    public static GasLiftValveSpacingResult CalculateEqualPressureDropSpacing(
        GasLiftWellProperties wellProperties,
        decimal gasInjectionPressure,
        int numberOfValves);
    
    public static GasLiftValveSpacingResult CalculateEqualDepthSpacing(
        GasLiftWellProperties wellProperties,
        decimal gasInjectionPressure,
        int numberOfValves);
}
```

#### `GasLiftWellProperties`
Well properties for gas lift.

**Key Properties:**
```csharp
public class GasLiftWellProperties
{
    public decimal WellDepth { get; set; }              // feet
    public decimal TubingDiameter { get; set; }         // inches
    public decimal CasingDiameter { get; set; }         // inches
    public decimal WellheadPressure { get; set; }       // psia
    public decimal BottomHolePressure { get; set; }     // psia
    public decimal WellheadTemperature { get; set; }    // Rankine
    public decimal BottomHoleTemperature { get; set; }   // Rankine
    public decimal OilGravity { get; set; }              // API
    public decimal WaterCut { get; set; }                // fraction
    public decimal GasOilRatio { get; set; }             // scf/bbl
    public decimal GasSpecificGravity { get; set; }
    public decimal DesiredProductionRate { get; set; }   // bbl/day
}
```

---

## Integration with LifeCycle Services

### Planned Integration

**Service:** `PPDMDevelopmentService` or new `ArtificialLiftService`  
**Location:** `Beep.OilandGas.LifeCycle.Services.Development.PPDMDevelopmentService`

### Integration Points

1. **Gas Lift Analysis Method**
   - Method: `AnalyzeGasLiftPotentialAsync(GasLiftAnalysisRequest request)`
   - Retrieves well data from PPDM database
   - Performs gas lift analysis
   - Stores results in gas lift-related table

2. **Data Flow**
   ```
   Well Data (PPDM) → GasLiftCalculator → GasLiftResult → GasLiftAnalysisResult DTO → PPDM Database
   ```

### Service Methods (To Be Added)

```csharp
public interface IFieldDevelopmentService
{
    Task<GasLiftAnalysisResult> AnalyzeGasLiftPotentialAsync(GasLiftAnalysisRequest request);
    Task<GasLiftValveDesignResult> DesignGasLiftValvesAsync(GasLiftValveDesignRequest request);
}
```

---

## Usage Examples

### Example 1: Gas Lift Potential Analysis

```csharp
using Beep.OilandGas.GasLift.Models;
using Beep.OilandGas.GasLift.Calculations;

var wellProperties = new GasLiftWellProperties
{
    WellDepth = 10000m,
    TubingDiameter = 2.875m,
    CasingDiameter = 7.0m,
    WellheadPressure = 100m,
    BottomHolePressure = 2000m,
    WellheadTemperature = 520m,
    BottomHoleTemperature = 580m,
    OilGravity = 35m,
    WaterCut = 0.3m,
    GasOilRatio = 500m,
    GasSpecificGravity = 0.65m,
    DesiredProductionRate = 2000m
};

var potentialResult = GasLiftPotentialCalculator.AnalyzeGasLiftPotential(
    wellProperties,
    minGasInjectionRate: 100m,
    maxGasInjectionRate: 5000m,
    numberOfPoints: 50);

Console.WriteLine($"Optimal Gas Injection Rate: {potentialResult.OptimalGasInjectionRate:F2} Mscf/day");
Console.WriteLine($"Maximum Production Rate: {potentialResult.MaximumProductionRate:F2} bbl/day");
```

### Example 2: Valve Design

```csharp
var designResult = GasLiftValveDesignCalculator.DesignValvesUS(
    wellProperties,
    gasInjectionPressure: 1500m,
    numberOfValves: 5);

foreach (var valve in designResult.Valves)
{
    Console.WriteLine($"Valve at {valve.Depth:F0} ft: Port Size = {valve.PortSize:F3} in");
}
```

### Example 3: Integration with LifeCycle Service (Planned)

```csharp
var developmentService = serviceProvider.GetRequiredService<IFieldDevelopmentService>();

var request = new GasLiftAnalysisRequest
{
    WellId = "WELL-001",
    MinGasInjectionRate = 100m,
    MaxGasInjectionRate = 5000m,
    UserId = "user123"
};

var result = await developmentService.AnalyzeGasLiftPotentialAsync(request);
```

---

## Integration Patterns

### Adding Gas Lift to LifeCycle Services

1. **Add Dependency**
   ```csharp
   using Beep.OilandGas.GasLift;
   using Beep.OilandGas.GasLift.Calculations;
   ```

2. **Add to Development Service**
   ```csharp
   public async Task<GasLiftAnalysisResult> AnalyzeGasLiftPotentialAsync(
       GasLiftAnalysisRequest request)
   {
       // Retrieve well data from PPDM
       // Build GasLiftWellProperties
       // Perform analysis
       // Store results
       // Return result
   }
   ```

---

## Data Storage

### PPDM Tables

#### Existing Table: `WELL_EQUIPMENT`

**Status:** ✅ **Existing PPDM Table** - Already exists in PPDM39, can be used for gas lift equipment.

**Usage:**
- Store gas lift valve design data
- Store gas lift system configuration
- Link to `WELL` via `WELL_ID`

**Note:** If additional gas lift-specific fields are needed beyond what `WELL_EQUIPMENT` provides, consider:
- Using `WELL_EQUIPMENT` with extended attributes/JSON fields
- Or creating `GAS_LIFT_VALVE` table if following PPDM extension patterns

### Relationships

- `WELL_EQUIPMENT.WELL_ID` → `WELL.WELL_ID`

---

## Best Practices

1. **Well Properties**
   - Use accurate well depth and pressure data
   - Consider temperature effects
   - Account for water cut and GOR

2. **Valve Design**
   - Use appropriate number of valves
   - Consider injection pressure constraints
   - Optimize valve spacing

---

## Future Enhancements

### Planned Integrations

1. **Development Service Integration**
   - Automatic gas lift design for new wells
   - Integration with well completion
   - System optimization

2. **Production Service Integration**
   - Gas lift performance monitoring
   - Optimization recommendations
   - Integration with production forecasting

---

## References

- **Project Location:** `Beep.OilandGas.GasLift`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Development.PPDMDevelopmentService` (planned)
- **Documentation:** `Beep.OilandGas.GasLift/README.md`
- **PPDM Table:** `WELL_EQUIPMENT` or `GAS_LIFT_SYSTEM` (to be created)

---

**Last Updated:** 2024  
**Status:** ⚠️ Not Yet Integrated (Should be integrated)

