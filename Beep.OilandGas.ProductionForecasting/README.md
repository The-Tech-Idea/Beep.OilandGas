# Beep.OilandGas.ProductionForecasting

A comprehensive library for production forecasting in oil and gas engineering applications.

## Overview

This library provides industry-standard methods for forecasting production rates and cumulative production based on reservoir properties and operating conditions.

## Features

### Production Forecasting Methods

- **Pseudo-Steady State (Single-Phase)** - For oil wells above bubble point
- **Pseudo-Steady State (Two-Phase)** - For oil wells below bubble point
- **Transient Flow** - Early-time production forecasting
- **Gas Well Forecasting** - Specialized gas well production forecasting

### Key Capabilities

- Production rate forecasting
- Cumulative production forecasting
- Reservoir pressure decline
- Multiple forecast types
- Integration with gas properties

## Installation

```bash
dotnet add package Beep.OilandGas.ProductionForecasting
```

## Quick Start

### Single-Phase Pseudo-Steady State Forecast

```csharp
using Beep.OilandGas.ProductionForecasting.Models;
using Beep.OilandGas.ProductionForecasting.Calculations;
using Beep.OilandGas.ProductionForecasting.Validation;

// Define reservoir properties
var reservoir = new ReservoirForecastProperties
{
    InitialPressure = 3000m, // psia
    Permeability = 100m, // md
    Thickness = 50m, // feet
    DrainageRadius = 1000m, // feet
    WellboreRadius = 0.25m, // feet
    FormationVolumeFactor = 1.2m, // RB/STB
    OilViscosity = 1.5m, // cp
    TotalCompressibility = 0.00001m, // 1/psi
    Porosity = 0.2m,
    SkinFactor = 0m,
    Temperature = 580m // Rankine
};

// Validate inputs
ForecastValidator.ValidateForecastParameters(
    reservoir, bottomHolePressure: 1500m, forecastDuration: 365m, timeSteps: 100);

// Generate forecast
var forecast = PseudoSteadyStateForecast.GenerateSinglePhaseForecast(
    reservoir,
    bottomHolePressure: 1500m,
    forecastDuration: 365m, // 1 year
    timeSteps: 100);

// Access results
Console.WriteLine($"Initial Rate: {forecast.InitialProductionRate:F2} bbl/day");
Console.WriteLine($"Final Rate: {forecast.FinalProductionRate:F2} bbl/day");
Console.WriteLine($"Total Cumulative: {forecast.TotalCumulativeProduction:F2} bbl");
```

### Two-Phase Forecast

```csharp
var forecast = PseudoSteadyStateForecast.GenerateTwoPhaseForecast(
    reservoir,
    bottomHolePressure: 1500m,
    bubblePointPressure: 2000m,
    forecastDuration: 365m,
    timeSteps: 100);
```

### Transient Forecast

```csharp
var forecast = TransientForecast.GenerateTransientForecast(
    reservoir,
    bottomHolePressure: 1500m,
    forecastDuration: 365m,
    timeSteps: 100);
```

### Gas Well Forecast

```csharp
var reservoir = new ReservoirForecastProperties
{
    // ... same properties as above ...
    GasSpecificGravity = 0.65m
};

var forecast = GasWellForecast.GenerateGasWellForecast(
    reservoir,
    bottomHolePressure: 1000m,
    forecastDuration: 365m,
    timeSteps: 100);

// Gas rates are in Mscf/day
Console.WriteLine($"Initial Rate: {forecast.InitialProductionRate:F2} Mscf/day");
```

## Forecast Types

### Pseudo-Steady State (Single-Phase)

- **Use Case:** Oil wells producing above bubble point pressure
- **Method:** Material balance with constant productivity index
- **Output:** Production rate and cumulative production over time

### Pseudo-Steady State (Two-Phase)

- **Use Case:** Oil wells producing below bubble point pressure
- **Method:** Material balance with Vogel equation for two-phase flow
- **Output:** Production rate and cumulative production accounting for gas evolution

### Transient Flow

- **Use Case:** Early-time production or new wells
- **Method:** Transient flow equations with exponential integral
- **Output:** Production rate during transient period

### Gas Well

- **Use Case:** Gas well production forecasting
- **Method:** Gas deliverability equations with Z-factor calculations
- **Output:** Gas production rate in Mscf/day

## Units

### Pressure
- **Input/Output:** psia (pounds per square inch absolute)

### Flow Rate
- **Oil:** bbl/day (barrels per day)
- **Gas:** Mscf/day (thousand standard cubic feet per day)

### Time
- **Input/Output:** days

### Reservoir Properties
- **Permeability:** md (millidarcies)
- **Thickness:** feet
- **Radius:** feet
- **Viscosity:** cp (centipoise)
- **Temperature:** Rankine (°R)

## Validation

All forecast methods include comprehensive validation:

```csharp
try
{
    ForecastValidator.ValidateForecastParameters(
        reservoir, bottomHolePressure, forecastDuration, timeSteps);
    
    // Generate forecast
}
catch (InvalidReservoirPropertiesException ex)
{
    Console.WriteLine($"Invalid reservoir: {ex.Message}");
}
catch (ForecastParameterOutOfRangeException ex)
{
    Console.WriteLine($"Invalid parameter {ex.ParameterName}: {ex.Message}");
}
```

## Integration

### With Beep.OilandGas.Properties

- ✅ Z-factor calculations for gas wells
- ✅ Gas property support
- ✅ Temperature and pressure conversions

### With Other Projects

- ✅ Works with `Beep.OilandGas.ProductionAccounting`
- ✅ Compatible with `Beep.NodalAnalysis`
- ✅ Can integrate with `Beep.DCA` for decline curve analysis

## Best Practices

1. **Validate inputs** before generating forecasts
2. **Choose appropriate forecast type** for your well
3. **Use sufficient time steps** for accuracy (100+ recommended)
4. **Consider bubble point** for oil wells
5. **Account for skin factor** in productivity calculations

## Error Handling

The library provides specific exceptions:

- `ForecastException` - Base exception
- `InvalidReservoirPropertiesException` - Invalid reservoir data
- `ForecastParameterOutOfRangeException` - Parameter validation
- `ForecastConvergenceException` - Calculation convergence failures

## References

- Pseudo-steady state flow equations
- Transient flow theory
- Gas well deliverability equations
- Material balance principles
- Vogel two-phase flow equation

## License

MIT License - See LICENSE file for details.

## Contributing

Contributions are welcome! Please follow the project's coding standards and include tests for new features.

---

**Status:** Production Ready ✅

