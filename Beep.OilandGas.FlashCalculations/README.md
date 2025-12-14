# Beep.OilandGas.FlashCalculations

A comprehensive library for phase equilibrium and flash calculations in oil and gas operations.

## Overview

`Beep.OilandGas.FlashCalculations` provides calculations for:
- Isothermal flash calculations (pressure and temperature specified)
- Phase equilibrium calculations
- K-value calculations
- Phase composition calculations
- Phase property calculations

## Features

### ✅ Flash Calculations
- Isothermal flash (P-T flash)
- Rachford-Rice equation solver
- Wilson correlation for K-values
- Newton-Raphson iteration
- Convergence monitoring

### ✅ Phase Equilibrium
- Vapor-liquid equilibrium
- K-value calculations
- Phase composition calculations
- Phase property calculations

### ✅ Integration
- Integration with `Beep.OilandGas.Properties` for gas calculations
- Z-factor calculations
- Gas property support

## Installation

```bash
dotnet add package Beep.OilandGas.FlashCalculations
```

## Usage Examples

### Basic Flash Calculation

```csharp
using Beep.OilandGas.FlashCalculations.Models;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.FlashCalculations.Validation;

// Define components
var methane = new Component
{
    Name = "Methane",
    MoleFraction = 0.5m,
    CriticalTemperature = 343.0m, // Rankine
    CriticalPressure = 667.8m, // psia
    AcentricFactor = 0.008m,
    MolecularWeight = 16.04m
};

var ethane = new Component
{
    Name = "Ethane",
    MoleFraction = 0.3m,
    CriticalTemperature = 549.6m, // Rankine
    CriticalPressure = 707.8m, // psia
    AcentricFactor = 0.098m,
    MolecularWeight = 30.07m
};

var propane = new Component
{
    Name = "Propane",
    MoleFraction = 0.2m,
    CriticalTemperature = 665.7m, // Rankine
    CriticalPressure = 616.3m, // psia
    AcentricFactor = 0.152m,
    MolecularWeight = 44.10m
};

// Define flash conditions
var conditions = new FlashConditions
{
    Pressure = 500m, // psia
    Temperature = 540m, // Rankine
    FeedComposition = new List<Component> { methane, ethane, propane }
};

// Validate conditions
FlashValidator.ValidateFlashConditions(conditions);

// Perform flash calculation
var flashResult = FlashCalculator.PerformIsothermalFlash(conditions);

// Access results
Console.WriteLine($"Vapor Fraction: {flashResult.VaporFraction:F4}");
Console.WriteLine($"Liquid Fraction: {flashResult.LiquidFraction:F4}");
Console.WriteLine($"Converged: {flashResult.Converged}");
Console.WriteLine($"Iterations: {flashResult.Iterations}");

// Display phase compositions
Console.WriteLine("\nVapor Composition:");
foreach (var kvp in flashResult.VaporComposition)
{
    Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
}

Console.WriteLine("\nLiquid Composition:");
foreach (var kvp in flashResult.LiquidComposition)
{
    Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
}

Console.WriteLine("\nK-Values:");
foreach (var kvp in flashResult.KValues)
{
    Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
}
```

### Phase Property Calculations

```csharp
// Calculate vapor phase properties
var vaporProperties = FlashCalculator.CalculateVaporProperties(flashResult, conditions);

Console.WriteLine($"Vapor Molecular Weight: {vaporProperties.MolecularWeight:F2} lb/lbmol");
Console.WriteLine($"Vapor Density: {vaporProperties.Density:F4} lb/ft³");
Console.WriteLine($"Vapor Specific Gravity: {vaporProperties.SpecificGravity:F4}");

// Calculate liquid phase properties
var liquidProperties = FlashCalculator.CalculateLiquidProperties(flashResult, conditions);

Console.WriteLine($"Liquid Molecular Weight: {liquidProperties.MolecularWeight:F2} lb/lbmol");
Console.WriteLine($"Liquid Density: {liquidProperties.Density:F4} lb/ft³");
Console.WriteLine($"Liquid Specific Gravity: {liquidProperties.SpecificGravity:F4}");
```

### Multi-Component Mixture

```csharp
// Create a more complex mixture
var components = new List<Component>
{
    new Component { Name = "C1", MoleFraction = 0.4m, CriticalTemperature = 343.0m, 
                    CriticalPressure = 667.8m, AcentricFactor = 0.008m, MolecularWeight = 16.04m },
    new Component { Name = "C2", MoleFraction = 0.25m, CriticalTemperature = 549.6m, 
                    CriticalPressure = 707.8m, AcentricFactor = 0.098m, MolecularWeight = 30.07m },
    new Component { Name = "C3", MoleFraction = 0.15m, CriticalTemperature = 665.7m, 
                    CriticalPressure = 616.3m, AcentricFactor = 0.152m, MolecularWeight = 44.10m },
    new Component { Name = "iC4", MoleFraction = 0.1m, CriticalTemperature = 734.1m, 
                    CriticalPressure = 527.9m, AcentricFactor = 0.176m, MolecularWeight = 58.12m },
    new Component { Name = "nC4", MoleFraction = 0.1m, CriticalTemperature = 765.3m, 
                    CriticalPressure = 550.7m, AcentricFactor = 0.193m, MolecularWeight = 58.12m }
};

var flashConditions = new FlashConditions
{
    Pressure = 1000m, // psia
    Temperature = 560m, // Rankine
    FeedComposition = components
};

var result = FlashCalculator.PerformIsothermalFlash(flashConditions);

// Validate result
FlashValidator.ValidateFlashResult(result);
```

## API Reference

### Models

- `Component` - Component properties
- `FlashConditions` - Flash calculation conditions
- `FlashResult` - Flash calculation results
- `PhaseProperties` - Phase property results

### Calculations

- `FlashCalculator` - Flash calculation methods
  - `PerformIsothermalFlash` - Isothermal flash calculation
  - `CalculateVaporProperties` - Vapor phase properties
  - `CalculateLiquidProperties` - Liquid phase properties

### Validation

- `FlashValidator` - Input validation

### Constants

- `FlashConstants` - Standard values, convergence parameters

### Exceptions

- `FlashException` - Base exception
- `InvalidFlashConditionsException` - Invalid flash conditions
- `InvalidComponentException` - Invalid component properties
- `FlashConvergenceException` - Convergence failure

## Algorithm Details

### Rachford-Rice Equation

The Rachford-Rice equation is solved using Newton-Raphson iteration:

```
Σ(zi * (Ki - 1) / (1 + V * (Ki - 1))) = 0
```

Where:
- V = vapor fraction
- zi = feed mole fraction of component i
- Ki = K-value (equilibrium ratio) of component i

### K-Value Initialization

K-values are initialized using the Wilson correlation:

```
Ki = (Pci/P) * exp(5.37 * (1 + ωi) * (1 - Tci/T))
```

Where:
- Pci = critical pressure of component i
- Tci = critical temperature of component i
- ωi = acentric factor of component i
- P = system pressure
- T = system temperature

## Dependencies

- `Beep.OilandGas.Properties` - Gas property calculations
- `SkiaSharp` - Graphics rendering (for future visualization)

## Source Files

Based on Petroleum Engineer XLS files:
- `LP - Flash.xls`

## License

MIT License

## Contributing

Contributions are welcome! Please follow the project's coding standards and submit pull requests.

