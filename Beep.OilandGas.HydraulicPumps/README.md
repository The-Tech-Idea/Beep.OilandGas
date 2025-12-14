# Beep.OilandGas.HydraulicPumps

A comprehensive library for hydraulic pump analysis and design in oil and gas operations, supporting both jet pumps and piston pumps.

## Overview

`Beep.OilandGas.HydraulicPumps` provides calculations for:
- Hydraulic jet pump performance analysis
- Hydraulic piston pump performance analysis
- Power fluid requirements
- Production rate predictions
- System efficiency calculations

## Features

### ✅ Hydraulic Jet Pump Analysis
- Production rate calculations
- Power fluid requirements
- Pump efficiency calculations
- Area ratio optimization
- Pressure calculations

### ✅ Hydraulic Piston Pump Analysis
- Pump displacement calculations
- Production rate calculations
- Volumetric efficiency
- Power fluid consumption
- Power requirements

### ✅ Integration
- Integration with `Beep.OilandGas.Properties` for gas calculations
- Z-factor calculations
- Gas property support

## Installation

```bash
dotnet add package Beep.OilandGas.HydraulicPumps
```

## Usage Examples

### Hydraulic Jet Pump Analysis

```csharp
using Beep.OilandGas.HydraulicPumps.Models;
using Beep.OilandGas.HydraulicPumps.Calculations;

// Define well properties
var wellProperties = new HydraulicPumpWellProperties
{
    WellDepth = 8000m, // feet
    TubingDiameter = 2.875m, // inches
    CasingDiameter = 7.0m, // inches
    WellheadPressure = 50m, // psia
    BottomHolePressure = 500m, // psia
    WellheadTemperature = 520m, // Rankine
    BottomHoleTemperature = 580m, // Rankine
    OilGravity = 35m, // API
    WaterCut = 0.2m, // 20%
    GasOilRatio = 200m, // scf/bbl
    GasSpecificGravity = 0.65m,
    DesiredProductionRate = 500m // bbl/day
};

// Define jet pump properties
var jetPumpProperties = new HydraulicJetPumpProperties
{
    NozzleDiameter = 0.5m, // inches
    ThroatDiameter = 1.0m, // inches
    DiffuserDiameter = 1.5m, // inches
    PowerFluidPressure = 2000m, // psia
    PowerFluidRate = 300m, // bbl/day
    PowerFluidSpecificGravity = 1.0m
};

// Calculate jet pump performance
var jetPumpResult = HydraulicJetPumpCalculator.CalculatePerformance(
    wellProperties, jetPumpProperties);

// Access results
Console.WriteLine($"Production Rate: {jetPumpResult.ProductionRate:F2} bbl/day");
Console.WriteLine($"Total Flow Rate: {jetPumpResult.TotalFlowRate:F2} bbl/day");
Console.WriteLine($"Production Ratio: {jetPumpResult.ProductionRatio:F2}");
Console.WriteLine($"Pump Efficiency: {jetPumpResult.PumpEfficiency:P2}");
Console.WriteLine($"Power Fluid HP: {jetPumpResult.PowerFluidHorsepower:F2} HP");
Console.WriteLine($"Hydraulic HP: {jetPumpResult.HydraulicHorsepower:F2} HP");
Console.WriteLine($"System Efficiency: {jetPumpResult.SystemEfficiency:P2}");
Console.WriteLine($"Pump Intake Pressure: {jetPumpResult.PumpIntakePressure:F2} psia");
Console.WriteLine($"Pump Discharge Pressure: {jetPumpResult.PumpDischargePressure:F2} psia");
```

### Hydraulic Piston Pump Analysis

```csharp
// Define piston pump properties
var pistonPumpProperties = new HydraulicPistonPumpProperties
{
    PistonDiameter = 2.0m, // inches
    RodDiameter = 0.875m, // inches
    StrokeLength = 48m, // inches
    StrokesPerMinute = 12m, // SPM
    PowerFluidPressure = 2000m, // psia
    PowerFluidRate = 400m, // bbl/day
    PowerFluidSpecificGravity = 1.0m
};

// Calculate piston pump performance
var pistonPumpResult = HydraulicPistonPumpCalculator.CalculatePerformance(
    wellProperties, pistonPumpProperties);

// Access results
Console.WriteLine($"Production Rate: {pistonPumpResult.ProductionRate:F2} bbl/day");
Console.WriteLine($"Pump Displacement: {pistonPumpResult.PumpDisplacement:F2} bbl/day");
Console.WriteLine($"Volumetric Efficiency: {pistonPumpResult.VolumetricEfficiency:P2}");
Console.WriteLine($"Power Fluid Consumption: {pistonPumpResult.PowerFluidConsumption:F2} bbl/day");
Console.WriteLine($"Power Fluid HP: {pistonPumpResult.PowerFluidHorsepower:F2} HP");
Console.WriteLine($"Hydraulic HP: {pistonPumpResult.HydraulicHorsepower:F2} HP");
Console.WriteLine($"System Efficiency: {pistonPumpResult.SystemEfficiency:P2}");
Console.WriteLine($"Pump Intake Pressure: {pistonPumpResult.PumpIntakePressure:F2} psia");
Console.WriteLine($"Pump Discharge Pressure: {pistonPumpResult.PumpDischargePressure:F2} psia");
```

## API Reference

### Models

- `HydraulicPumpWellProperties` - Well properties for analysis
- `HydraulicJetPumpProperties` - Jet pump properties
- `HydraulicPistonPumpProperties` - Piston pump properties
- `HydraulicJetPumpResult` - Jet pump performance results
- `HydraulicPistonPumpResult` - Piston pump performance results

### Calculations

- `HydraulicJetPumpCalculator` - Jet pump calculations
- `HydraulicPistonPumpCalculator` - Piston pump calculations

### Validation

- `HydraulicPumpValidator` - Input validation

### Constants

- `HydraulicPumpConstants` - Standard values and conversion factors

### Exceptions

- `HydraulicPumpException` - Base exception
- `InvalidWellPropertiesException` - Invalid well properties
- `InvalidPumpPropertiesException` - Invalid pump properties
- `HydraulicPumpParameterOutOfRangeException` - Parameter out of range

## Dependencies

- `Beep.OilandGas.Properties` - Gas property calculations
- `SkiaSharp` - Graphics rendering (for future visualization)

## Source Files

Based on Petroleum Engineer XLS files:
- `HydraulicJetPump.xls`
- `HydraulicPistonPump.xls`

## License

MIT License

## Contributing

Contributions are welcome! Please follow the project's coding standards and submit pull requests.

