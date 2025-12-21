# Beep.OilandGas.SuckerRodPumping - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.SuckerRodPumping** is a comprehensive library for sucker rod pumping analysis and design in oil and gas operations.

### Key Capabilities
- **Sucker Rod Load Analysis**: Peak and minimum loads, stress analysis
- **Production Rate Calculations**: Pump displacement and production rate
- **Power Requirements**: Polished rod, hydraulic, and total horsepower
- **Pump Card Generation**: Load vs position curves
- **System Optimization**: Optimal pump design

### Current Status
⚠️ **Not Yet Integrated** - Should be integrated into `PPDMProductionService` for artificial lift management

---

## Key Classes and Interfaces

### Main Classes

#### `SuckerRodLoadCalculator`
Load analysis and pump card generation.

**Key Methods:**
```csharp
public static class SuckerRodLoadCalculator
{
    public static SuckerRodLoadResult CalculateLoads(
        SuckerRodSystemProperties systemProperties,
        SuckerRodString rodString);
    
    public static PumpCard GeneratePumpCard(
        SuckerRodSystemProperties systemProperties,
        SuckerRodString rodString);
}
```

#### `SuckerRodFlowRatePowerCalculator`
Flow rate and power calculations.

**Key Methods:**
```csharp
public static class SuckerRodFlowRatePowerCalculator
{
    public static SuckerRodFlowRatePowerResult CalculateFlowRateAndPower(
        SuckerRodSystemProperties systemProperties,
        SuckerRodLoadResult loadResult);
    
    public static decimal CalculateProductionRate(
        SuckerRodSystemProperties systemProperties);
    
    public static decimal CalculatePowerRequirements(
        SuckerRodSystemProperties systemProperties,
        SuckerRodLoadResult loadResult);
}
```

---

## Integration with LifeCycle Services

### Planned Integration

**Service:** `PPDMProductionService`  
**Location:** `Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService`

### Integration Points

1. **Sucker Rod Analysis Method**
   - Method: `AnalyzeSuckerRodSystemAsync(SuckerRodAnalysisRequest request)`
   - Retrieves well and equipment data from PPDM database
   - Performs load and power analysis
   - Stores results in `WELL_EQUIPMENT` or related table

---

## Usage Examples

### Example 1: Load Analysis

```csharp
using Beep.OilandGas.SuckerRodPumping.Models;
using Beep.OilandGas.SuckerRodPumping.Calculations;

var systemProperties = new SuckerRodSystemProperties
{
    WellDepth = 5000m,
    TubingDiameter = 2.875m,
    RodDiameter = 0.875m,
    PumpDiameter = 2.0m,
    StrokeLength = 48m,
    StrokesPerMinute = 12m,
    WellheadPressure = 50m,
    BottomHolePressure = 500m,
    OilGravity = 35m,
    WaterCut = 0.3m,
    GasOilRatio = 200m,
    GasSpecificGravity = 0.65m,
    PumpEfficiency = 0.85m
};

var rodString = new SuckerRodString
{
    Sections = new List<RodSection>
    {
        new RodSection
        {
            Diameter = 0.875m,
            Length = 5000m,
            Density = 490m
        }
    }
};

var loadResult = SuckerRodLoadCalculator.CalculateLoads(
    systemProperties, rodString);

Console.WriteLine($"Peak Load: {loadResult.PeakLoad:F2} lb");
Console.WriteLine($"Maximum Stress: {loadResult.MaximumStress:F2} psi");
```

### Example 2: Integration with LifeCycle Service (Planned)

```csharp
var productionService = serviceProvider.GetRequiredService<IFieldProductionService>();

var request = new SuckerRodAnalysisRequest
{
    WellId = "WELL-001",
    UserId = "user123"
};

var result = await productionService.AnalyzeSuckerRodSystemAsync(request);
```

---

## Data Storage

### PPDM Tables

#### Existing Table: `WELL_EQUIPMENT`

**Status:** ✅ **Existing PPDM Table** - Already exists in PPDM39, can be used for sucker rod system data.

**Usage:**
- Store sucker rod system properties (rod diameter, pump diameter, stroke length, etc.)
- Store analysis results (loads, power requirements, pump card data)
- Link to `WELL` via `WELL_ID`

**Note:** If additional sucker rod-specific fields are needed beyond what `WELL_EQUIPMENT` provides, consider:
- Using `WELL_EQUIPMENT` with extended attributes/JSON fields
- Or creating `SUCKER_ROD_SYSTEM` table if following PPDM extension patterns

### Relationships

- `WELL_EQUIPMENT.WELL_ID` → `WELL.WELL_ID`

---

## References

- **Project Location:** `Beep.OilandGas.SuckerRodPumping`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService` (planned)
- **Documentation:** `Beep.OilandGas.SuckerRodPumping/README.md`
- **PPDM Table:** `WELL_EQUIPMENT`

---

**Last Updated:** 2024  
**Status:** ⚠️ Not Yet Integrated (Should be integrated)

