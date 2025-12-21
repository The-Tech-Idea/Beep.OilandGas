# Beep.OilandGas.ChokeAnalysis - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.ChokeAnalysis** is a comprehensive library for gas choke flow calculations in oil and gas engineering applications.

### Key Capabilities
- **Downhole Choke Flow**: Gas flow through downhole chokes
- **Uphole Choke Flow**: Gas flow through uphole chokes
- **Sonic Flow**: Critical (sonic) flow calculations
- **Subsonic Flow**: Subcritical flow calculations
- **Choke Sizing**: Calculate required choke size for desired flow rate
- **Pressure Calculations**: Calculate downstream pressure for given flow rate

### Current Status
⚠️ **Not Yet Integrated** - Should be integrated into `PPDMProductionService` for production optimization

---

## Key Classes and Interfaces

### Main Classes

#### `GasChokeCalculator`
Gas choke flow calculations.

**Key Methods:**
```csharp
public static class GasChokeCalculator
{
    public static ChokeFlowResult CalculateDownholeChokeFlow(
        ChokeProperties choke,
        GasChokeProperties gasProperties);
    
    public static ChokeFlowResult CalculateUpholeChokeFlow(
        ChokeProperties choke,
        GasChokeProperties gasProperties);
    
    public static decimal CalculateRequiredChokeSize(
        GasChokeProperties gasProperties,
        decimal flowRate);
    
    public static decimal CalculateDownstreamPressure(
        ChokeProperties choke,
        GasChokeProperties gasProperties,
        decimal flowRate);
}
```

---

## Integration with LifeCycle Services

### Planned Integration

**Service:** `PPDMProductionService`  
**Location:** `Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService`

### Integration Points

1. **Choke Analysis Method**
   - Method: `AnalyzeChokeFlowAsync(ChokeAnalysisRequest request)`
   - Retrieves well/choke data from PPDM database
   - Performs choke flow analysis
   - Stores results in well equipment table

---

## Usage Examples

### Example 1: Downhole Choke Flow

```csharp
using Beep.OilandGas.ChokeAnalysis.Models;
using Beep.OilandGas.ChokeAnalysis.Calculations;

var choke = new ChokeProperties
{
    ChokeDiameter = 0.5m,
    ChokeType = ChokeType.Bean,
    DischargeCoefficient = 0.85m
};

var gasProperties = new GasChokeProperties
{
    UpstreamPressure = 2000m,
    DownstreamPressure = 500m,
    Temperature = 580m,
    GasSpecificGravity = 0.65m
};

var result = GasChokeCalculator.CalculateDownholeChokeFlow(choke, gasProperties);

Console.WriteLine($"Flow Rate: {result.FlowRate:F2} Mscf/day");
Console.WriteLine($"Flow Regime: {result.FlowRegime}");
```

### Example 2: Integration with LifeCycle Service (Planned)

```csharp
var productionService = serviceProvider.GetRequiredService<IFieldProductionService>();

var request = new ChokeAnalysisRequest
{
    WellId = "WELL-001",
    ChokeType = "DOWNHOLE",
    UserId = "user123"
};

var result = await productionService.AnalyzeChokeFlowAsync(request);
```

---

## Data Storage

### PPDM Tables

#### Existing Table: `WELL_EQUIPMENT`

**Status:** ✅ **Existing PPDM Table** - Already exists in PPDM39, can be used for choke data.

**Usage:**
- Store choke properties (diameter, type, discharge coefficient)
- Store choke analysis results (flow rate, pressure drop, flow regime)
- Link to `WELL` via `WELL_ID`

**Note:** If additional choke-specific fields are needed beyond what `WELL_EQUIPMENT` provides, consider:
- Using `WELL_EQUIPMENT` with extended attributes/JSON fields
- Or creating `CHOKE_ANALYSIS` table if following PPDM extension patterns

### Relationships

- `WELL_EQUIPMENT.WELL_ID` → `WELL.WELL_ID`

---

## References

- **Project Location:** `Beep.OilandGas.ChokeAnalysis`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Production.PPDMProductionService` (planned)
- **Documentation:** `Beep.OilandGas.ChokeAnalysis/README.md`
- **PPDM Table:** `WELL_EQUIPMENT`

---

**Last Updated:** 2024  
**Status:** ⚠️ Not Yet Integrated (Should be integrated)

