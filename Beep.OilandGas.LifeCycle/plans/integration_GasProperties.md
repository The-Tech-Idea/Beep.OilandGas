# Beep.OilandGas.GasProperties - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.GasProperties** is a comprehensive library for calculating gas properties in oil and gas engineering applications.

### Key Capabilities
- **Z-Factor Calculations**: Multiple correlation methods (Brill-Beggs, Hall-Yarborough, Standing-Katz)
- **Gas Viscosity**: Carr-Kobayashi-Burrows and Lee-Gonzalez-Eakin correlations
- **Pseudo-Pressure**: Numerical integration methods
- **Average Properties**: Pressure-weighted and arithmetic averaging
- **Gas Composition**: Pseudo-critical properties from composition

### Current Status
✅ **Used by Other Projects** - Used by DCA, ProductionForecasting, GasLift, PipelineAnalysis, etc.

---

## Key Classes and Interfaces

### Main Classes

#### `ZFactorCalculator`
Z-factor (compressibility factor) calculations.

**Key Methods:**
```csharp
public static class ZFactorCalculator
{
    public static decimal CalculateBrillBeggs(
        decimal pressure, 
        decimal temperature, 
        decimal specificGravity);
    
    public static decimal CalculateHallYarborough(
        decimal pressure, 
        decimal temperature, 
        decimal specificGravity);
    
    public static (decimal, decimal) CalculatePseudoCriticalProperties(
        GasComposition composition);
}
```

#### `GasViscosityCalculator`
Gas viscosity calculations.

**Key Methods:**
```csharp
public static class GasViscosityCalculator
{
    public static decimal CalculateCarrKobayashiBurrows(
        decimal pressure, 
        decimal temperature, 
        decimal specificGravity, 
        decimal zFactor);
    
    public static decimal CalculateLeeGonzalezEakin(
        decimal pressure, 
        decimal temperature, 
        decimal specificGravity, 
        decimal zFactor);
}
```

#### `PseudoPressureCalculator`
Pseudo-pressure calculations.

**Key Methods:**
```csharp
public static class PseudoPressureCalculator
{
    public static decimal CalculatePseudoPressure(
        decimal pressure,
        decimal temperature,
        decimal specificGravity,
        Func<decimal, decimal, decimal, decimal> zFactorFunction,
        Func<decimal, decimal, decimal, decimal, decimal> viscosityFunction);
}
```

---

## Integration with LifeCycle Services

### Current Integration

**Indirect Integration** - Used by other calculation libraries that are integrated into LifeCycle services:
- `Beep.OilandGas.DCA` - Used in gas well DCA
- `Beep.OilandGas.ProductionForecasting` - Used in gas well forecasting
- `Beep.OilandGas.GasLift` - Used in gas lift calculations
- `Beep.OilandGas.PipelineAnalysis` - Used in gas pipeline analysis
- `Beep.OilandGas.CompressorAnalysis` - Used in compressor analysis
- `Beep.OilandGas.ChokeAnalysis` - Used in choke flow calculations

### Integration Points

1. **Direct Usage in Services**
   - Can be used directly in LifeCycle services for gas property calculations
   - Provides foundation for all gas-related calculations

2. **Data Flow**
   ```
   Gas Properties (Pressure, Temperature, SG) → GasProperties Calculator → Z-Factor, Viscosity, etc. → Used by Other Calculations
   ```

---

## Usage Examples

### Example 1: Z-Factor Calculation

```csharp
using Beep.OilandGas.GasProperties.Calculations;

decimal pressure = 2000m;      // psia
decimal temperature = 580m;     // Rankine
decimal specificGravity = 0.65m;

decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
    pressure, temperature, specificGravity);

Console.WriteLine($"Z-factor: {zFactor:F4}");
```

### Example 2: Gas Viscosity

```csharp
decimal viscosity = GasViscosityCalculator.CalculateCarrKobayashiBurrows(
    pressure, temperature, specificGravity, zFactor);

Console.WriteLine($"Gas viscosity: {viscosity:F4} cp");
```

### Example 3: Direct Usage in LifeCycle Service

```csharp
public class PPDMProductionService
{
    public decimal CalculateGasZFactor(string wellId, decimal pressure, decimal temperature)
    {
        // Get gas specific gravity from well data
        var well = GetWell(wellId);
        var specificGravity = well.GasSpecificGravity ?? 0.65m;
        
        // Calculate Z-factor
        return ZFactorCalculator.CalculateBrillBeggs(
            pressure, temperature, specificGravity);
    }
}
```

---

## Integration Patterns

### Using Gas Properties in Services

1. **Add Dependency**
   ```csharp
   using Beep.OilandGas.GasProperties.Calculations;
   ```

2. **Use in Service Methods**
   ```csharp
   public decimal CalculateGasProperty(string wellId, decimal pressure, decimal temperature)
   {
       var well = GetWell(wellId);
       var sg = well.GasSpecificGravity ?? 0.65m;
       
       return ZFactorCalculator.CalculateBrillBeggs(pressure, temperature, sg);
   }
   ```

---

## Data Storage

### PPDM Tables

**No Direct Storage** - Gas properties are calculated on-demand from well/reservoir data stored in:
- `WELL` - Well properties including gas specific gravity
- `PDEN` - Production entity properties
- `RESENT` - Reserve entity properties

---

## Best Practices

1. **Correlation Selection**
   - **Brill-Beggs**: General purpose, widely used
   - **Hall-Yarborough**: High-pressure applications
   - **Standing-Katz**: Standard chart correlation

2. **Input Validation**
   - Always validate pressure, temperature, and specific gravity
   - Check Z-factor range (typically 0.1 to 2.0)
   - Validate gas composition sums to 1.0

---

## Future Enhancements

### Planned Integrations

1. **Production Service Integration**
   - Automatic Z-factor calculations for gas wells
   - Gas property caching
   - Integration with production forecasting

2. **Well Test Analysis Integration**
   - Z-factor for well test analysis
   - Pseudo-pressure calculations
   - Integration with pressure transient analysis

---

## References

- **Project Location:** `Beep.OilandGas.GasProperties`
- **Used By:** Multiple calculation libraries integrated into LifeCycle
- **Documentation:** `Beep.OilandGas.GasProperties/README.md`

---

**Last Updated:** 2024  
**Status:** ✅ Used by Integrated Projects

