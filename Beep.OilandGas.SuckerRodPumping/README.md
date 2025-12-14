# Beep.OilandGas.SuckerRodPumping

A comprehensive library for sucker rod pumping analysis and design in oil and gas operations.

## Overview

`Beep.OilandGas.SuckerRodPumping` provides calculations for:
- Sucker rod load analysis
- Production rate calculations
- Power requirements
- Pump card generation
- System optimization

## Features

### ✅ Sucker Rod Load Analysis
- Calculate peak and minimum loads
- Rod string weight calculations
- Fluid load calculations
- Dynamic load calculations
- Stress analysis
- Safety factor calculations

### ✅ Flow Rate and Power Calculations
- Pump displacement calculations
- Production rate calculations
- Volumetric efficiency
- Polished rod horsepower
- Hydraulic horsepower
- Friction horsepower
- Total power requirements
- Energy consumption

### ✅ Pump Card Generation
- Load vs position curves
- Net area calculations
- Performance visualization

### ✅ Integration
- Integration with `Beep.OilandGas.Properties` for gas calculations
- Z-factor calculations
- Gas property support

## Installation

```bash
dotnet add package Beep.OilandGas.SuckerRodPumping
```

## Usage Examples

### Sucker Rod Load Analysis

```csharp
using Beep.OilandGas.SuckerRodPumping.Models;
using Beep.OilandGas.SuckerRodPumping.Calculations;

// Define system properties
var systemProperties = new SuckerRodSystemProperties
{
    WellDepth = 5000m, // feet
    TubingDiameter = 2.875m, // inches
    RodDiameter = 0.875m, // inches
    PumpDiameter = 2.0m, // inches
    StrokeLength = 48m, // inches
    StrokesPerMinute = 12m, // SPM
    WellheadPressure = 50m, // psia
    BottomHolePressure = 500m, // psia
    OilGravity = 35m, // API
    WaterCut = 0.3m, // 30%
    GasOilRatio = 200m, // scf/bbl
    GasSpecificGravity = 0.65m,
    PumpEfficiency = 0.85m
};

// Define rod string
var rodString = new SuckerRodString
{
    Sections = new List<RodSection>
    {
        new RodSection
        {
            Diameter = 0.875m, // inches
            Length = 5000m, // feet
            Density = 490m // lb/ft³ (steel)
        }
    }
};

// Calculate loads
var loadResult = SuckerRodLoadCalculator.CalculateLoads(
    systemProperties, rodString);

// Access results
Console.WriteLine($"Peak Load: {loadResult.PeakLoad:F2} lb");
Console.WriteLine($"Minimum Load: {loadResult.MinimumLoad:F2} lb");
Console.WriteLine($"Load Range: {loadResult.LoadRange:F2} lb");
Console.WriteLine($"Maximum Stress: {loadResult.MaximumStress:F2} psi");
Console.WriteLine($"Safety Factor: {loadResult.LoadFactor:F2}");
```

### Flow Rate and Power Calculations

```csharp
// Calculate flow rate and power
var flowRatePowerResult = SuckerRodFlowRatePowerCalculator.CalculateFlowRateAndPower(
    systemProperties, loadResult);

// Access results
Console.WriteLine($"Production Rate: {flowRatePowerResult.ProductionRate:F2} bbl/day");
Console.WriteLine($"Pump Displacement: {flowRatePowerResult.PumpDisplacement:F2} bbl/day");
Console.WriteLine($"Volumetric Efficiency: {flowRatePowerResult.VolumetricEfficiency:P2}");
Console.WriteLine($"Polished Rod HP: {flowRatePowerResult.PolishedRodHorsepower:F2} HP");
Console.WriteLine($"Hydraulic HP: {flowRatePowerResult.HydraulicHorsepower:F2} HP");
Console.WriteLine($"Total HP: {flowRatePowerResult.TotalHorsepower:F2} HP");
Console.WriteLine($"Motor HP: {flowRatePowerResult.MotorHorsepower:F2} HP");
Console.WriteLine($"System Efficiency: {flowRatePowerResult.SystemEfficiency:P2}");
Console.WriteLine($"Energy Consumption: {flowRatePowerResult.EnergyConsumption:F2} kWh/day");
```

### Pump Card Generation

```csharp
// Generate pump card
var pumpCard = SuckerRodLoadCalculator.GeneratePumpCard(
    systemProperties, rodString);

// Access pump card data
Console.WriteLine($"Peak Load: {pumpCard.PeakLoad:F2} lb");
Console.WriteLine($"Minimum Load: {pumpCard.MinimumLoad:F2} lb");
Console.WriteLine($"Net Area: {pumpCard.NetArea:F2} lb-in");

// Access individual points
foreach (var point in pumpCard.Points)
{
    Console.WriteLine($"Position: {point.Position:P2}, Load: {point.Load:F2} lb");
}
```

### Quick Calculations

```csharp
// Calculate production rate only
decimal productionRate = SuckerRodFlowRatePowerCalculator.CalculateProductionRate(
    systemProperties);

// Calculate power requirements only
decimal powerRequired = SuckerRodFlowRatePowerCalculator.CalculatePowerRequirements(
    systemProperties, loadResult);
```

## API Reference

### Models

- `SuckerRodSystemProperties` - System properties for analysis
- `SuckerRodString` - Rod string configuration
- `RodSection` - Individual rod section
- `SuckerRodLoadResult` - Load analysis results
- `SuckerRodFlowRatePowerResult` - Flow rate and power results
- `PumpCard` - Pump card (load vs position)
- `PumpCardPoint` - Pump card point

### Calculations

- `SuckerRodLoadCalculator` - Load analysis and pump card generation
- `SuckerRodFlowRatePowerCalculator` - Flow rate and power calculations

### Validation

- `SuckerRodValidator` - Input validation

### Constants

- `SuckerRodConstants` - Standard values and conversion factors

### Exceptions

- `SuckerRodException` - Base exception
- `InvalidSystemPropertiesException` - Invalid system properties
- `InvalidRodStringException` - Invalid rod string
- `SuckerRodParameterOutOfRangeException` - Parameter out of range
- `RodStressExceededException` - Rod stress exceeds safe limits

## Dependencies

- `Beep.OilandGas.Properties` - Gas property calculations
- `SkiaSharp` - Graphics rendering (for future visualization)

## Source Files

Based on Petroleum Engineer XLS files:
- `SuckerRodPumpingLoad.xls`
- `SuckerRodPumpingFlowRate&Power.xls`

## License

MIT License

## Contributing

Contributions are welcome! Please follow the project's coding standards and submit pull requests.

