# Beep.OilandGas.PlungerLift

A comprehensive library for plunger lift analysis and design in oil and gas operations.

## Overview

`Beep.OilandGas.PlungerLift` provides calculations for:
- Plunger lift cycle analysis
- Gas requirements calculations
- Production rate predictions
- System feasibility analysis
- Performance optimization

## Features

### ✅ Plunger Lift Cycle Analysis
- Calculate cycle time (fall, rise, shut-in)
- Plunger velocity calculations
- Liquid slug size calculations
- Production per cycle
- Daily production rate

### ✅ Gas Requirements
- Required gas injection rate
- Available gas from well
- Additional gas requirements
- Gas-liquid ratio calculations
- Casing pressure requirements

### ✅ Performance Analysis
- Complete performance analysis
- System feasibility checks
- System efficiency calculations
- Optimization recommendations

### ✅ Integration
- Integration with `Beep.OilandGas.Properties` for gas calculations
- Z-factor calculations
- Gas property support

## Installation

```bash
dotnet add package Beep.OilandGas.PlungerLift
```

## Usage Examples

### Plunger Lift Cycle Analysis

```csharp
using Beep.OilandGas.PlungerLift.Models;
using Beep.OilandGas.PlungerLift.Calculations;

// Define well properties
var wellProperties = new PlungerLiftWellProperties
{
    WellDepth = 8000m, // feet
    TubingDiameter = 2.875m, // inches
    PlungerDiameter = 2.5m, // inches
    WellheadPressure = 50m, // psia
    CasingPressure = 200m, // psia
    BottomHolePressure = 500m, // psia
    WellheadTemperature = 520m, // Rankine
    BottomHoleTemperature = 580m, // Rankine
    OilGravity = 35m, // API
    WaterCut = 0.2m, // 20%
    GasOilRatio = 300m, // scf/bbl
    GasSpecificGravity = 0.65m,
    LiquidProductionRate = 50m // bbl/day
};

// Analyze cycle
var cycleResult = PlungerLiftCalculator.AnalyzeCycle(wellProperties);

// Access results
Console.WriteLine($"Cycle Time: {cycleResult.CycleTime:F2} minutes");
Console.WriteLine($"Fall Time: {cycleResult.FallTime:F2} minutes");
Console.WriteLine($"Rise Time: {cycleResult.RiseTime:F2} minutes");
Console.WriteLine($"Shut-In Time: {cycleResult.ShutInTime:F2} minutes");
Console.WriteLine($"Fall Velocity: {cycleResult.FallVelocity:F2} ft/min");
Console.WriteLine($"Rise Velocity: {cycleResult.RiseVelocity:F2} ft/min");
Console.WriteLine($"Liquid Slug Size: {cycleResult.LiquidSlugSize:F2} bbl");
Console.WriteLine($"Production Per Cycle: {cycleResult.ProductionPerCycle:F2} bbl");
Console.WriteLine($"Cycles Per Day: {cycleResult.CyclesPerDay:F2}");
Console.WriteLine($"Daily Production Rate: {cycleResult.DailyProductionRate:F2} bbl/day");
```

### Gas Requirements

```csharp
// Calculate gas requirements
var gasRequirements = PlungerLiftCalculator.CalculateGasRequirements(
    wellProperties, cycleResult);

// Access results
Console.WriteLine($"Required Gas Injection Rate: {gasRequirements.RequiredGasInjectionRate:F2} Mscf/day");
Console.WriteLine($"Available Gas: {gasRequirements.AvailableGas:F2} Mscf/day");
Console.WriteLine($"Additional Gas Required: {gasRequirements.AdditionalGasRequired:F2} Mscf/day");
Console.WriteLine($"Required GLR: {gasRequirements.RequiredGasLiquidRatio:F2} scf/bbl");
Console.WriteLine($"Minimum Casing Pressure: {gasRequirements.MinimumCasingPressure:F2} psia");
Console.WriteLine($"Maximum Casing Pressure: {gasRequirements.MaximumCasingPressure:F2} psia");
```

### Complete Performance Analysis

```csharp
// Perform complete performance analysis
var performanceResult = PlungerLiftCalculator.AnalyzePerformance(wellProperties);

// Access results
Console.WriteLine($"System Feasible: {performanceResult.IsFeasible}");
Console.WriteLine($"System Efficiency: {performanceResult.SystemEfficiency:P2}");

if (!performanceResult.IsFeasible)
{
    Console.WriteLine("Feasibility Issues:");
    foreach (var reason in performanceResult.FeasibilityReasons)
    {
        Console.WriteLine($"  - {reason}");
    }
}

// Access cycle and gas requirements
Console.WriteLine($"Cycle Time: {performanceResult.CycleResult.CycleTime:F2} minutes");
Console.WriteLine($"Daily Production: {performanceResult.CycleResult.DailyProductionRate:F2} bbl/day");
Console.WriteLine($"Required Gas: {performanceResult.GasRequirements.RequiredGasInjectionRate:F2} Mscf/day");
```

## API Reference

### Models

- `PlungerLiftWellProperties` - Well properties for analysis
- `PlungerLiftCycleResult` - Cycle analysis results
- `PlungerLiftGasRequirements` - Gas requirements
- `PlungerLiftPerformanceResult` - Complete performance analysis
- `PlungerLiftCyclePhase` - Cycle phase enumeration
- `PlungerLiftCyclePoint` - Cycle point data

### Calculations

- `PlungerLiftCalculator` - All plunger lift calculations

### Validation

- `PlungerLiftValidator` - Input validation

### Constants

- `PlungerLiftConstants` - Standard values and limits

### Exceptions

- `PlungerLiftException` - Base exception
- `InvalidWellPropertiesException` - Invalid well properties
- `PlungerLiftParameterOutOfRangeException` - Parameter out of range
- `PlungerLiftNotFeasibleException` - System not feasible

## Dependencies

- `Beep.OilandGas.Properties` - Gas property calculations
- `SkiaSharp` - Graphics rendering (for future visualization)

## Source Files

Based on Petroleum Engineer XLS files:
- `PlungerLift.xls`

## License

MIT License

## Contributing

Contributions are welcome! Please follow the project's coding standards and submit pull requests.

