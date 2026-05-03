# Beep.OilandGas.ChokeAnalysis

A comprehensive library for gas choke flow calculations in oil and gas engineering applications.

## Overview

This library provides industry-standard methods for calculating gas flow through chokes, including downhole and uphole choke analysis, flow rate calculations, and choke sizing.

## Features

### Choke Flow Calculations

- **Downhole Choke Flow** - Gas flow through downhole chokes
- **Uphole Choke Flow** - Gas flow through uphole chokes
- **Sonic Flow** - Critical (sonic) flow calculations
- **Subsonic Flow** - Subcritical flow calculations
- **Choke Sizing** - Calculate required choke size for desired flow rate
- **Pressure Calculations** - Calculate downstream pressure for given flow rate

### Key Capabilities

- Flow rate calculations
- Pressure drop calculations
- Flow regime determination (sonic/subsonic)
- Choke sizing
- Integration with gas properties

## Installation

```bash
dotnet add package Beep.OilandGas.ChokeAnalysis
```

## Testing

```bash
dotnet test Beep.OilandGas.ChokeAnalysis.Tests/Beep.OilandGas.ChokeAnalysis.Tests.csproj
```

`GasChokeCalculator` uses **`CHOKE_PROPERTIES.CHOKE_AREA`** (in²). If **`CHOKE_AREA`** is zero or unset, it is **derived from `CHOKE_DIAMETER`** as \(A = \pi D^2 / 4\) (same as `ChokeAreaSquareInchesFromDiameter`).

## Quick Start

### Calculate Downhole Choke Flow

```csharp
using Beep.OilandGas.ChokeAnalysis.Calculations;
using Beep.OilandGas.ChokeAnalysis.Validation;
using Beep.OilandGas.Models.Data.ChokeAnalysis;

var choke = new CHOKE_PROPERTIES
{
    CHOKE_DIAMETER = 0.5m,
    CHOKE_TYPE = "BEAN",
    DISCHARGE_COEFFICIENT = 0.85m
};

var gasProperties = new GAS_CHOKE_PROPERTIES
{
    UPSTREAM_PRESSURE = 2000m,
    DOWNSTREAM_PRESSURE = 500m,
    TEMPERATURE = 580m,
    GAS_SPECIFIC_GRAVITY = 0.65m,
    Z_FACTOR = 0.92m
};

ChokeValidator.ValidateCalculationParameters(choke, gasProperties);

var result = GasChokeCalculator.CalculateDownholeChokeFlow(choke, gasProperties);

Console.WriteLine($"Flow Rate: {result.FLOW_RATE:F2} Mscf/day");
Console.WriteLine($"Flow Regime: {result.FLOW_REGIME}");
Console.WriteLine($"Pressure ratio: {result.PRESSURE_RATIO:F4}");
```

For gas choke results, `FLOW_REGIME` stores reference codes (`SONIC`, `SUBSONIC`) aligned with `R_CHOKE_ANALYSIS_REFERENCE_CODE`.

### Calculate Required Choke Size

```csharp
// Calculate required choke size for desired flow rate
decimal requiredChokeSize = GasChokeCalculator.CalculateRequiredChokeSize(
    gasProperties,
    flowRate: 5000m); // Mscf/day

Console.WriteLine($"Required Choke Size: {requiredChokeSize:F3} inches");
```

### Calculate Downstream Pressure

```csharp
// Calculate downstream pressure for given flow rate
decimal downstreamPressure = GasChokeCalculator.CalculateDownstreamPressure(
    choke,
    gasProperties,
    flowRate: 3000m); // Mscf/day

Console.WriteLine($"Downstream Pressure: {downstreamPressure:F2} psia");
```

## Flow Regimes

### Sonic (Critical) Flow

Occurs when the pressure ratio (P2/P1) is less than the critical pressure ratio. In sonic flow:
- Flow rate is independent of downstream pressure
- Maximum flow rate for given upstream conditions
- Choked flow condition

### Subsonic Flow

Occurs when the pressure ratio is greater than the critical pressure ratio. In subsonic flow:
- Flow rate depends on both upstream and downstream pressures
- Flow rate increases as downstream pressure decreases

## Units

### Pressure
- **Input/Output:** psia (pounds per square inch absolute)

### Flow Rate
- **Output:** Mscf/day (thousand standard cubic feet per day)

### Choke Diameter
- **Input/Output:** inches

### Temperature
- **Input:** Rankine (°R)

### Gas Specific Gravity
- **Input:** Dimensionless (relative to air = 1.0)

## Validation

All calculation methods include parameter validation:

```csharp
try
{
    ChokeValidator.ValidateCalculationParameters(choke, gasProperties);
    
    // Perform calculations
}
catch (InvalidChokePropertiesException ex)
{
    Console.WriteLine($"Invalid choke: {ex.Message}");
}
catch (ChokeParameterOutOfRangeException ex)
{
    Console.WriteLine($"Invalid parameter {ex.ParameterName}: {ex.Message}");
}
```

## Integration

### With Beep.OilandGas.Properties

- ✅ Z-factor calculations (Brill-Beggs)
- ✅ Gas property support
- ✅ Temperature and pressure handling

### With Other Projects

- ✅ Works with `Beep.NodalAnalysis` for well analysis
- ✅ Compatible with `Beep.OilandGas.ProductionForecasting`
- ✅ Can integrate with production systems

## Best Practices

1. **Validate inputs** before calculations
2. **Check flow regime** to understand flow behavior
3. **Use appropriate discharge coefficient** for choke type
4. **Account for Z-factor** for accurate calculations
5. **Consider temperature effects** on gas properties

## Error Handling

The library provides specific exceptions:

- `ChokeException` - Base exception
- `InvalidChokePropertiesException` - Invalid choke data
- `ChokeParameterOutOfRangeException` - Parameter validation
- `ChokeConvergenceException` - Calculation convergence failures

## References

- Isentropic flow equations
- Critical flow theory
- Gas flow through restrictions
- Choke performance curves

## License

MIT License - See LICENSE file for details.

## Contributing

Contributions are welcome! Please follow the project's coding standards and include tests for new features.

---

**Status:** Production Ready ✅

