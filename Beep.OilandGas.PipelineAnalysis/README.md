# Beep.OilandGas.PipelineAnalysis

A comprehensive library for pipeline capacity and flow analysis in oil and gas operations, supporting both gas and liquid pipelines.

## Overview

`Beep.OilandGas.PipelineAnalysis` provides calculations for:
- Gas pipeline capacity calculations
- Liquid pipeline capacity calculations
- Flow rate calculations
- Pressure drop calculations
- Flow regime analysis

## Features

### ✅ Gas Pipeline Analysis
- Pipeline capacity calculations (Weymouth equation)
- Flow rate calculations
- Pressure drop calculations
- Friction factor calculations
- Reynolds number calculations
- Flow regime determination

### ✅ Liquid Pipeline Analysis
- Pipeline capacity calculations (Darcy-Weisbach equation)
- Flow rate calculations
- Pressure drop calculations
- Friction factor calculations
- Reynolds number calculations
- Flow regime determination

### ✅ Integration
- Integration with `Beep.OilandGas.Properties` for gas calculations
- Z-factor calculations
- Gas property support

## Installation

```bash
dotnet add package Beep.OilandGas.PipelineAnalysis
```

## Usage Examples

### Gas Pipeline Capacity Calculation

```csharp
using Beep.OilandGas.PipelineAnalysis.Models;
using Beep.OilandGas.PipelineAnalysis.Calculations;

// Define pipeline properties
var pipeline = new PipelineProperties
{
    Diameter = 12m, // inches
    Length = 50000m, // feet
    Roughness = 0.00015m, // feet (commercial steel)
    ElevationChange = 100m, // feet
    InletPressure = 1000m, // psia
    OutletPressure = 500m, // psia
    AverageTemperature = 540m // Rankine
};

// Define gas flow properties
var gasFlowProperties = new GasPipelineFlowProperties
{
    Pipeline = pipeline,
    GasFlowRate = 5000m, // Mscf/day
    GasSpecificGravity = 0.65m,
    GasMolecularWeight = 18.8m,
    BasePressure = 14.7m, // psia
    BaseTemperature = 520m // Rankine
};

// Calculate pipeline capacity
var capacityResult = PipelineCapacityCalculator.CalculateGasPipelineCapacity(
    gasFlowProperties);

// Access results
Console.WriteLine($"Maximum Flow Rate: {capacityResult.MaximumFlowRate:F2} Mscf/day");
Console.WriteLine($"Pressure Drop: {capacityResult.PressureDrop:F2} psi");
Console.WriteLine($"Flow Velocity: {capacityResult.FlowVelocity:F2} ft/s");
Console.WriteLine($"Reynolds Number: {capacityResult.ReynoldsNumber:F2}");
Console.WriteLine($"Friction Factor: {capacityResult.FrictionFactor:F4}");
Console.WriteLine($"Pressure Gradient: {capacityResult.PressureGradient:F4} psia/ft");
Console.WriteLine($"Outlet Pressure: {capacityResult.OutletPressure:F2} psia");
```

### Liquid Pipeline Capacity Calculation

```csharp
// Define liquid flow properties
var liquidFlowProperties = new LiquidPipelineFlowProperties
{
    Pipeline = pipeline,
    LiquidFlowRate = 10000m, // bbl/day
    LiquidSpecificGravity = 0.85m,
    LiquidViscosity = 2.0m // cp
};

// Calculate pipeline capacity
var liquidCapacityResult = PipelineCapacityCalculator.CalculateLiquidPipelineCapacity(
    liquidFlowProperties);

// Access results
Console.WriteLine($"Maximum Flow Rate: {liquidCapacityResult.MaximumFlowRate:F2} bbl/day");
Console.WriteLine($"Pressure Drop: {liquidCapacityResult.PressureDrop:F2} psi");
Console.WriteLine($"Flow Velocity: {liquidCapacityResult.FlowVelocity:F2} ft/s");
```

### Flow Rate Calculations

```csharp
// Calculate gas flow for given pressure drop
var flowResult = PipelineFlowCalculator.CalculateGasFlow(gasFlowProperties);

Console.WriteLine($"Flow Rate: {flowResult.FlowRate:F2} Mscf/day");
Console.WriteLine($"Pressure Drop: {flowResult.PressureDrop:F2} psi");
Console.WriteLine($"Flow Regime: {flowResult.FlowRegime}");
Console.WriteLine($"Reynolds Number: {flowResult.ReynoldsNumber:F2}");

// Calculate liquid flow
var liquidFlowResult = PipelineFlowCalculator.CalculateLiquidFlow(liquidFlowProperties);

Console.WriteLine($"Flow Rate: {liquidFlowResult.FlowRate:F2} bbl/day");
Console.WriteLine($"Flow Regime: {liquidFlowResult.FlowRegime}");

// Calculate pressure drop for given flow rate
decimal pressureDrop = PipelineFlowCalculator.CalculateGasPressureDrop(gasFlowProperties);
Console.WriteLine($"Pressure Drop: {pressureDrop:F2} psi");
```

## API Reference

### Models

- `PipelineProperties` - Pipeline physical properties
- `GasPipelineFlowProperties` - Gas pipeline flow properties
- `LiquidPipelineFlowProperties` - Liquid pipeline flow properties
- `PipelineCapacityResult` - Capacity calculation results
- `PipelineFlowAnalysisResult` - Flow analysis results

### Calculations

- `PipelineCapacityCalculator` - Pipeline capacity calculations
- `PipelineFlowCalculator` - Flow rate and pressure drop calculations

### Validation

- `PipelineValidator` - Input validation

### Constants

- `PipelineConstants` - Standard values, roughness values, conversion factors

### Exceptions

- `PipelineException` - Base exception
- `InvalidPipelinePropertiesException` - Invalid pipeline properties
- `InvalidFlowPropertiesException` - Invalid flow properties
- `PipelineParameterOutOfRangeException` - Parameter out of range

## Dependencies

- `Beep.OilandGas.Properties` - Gas property calculations
- `SkiaSharp` - Graphics rendering (for future visualization)

## Source Files

Based on Petroleum Engineer XLS files:
- `PipelineCapacity.xls`

## License

MIT License

## Contributing

Contributions are welcome! Please follow the project's coding standards and submit pull requests.

