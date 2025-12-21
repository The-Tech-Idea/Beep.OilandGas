# Beep.OilandGas.FlashCalculations - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.FlashCalculations** is a comprehensive library for phase equilibrium and flash calculations in oil and gas operations.

### Key Capabilities
- **Isothermal Flash Calculations**: Pressure and temperature specified flash
- **Phase Equilibrium**: Vapor-liquid equilibrium calculations
- **K-Value Calculations**: Equilibrium ratio calculations
- **Phase Composition**: Vapor and liquid phase compositions
- **Phase Properties**: Phase property calculations

### Current Status
⚠️ **Not Yet Integrated** - Should be integrated into `PPDMCalculationService` for centralized phase behavior analysis

### Architecture Decision
**Service:** `PPDMCalculationService` (Centralized Calculation Service)  
**Rationale:** Pure calculation/analysis project - performs phase equilibrium calculations without operational data management. Centralized service provides consistent data mapping and result storage.

---

## Key Classes and Interfaces

### Main Classes

#### `FlashCalculator`
Flash calculation methods.

**Key Methods:**
```csharp
public static class FlashCalculator
{
    public static FlashResult PerformIsothermalFlash(FlashConditions conditions);
    public static PhaseProperties CalculateVaporProperties(
        FlashResult flashResult, 
        FlashConditions conditions);
    public static PhaseProperties CalculateLiquidProperties(
        FlashResult flashResult, 
        FlashConditions conditions);
}
```

---

## Integration with LifeCycle Services

### Planned Integration

**Service:** `PPDMCalculationService` or `PPDMProductionService`  
**Location:** `Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService`

### Integration Points

1. **Flash Calculation Method**
   - Method: `PerformFlashCalculationAsync(FlashCalculationRequest request)`
   - Used for phase behavior analysis
   - Stores results in calculation table

---

## Usage Examples

### Example 1: Basic Flash Calculation

```csharp
using Beep.OilandGas.FlashCalculations.Models;
using Beep.OilandGas.FlashCalculations.Calculations;

var conditions = new FlashConditions
{
    Pressure = 500m,
    Temperature = 540m,
    FeedComposition = new List<Component> { methane, ethane, propane }
};

var flashResult = FlashCalculator.PerformIsothermalFlash(conditions);

Console.WriteLine($"Vapor Fraction: {flashResult.VaporFraction:F4}");
Console.WriteLine($"Liquid Fraction: {flashResult.LiquidFraction:F4}");
```

---

## Data Storage

### PPDM Tables

#### New Table Required: `FLASH_CALCULATION`

**Status:** ⚠️ **New Table Needed** - Does not exist in PPDM39, must be created following PPDM patterns.

**Table Structure:**
```sql
CREATE TABLE FLASH_CALCULATION (
    FLASH_CALCULATION_ID VARCHAR(50) PRIMARY KEY,
    WELL_ID VARCHAR(50) NULL,
    FACILITY_ID VARCHAR(50) NULL,
    CALCULATION_DATE DATETIME,
    PRESSURE DECIMAL(18,2),           -- psia
    TEMPERATURE DECIMAL(18,2),        -- Rankine
    VAPOR_FRACTION DECIMAL(10,4),
    LIQUID_FRACTION DECIMAL(10,4),
    FEED_COMPOSITION_JSON NVARCHAR(MAX),  -- JSON serialized feed composition
    VAPOR_COMPOSITION_JSON NVARCHAR(MAX), -- JSON serialized vapor composition
    LIQUID_COMPOSITION_JSON NVARCHAR(MAX), -- JSON serialized liquid composition
    K_VALUES_JSON NVARCHAR(MAX),          -- JSON serialized K-values
    -- Standard PPDM columns
    ROW_ID VARCHAR(50),
    ROW_CHANGED_BY VARCHAR(50),
    ROW_CHANGED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50),
    ROW_CREATED_DATE DATETIME
);
```

**Foreign Keys:**
- `FLASH_CALCULATION.WELL_ID` → `WELL.WELL_ID` (optional)
- `FLASH_CALCULATION.FACILITY_ID` → `FACILITY.FACILITY_ID` (optional)

**Notes:**
- Store compositions and K-values as JSON for flexibility
- Can be linked to WELL or FACILITY depending on use case
- Follow PPDM naming conventions and include standard PPDM audit columns

---

## References

- **Project Location:** `Beep.OilandGas.FlashCalculations`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService` (planned)
- **Documentation:** `Beep.OilandGas.FlashCalculations/README.md`

---

**Last Updated:** 2024  
**Status:** ⚠️ Not Yet Integrated (Should be integrated)

