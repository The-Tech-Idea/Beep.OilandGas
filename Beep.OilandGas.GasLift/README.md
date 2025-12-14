# Beep.OilandGas.GasLift

A comprehensive library for gas lift analysis and design in oil and gas operations.

## Overview

`Beep.OilandGas.GasLift` provides calculations for:
- Gas lift potential analysis
- Gas lift valve design (SI and US field units)
- Gas lift valve spacing calculations
- Performance curve generation
- System optimization

## Features

### ✅ Gas Lift Potential Analysis
- Analyze production potential with gas lift
- Generate performance curves
- Find optimal gas injection rate
- Calculate maximum production rate

### ✅ Gas Lift Valve Design
- Design valves for US field units
- Design valves for SI units
- Automatic port size selection
- Opening/closing pressure calculations

### ✅ Valve Spacing Calculations
- Equal pressure drop spacing
- Equal depth spacing
- Optimal valve placement
- Temperature and pressure corrections

### ✅ Integration
- Integration with `Beep.OilandGas.Properties` for gas calculations
- Z-factor calculations
- Gas property support

## Installation

```bash
dotnet add package Beep.OilandGas.GasLift
```

## Usage Examples

### Gas Lift Potential Analysis

```csharp
using Beep.OilandGas.GasLift.Models;
using Beep.OilandGas.GasLift.Calculations;

// Define well properties
var wellProperties = new GasLiftWellProperties
{
    WellDepth = 10000m, // feet
    TubingDiameter = 2.875m, // inches
    CasingDiameter = 7.0m, // inches
    WellheadPressure = 100m, // psia
    BottomHolePressure = 2000m, // psia
    WellheadTemperature = 520m, // Rankine
    BottomHoleTemperature = 580m, // Rankine
    OilGravity = 35m, // API
    WaterCut = 0.3m, // 30%
    GasOilRatio = 500m, // scf/bbl
    GasSpecificGravity = 0.65m,
    DesiredProductionRate = 2000m // bbl/day
};

// Analyze gas lift potential
var potentialResult = GasLiftPotentialCalculator.AnalyzeGasLiftPotential(
    wellProperties,
    minGasInjectionRate: 100m, // Mscf/day
    maxGasInjectionRate: 5000m, // Mscf/day
    numberOfPoints: 50);

// Access results
Console.WriteLine($"Optimal Gas Injection Rate: {potentialResult.OptimalGasInjectionRate:F2} Mscf/day");
Console.WriteLine($"Maximum Production Rate: {potentialResult.MaximumProductionRate:F2} bbl/day");
Console.WriteLine($"Optimal GLR: {potentialResult.OptimalGasLiquidRatio:F2}");
```

### Gas Lift Valve Design (US Field Units)

```csharp
// Design valves for US field units
var designResult = GasLiftValveDesignCalculator.DesignValvesUS(
    wellProperties,
    gasInjectionPressure: 1500m, // psia
    numberOfValves: 5);

// Access results
Console.WriteLine($"Number of Valves: {designResult.Valves.Count}");
Console.WriteLine($"Total Gas Injection Rate: {designResult.TotalGasInjectionRate:F2} Mscf/day");
Console.WriteLine($"Expected Production Rate: {designResult.ExpectedProductionRate:F2} bbl/day");
Console.WriteLine($"System Efficiency: {designResult.SystemEfficiency:P2}");

// Access individual valve properties
foreach (var valve in designResult.Valves)
{
    Console.WriteLine($"Valve at {valve.Depth:F0} ft: " +
                     $"Port Size = {valve.PortSize:F3} in, " +
                     $"Opening Pressure = {valve.OpeningPressure:F2} psia, " +
                     $"Gas Rate = {valve.GasInjectionRate:F2} Mscf/day");
}
```

### Gas Lift Valve Design (SI Units)

```csharp
// Design valves for SI units
var designResultSI = GasLiftValveDesignCalculator.DesignValvesSI(
    wellProperties,
    gasInjectionPressure: 1500m, // psia (converted internally)
    numberOfValves: 5);
```

### Valve Spacing Calculations

```csharp
using Beep.OilandGas.GasLift.Calculations;

// Calculate equal pressure drop spacing
var spacingResult = GasLiftValveSpacingCalculator.CalculateEqualPressureDropSpacing(
    wellProperties,
    gasInjectionPressure: 1500m, // psia
    numberOfValves: 5);

// Access spacing results
Console.WriteLine($"Number of Valves: {spacingResult.NumberOfValves}");
Console.WriteLine($"Total Depth Coverage: {spacingResult.TotalDepthCoverage:F0} ft");

for (int i = 0; i < spacingResult.ValveDepths.Count; i++)
{
    Console.WriteLine($"Valve {i + 1}: Depth = {spacingResult.ValveDepths[i]:F0} ft, " +
                     $"Opening Pressure = {spacingResult.OpeningPressures[i]:F2} psia");
}

// Calculate equal depth spacing
var equalDepthSpacing = GasLiftValveSpacingCalculator.CalculateEqualDepthSpacing(
    wellProperties,
    gasInjectionPressure: 1500m,
    numberOfValves: 5);
```

## API Reference

### Models

- `GasLiftWellProperties` - Well properties for gas lift analysis
- `GasLiftValve` - Gas lift valve properties
- `GasLiftPotentialResult` - Gas lift potential analysis results
- `GasLiftValveDesignResult` - Valve design results
- `GasLiftValveSpacingResult` - Valve spacing results
- `GasLiftPerformancePoint` - Performance curve point

### Calculations

- `GasLiftPotentialCalculator` - Gas lift potential analysis
- `GasLiftValveDesignCalculator` - Valve design (US and SI units)
- `GasLiftValveSpacingCalculator` - Valve spacing calculations

### Validation

- `GasLiftValidator` - Input validation

### Constants

- `GasLiftConstants` - Gas lift calculation constants

### Exceptions

- `GasLiftException` - Base exception
- `InvalidWellPropertiesException` - Invalid well properties
- `GasLiftParameterOutOfRangeException` - Parameter out of range
- `GasLiftDesignException` - Design failure

## Dependencies

- `Beep.OilandGas.Properties` - Gas property calculations
- `SkiaSharp` - Graphics rendering (for future visualization)

## Source Files

Based on Petroleum Engineer XLS files:
- `GasLiftPotential.xls`
- `GasLiftValveDesign-SI Units.xls`
- `GasLiftValveDesign-US Field Units.xls`
- `GasLiftValveSpacing.xls`

## License

MIT License

## Contributing

Contributions are welcome! Please follow the project's coding standards and submit pull requests.

