# Beep.OilandGas.CompressorAnalysis

A comprehensive library for compressor analysis and design in oil and gas operations, supporting both centrifugal and reciprocating compressors.

## Overview

`Beep.OilandGas.CompressorAnalysis` provides calculations for:
- Centrifugal compressor power requirements
- Reciprocating compressor power requirements
- Compressor pressure calculations
- Compression ratio analysis
- System efficiency calculations

## Features

### ✅ Centrifugal Compressor Analysis
- Polytropic head calculations
- Adiabatic head calculations
- Power requirements
- Discharge temperature calculations
- Multi-stage support

### ✅ Reciprocating Compressor Analysis
- Cylinder displacement calculations
- Volumetric efficiency
- Power requirements
- Discharge temperature calculations
- Multi-cylinder support

### ✅ Compressor Pressure Calculations
- Required discharge pressure
- Maximum flow rate calculations
- Feasibility analysis

### ✅ Integration
- Integration with `Beep.OilandGas.Properties` for gas calculations
- Z-factor calculations
- Gas property support
- SI and US field units support

## Installation

```bash
dotnet add package Beep.OilandGas.CompressorAnalysis
```

## Usage Examples

### Centrifugal Compressor Power Calculation

```csharp
using Beep.OilandGas.CompressorAnalysis.Models;
using Beep.OilandGas.CompressorAnalysis.Calculations;

// Define operating conditions
var operatingConditions = new CompressorOperatingConditions
{
    SUCTION_PRESSURE = 100m, // psia
    DISCHARGE_PRESSURE = 500m, // psia
    SUCTION_TEMPERATURE = 520m, // Rankine
    DischargeTemperature = 600m, // Rankine
    GAS_FLOW_RATE = 1000m, // Mscf/day
    GAS_SPECIFIC_GRAVITY = 0.65m,
    GasMolecularWeight = 18.8m,
    COMPRESSOR_EFFICIENCY = 0.75m,
    MECHANICAL_EFFICIENCY = 0.95m
};

// Define centrifugal compressor properties
var compressorProperties = new CentrifugalCompressorProperties
{
    OperatingConditions = operatingConditions,
    POLYTROPIC_EFFICIENCY = 0.75m,
    SPECIFIC_HEAT_RATIO = 1.3m,
    NUMBER_OF_STAGES = 1,
    SPEED = 3600m // RPM
};

// Calculate power requirements
var result = CentrifugalCompressorCalculator.CalculatePower(
    compressorProperties,
    useSIUnits: false);

// Access results
Console.WriteLine($"Theoretical Power: {result.TheoreticalPower:F2} HP");
Console.WriteLine($"Brake Horsepower: {result.BrakeHorsepower:F2} HP");
Console.WriteLine($"Motor Horsepower: {result.MotorHorsepower:F2} HP");
Console.WriteLine($"Power Consumption: {result.PowerConsumptionKW:F2} kW");
Console.WriteLine($"Compression Ratio: {result.CompressionRatio:F2}");
Console.WriteLine($"Polytropic Head: {result.PolytropicHead:F2} ft");
Console.WriteLine($"Adiabatic Head: {result.AdiabaticHead:F2} ft");
Console.WriteLine($"Discharge Temperature: {result.DischargeTemperature:F2} R");
Console.WriteLine($"Overall Efficiency: {result.OverallEfficiency:P2}");
```

### Reciprocating Compressor Power Calculation

```csharp
// Define reciprocating compressor properties
var reciprocatingProperties = new ReciprocatingCompressorProperties
{
    OperatingConditions = operatingConditions,
    CylinderDiameter = 8.0m, // inches
    StrokeLength = 12.0m, // inches
    RotationalSpeed = 300m, // RPM
    NumberOfCylinders = 2,
    VolumetricEfficiency = 0.85m,
    ClearanceFactor = 0.05m
};

// Calculate power requirements
var reciprocatingResult = ReciprocatingCompressorCalculator.CalculatePower(
    reciprocatingProperties,
    useSIUnits: false);

// Access results
Console.WriteLine($"Theoretical Power: {reciprocatingResult.TheoreticalPower:F2} HP");
Console.WriteLine($"Brake Horsepower: {reciprocatingResult.BrakeHorsepower:F2} HP");
Console.WriteLine($"Motor Horsepower: {reciprocatingResult.MotorHorsepower:F2} HP");
```

### Compressor Pressure Calculations

```csharp
// Calculate required discharge pressure
var pressureResult = CompressorPressureCalculator.CalculateRequiredPressure(
    operatingConditions,
    requiredFlowRate: 1500m, // Mscf/day
    maxPower: 500m, // HP
    COMPRESSOR_EFFICIENCY: 0.75m);

// Access results
Console.WriteLine($"Required Discharge Pressure: {pressureResult.RequiredDischargePressure:F2} psia");
Console.WriteLine($"Compression Ratio: {pressureResult.CompressionRatio:F2}");
Console.WriteLine($"Required Power: {pressureResult.RequiredPower:F2} HP");
Console.WriteLine($"Discharge Temperature: {pressureResult.DischargeTemperature:F2} R");
Console.WriteLine($"Is Feasible: {pressureResult.IsFeasible}");

// Calculate maximum flow rate
decimal maxFlowRate = CompressorPressureCalculator.CalculateMaximumFlowRate(
    operatingConditions,
    compressionRatio: 5.0m,
    maxPower: 500m,
    COMPRESSOR_EFFICIENCY: 0.75m);

Console.WriteLine($"Maximum Flow Rate: {maxFlowRate:F2} Mscf/day");
```

## API Reference

### Models

- `CompressorOperatingConditions` - Operating conditions for compressors
- `CentrifugalCompressorProperties` - Centrifugal compressor properties
- `ReciprocatingCompressorProperties` - Reciprocating compressor properties
- `CompressorPowerResult` - Power calculation results
- `CompressorPressureResult` - Pressure calculation results

### Calculations

- `CentrifugalCompressorCalculator` - Centrifugal compressor calculations
- `ReciprocatingCompressorCalculator` - Reciprocating compressor calculations
- `CompressorPressureCalculator` - Compressor pressure calculations

### Validation

- `CompressorValidator` - Input validation

### Constants

- `CompressorConstants` - Standard values and conversion factors

### Exceptions

- `CompressorException` - Base exception
- `InvalidOperatingConditionsException` - Invalid operating conditions
- `InvalidCompressorPropertiesException` - Invalid compressor properties
- `CompressorParameterOutOfRangeException` - Parameter out of range
- `CompressorNotFeasibleException` - Operation not feasible

## Dependencies

- `Beep.OilandGas.Properties` - Gas property calculations
- `SkiaSharp` - Graphics rendering (for future visualization)

## Source Files

Based on Petroleum Engineer XLS files:
- `CentrifugalCompressorPower-*.xls`
- `ReciprocatingCompressorPower-*.xls`
- `CompressorPressure.xls`

## License

MIT License

## Contributing

Contributions are welcome! Please follow the project's coding standards and submit pull requests.

