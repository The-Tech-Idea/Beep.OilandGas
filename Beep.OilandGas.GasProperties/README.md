# Beep.OilandGas.GasProperties

A comprehensive library for calculating gas properties in oil and gas engineering applications.

## Overview

This library provides industry-standard methods for calculating gas properties including:
- **Z-factor (Compressibility Factor)** - Multiple correlation methods
- **Gas Viscosity** - Carr-Kobayashi-Burrows and Lee-Gonzalez-Eakin correlations
- **Pseudo-Pressure** - Numerical integration methods
- **Average Properties** - Pressure-weighted and arithmetic averaging

## Features

### Z-Factor Calculations

- **Brill-Beggs Correlation** - Widely used for natural gas
- **Hall-Yarborough Correlation** - Accurate for high-pressure applications
- **Standing-Katz Chart Correlation** - Dranchuk-Abu-Kassem approximation
- **Pseudo-Critical Properties** - From gas composition

### Gas Viscosity

- **Carr-Kobayashi-Burrows** - Industry standard correlation
- **Lee-Gonzalez-Eakin** - Alternative method

### Pseudo-Pressure

- **Simpson's Rule Integration** - High accuracy
- **Trapezoidal Integration** - Alternative method
- **Pseudo-Pressure Curves** - Generate complete curves

### Average Properties

- **Pressure-Weighted Averaging** - For variable pressure systems
- **Arithmetic Averaging** - Simple mean calculations
- **Range Averaging** - Over pressure ranges

## Installation

```bash
dotnet add package Beep.OilandGas.GasProperties
```

## Quick Start

### Calculate Z-Factor

```csharp
using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.GasProperties.Validation;

// Calculate Z-factor using Brill-Beggs correlation
decimal pressure = 2000m; // psia
decimal temperature = 580m; // Rankine (120°F)
decimal specificGravity = 0.65m; // Relative to air

// Validate inputs
GasPropertiesValidator.ValidateCalculationParameters(pressure, temperature, specificGravity);

// Calculate Z-factor
decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
    pressure, temperature, specificGravity);

Console.WriteLine($"Z-factor: {zFactor:F4}");
```

### Calculate Gas Viscosity

```csharp
using Beep.OilandGas.GasProperties.Calculations;

// Calculate gas viscosity using Carr-Kobayashi-Burrows
decimal zFactor = 0.85m; // From previous calculation

decimal viscosity = GasViscosityCalculator.CalculateCarrKobayashiBurrows(
    pressure, temperature, specificGravity, zFactor);

Console.WriteLine($"Gas viscosity: {viscosity:F4} cp");
```

### Calculate Pseudo-Pressure

```csharp
using Beep.OilandGas.GasProperties.Calculations;

// Calculate pseudo-pressure
decimal pseudoPressure = PseudoPressureCalculator.CalculatePseudoPressure(
    pressure,
    temperature,
    specificGravity,
    ZFactorCalculator.CalculateBrillBeggs,
    GasViscosityCalculator.CalculateCarrKobayashiBurrows);

Console.WriteLine($"Pseudo-pressure: {pseudoPressure:F2} psia²/cp");
```

### Calculate Average Properties

```csharp
using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.GasProperties.Models;

// Calculate pressure-weighted average
var pressures = new List<decimal> { 1000m, 1500m, 2000m };
var temperatures = new List<decimal> { 560m, 570m, 580m };
decimal specificGravity = 0.65m;

AverageGasProperties avgProps = AveragePropertiesCalculator.CalculatePressureWeightedAverage(
    pressures,
    temperatures,
    specificGravity,
    ZFactorCalculator.CalculateBrillBeggs);

Console.WriteLine($"Average Z-factor: {avgProps.AverageZFactor:F4}");
```

### Gas Composition

```csharp
using Beep.OilandGas.GasProperties.Models;
using Beep.OilandGas.GasProperties.Calculations;

// Define gas composition
var composition = new GasComposition
{
    MethaneFraction = 0.85m,
    EthaneFraction = 0.10m,
    PropaneFraction = 0.03m,
    NitrogenFraction = 0.02m
};

// Validate composition
if (!composition.IsValid())
    throw new InvalidGasCompositionException("Composition fractions must sum to 1.0");

// Calculate pseudo-critical properties
var (pseudoCriticalPressure, pseudoCriticalTemperature) = 
    ZFactorCalculator.CalculatePseudoCriticalProperties(composition);

Console.WriteLine($"Pseudo-critical pressure: {pseudoCriticalPressure:F2} psia");
Console.WriteLine($"Pseudo-critical temperature: {pseudoCriticalTemperature:F2} °R");
```

## Units

### Pressure
- **Input/Output:** psia (pounds per square inch absolute)

### Temperature
- **Input/Output:** Rankine (°R)
- **Conversion:** °F + 459.67 = °R, °C + 491.67 = °R

### Viscosity
- **Output:** centipoise (cp)

### Specific Gravity
- **Input:** Dimensionless (relative to air = 1.0)

### Z-Factor
- **Output:** Dimensionless

## Validation

All calculation methods include parameter validation:

```csharp
try
{
    GasPropertiesValidator.ValidateCalculationParameters(
        pressure, temperature, specificGravity);
    
    // Perform calculations
}
catch (ParameterOutOfRangeException ex)
{
    Console.WriteLine($"Invalid parameter: {ex.ParameterName}");
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Constants

Common constants are available:

```csharp
using Beep.OilandGas.GasProperties.Constants;

decimal standardPressure = GasPropertiesConstants.StandardPressure; // 14.696 psia
decimal standardTemperature = GasPropertiesConstants.StandardTemperature; // 519.67 °R (60°F)
decimal universalGasConstant = GasPropertiesConstants.UniversalGasConstant; // 10.7316 psia·ft³/(lbmol·°R)
```

## Error Handling

The library provides specific exceptions:

- `GasPropertiesException` - Base exception
- `InvalidGasCompositionException` - Invalid gas composition
- `ParameterOutOfRangeException` - Parameter out of valid range
- `CalculationConvergenceException` - Calculation failed to converge

## Best Practices

1. **Always validate inputs** before calculations
2. **Use appropriate correlation** for your application
3. **Check Z-factor range** (typically 0.1 to 2.0)
4. **Validate gas composition** sums to 1.0
5. **Use consistent units** throughout calculations

## References

- Brill-Beggs Z-factor correlation
- Hall-Yarborough Z-factor correlation
- Carr-Kobayashi-Burrows gas viscosity correlation
- Lee-Gonzalez-Eakin gas viscosity correlation
- Standing-Katz Z-factor chart
- Dranchuk-Abu-Kassem correlation

## License

MIT License - See LICENSE file for details.

## Contributing

Contributions are welcome! Please follow the project's coding standards and include tests for new features.

---

**Status:** Production Ready ✅

