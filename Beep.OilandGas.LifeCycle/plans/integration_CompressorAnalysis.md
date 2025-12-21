# Beep.OilandGas.CompressorAnalysis - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.CompressorAnalysis** is a comprehensive library for compressor analysis and design in oil and gas operations, supporting both centrifugal and reciprocating compressors.

### Key Capabilities
- **Centrifugal Compressor Analysis**: Polytropic head, adiabatic head, power requirements
- **Reciprocating Compressor Analysis**: Cylinder displacement, volumetric efficiency, power requirements
- **Compressor Pressure Calculations**: Required discharge pressure, maximum flow rate
- **System Efficiency Calculations**: Overall system efficiency
- **Multi-stage Support**: Multi-stage compressor analysis

### Current Status
⚠️ **Not Yet Integrated** - Should be integrated into `PPDMDevelopmentService` for facility management

---

## Key Classes and Interfaces

### Main Classes

#### `CentrifugalCompressorCalculator`
Centrifugal compressor calculations.

**Key Methods:**
```csharp
public static class CentrifugalCompressorCalculator
{
    public static CompressorPowerResult CalculatePower(
        CentrifugalCompressorProperties compressorProperties,
        bool useSIUnits = false);
}
```

#### `ReciprocatingCompressorCalculator`
Reciprocating compressor calculations.

**Key Methods:**
```csharp
public static class ReciprocatingCompressorCalculator
{
    public static CompressorPowerResult CalculatePower(
        ReciprocatingCompressorProperties compressorProperties,
        bool useSIUnits = false);
}
```

#### `CompressorPressureCalculator`
Compressor pressure calculations.

**Key Methods:**
```csharp
public static class CompressorPressureCalculator
{
    public static CompressorPressureResult CalculateRequiredPressure(
        CompressorOperatingConditions operatingConditions,
        decimal requiredFlowRate,
        decimal maxPower,
        decimal compressorEfficiency);
    
    public static decimal CalculateMaximumFlowRate(
        CompressorOperatingConditions operatingConditions,
        decimal compressionRatio,
        decimal maxPower,
        decimal compressorEfficiency);
}
```

---

## Integration with LifeCycle Services

### Planned Integration

**Service:** `PPDMDevelopmentService`  
**Location:** `Beep.OilandGas.LifeCycle.Services.Development.PPDMDevelopmentService`

### Integration Points

1. **Compressor Analysis Method**
   - Method: `AnalyzeCompressorAsync(CompressorAnalysisRequest request)`
   - Retrieves compressor/facility data from PPDM database
   - Performs power or pressure analysis
   - Stores results in `FACILITY_EQUIPMENT` or related table

---

## Usage Examples

### Example 1: Centrifugal Compressor Power

```csharp
using Beep.OilandGas.CompressorAnalysis.Models;
using Beep.OilandGas.CompressorAnalysis.Calculations;

var operatingConditions = new CompressorOperatingConditions
{
    SuctionPressure = 100m,
    DischargePressure = 500m,
    SuctionTemperature = 520m,
    DischargeTemperature = 600m,
    GasFlowRate = 1000m,
    GasSpecificGravity = 0.65m,
    GasMolecularWeight = 18.8m,
    CompressorEfficiency = 0.75m,
    MechanicalEfficiency = 0.95m
};

var compressorProperties = new CentrifugalCompressorProperties
{
    OperatingConditions = operatingConditions,
    PolytropicEfficiency = 0.75m,
    SpecificHeatRatio = 1.3m,
    NumberOfStages = 1,
    Speed = 3600m
};

var result = CentrifugalCompressorCalculator.CalculatePower(
    compressorProperties,
    useSIUnits: false);

Console.WriteLine($"Brake Horsepower: {result.BrakeHorsepower:F2} HP");
Console.WriteLine($"Compression Ratio: {result.CompressionRatio:F2}");
```

### Example 2: Integration with LifeCycle Service (Planned)

```csharp
var developmentService = serviceProvider.GetRequiredService<IFieldDevelopmentService>();

var request = new CompressorAnalysisRequest
{
    FacilityId = "FACILITY-001",
    CompressorType = "CENTRIFUGAL",
    UserId = "user123"
};

var result = await developmentService.AnalyzeCompressorAsync(request);
```

---

## Data Storage

### PPDM Tables

#### Existing Table: `FACILITY_EQUIPMENT`

**Status:** ✅ **Existing PPDM Table** - Already exists in PPDM39, can be used for compressor data.

**Usage:**
- Store compressor properties and specifications
- Store compressor analysis results (power, pressure, etc.)
- Link to `FACILITY` via `FACILITY_ID`

**Note:** If additional compressor-specific fields are needed beyond what `FACILITY_EQUIPMENT` provides, consider:
- Using `FACILITY_EQUIPMENT` with extended attributes/JSON fields
- Or creating `COMPRESSOR_ANALYSIS` table if following PPDM extension patterns

### Relationships

- `FACILITY_EQUIPMENT.FACILITY_ID` → `FACILITY.FACILITY_ID`

---

## References

- **Project Location:** `Beep.OilandGas.CompressorAnalysis`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Development.PPDMDevelopmentService` (planned)
- **Documentation:** `Beep.OilandGas.CompressorAnalysis/README.md`
- **PPDM Table:** `FACILITY_EQUIPMENT`

---

**Last Updated:** 2024  
**Status:** ⚠️ Not Yet Integrated (Should be integrated)

