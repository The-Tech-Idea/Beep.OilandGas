# Beep.NodalAnalysis - IPR/VLP Analysis Library

A comprehensive .NET library for Inflow Performance Relationship (IPR) and Vertical Lift Performance (VLP) analysis for production system optimization.

## Features

### Core Functionality
- **IPR Curves**: Generate inflow performance relationships
- **VLP Curves**: Vertical lift performance calculations
- **Nodal Analysis**: System optimization at operating point
- **Gas Lift Optimization**: Gas lift design and optimization
- **ESP Optimization**: Electric submersible pump optimization
- **Choke Performance**: Choke sizing and performance
- **Tubing Optimization**: Optimal tubing size selection
- **Production Prediction**: Rate prediction at different conditions

### IPR Methods
- **Vogel Method**: Solution gas drive reservoirs
- **Fetkovich Method**: Multi-point IPR
- **Wiggins Method**: Three-phase IPR
- **Composite IPR**: Layered reservoir IPR
- **Gas Well IPR**: Backpressure equation

### VLP Methods
- **Hagedorn-Brown**: Multiphase flow correlation
- **Beggs-Brill**: Multiphase flow correlation
- **Duns-Ros**: Multiphase flow correlation
- **Orkiszewski**: Multiphase flow correlation
- **Aziz-Govier-Fogarasi**: Multiphase flow correlation

### Visualization
- SkiaSharp-based rendering
- Interactive IPR/VLP plots
- Operating point visualization
- Sensitivity analysis plots
- Export capabilities

## Quick Start

```csharp
using Beep.NodalAnalysis;
using Beep.NodalAnalysis.Calculations;

// Define reservoir properties
var reservoir = new ReservoirProperties
{
    ReservoirPressure = 3000, // psi
    BubblePointPressure = 2500, // psi
    ProductivityIndex = 2.5, // BPD/psi
    WaterCut = 0.1 // fraction
};

// Generate IPR curve
var iprCurve = IPRCalculator.GenerateVogelIPR(reservoir, maxFlowRate: 5000);

// Define wellbore properties
var wellbore = new WellboreProperties
{
    TubingDiameter = 2.875, // inches
    TubingLength = 8000, // feet
    WellheadPressure = 500, // psi
    WaterCut = 0.1,
    GasOilRatio = 500 // SCF/STB
};

// Generate VLP curve
var vlpCurve = VLPCalculator.GenerateVLP(wellbore, flowRates: iprCurve.Select(p => p.FlowRate).ToArray());

// Perform nodal analysis
var operatingPoint = NodalAnalyzer.FindOperatingPoint(iprCurve, vlpCurve);
Console.WriteLine($"Operating Flow Rate: {operatingPoint.FlowRate} BPD");
Console.WriteLine($"Operating Pressure: {operatingPoint.Pressure} psi");
```

## Installation

```bash
dotnet add package Beep.NodalAnalysis
```

## Documentation

See the [API Documentation](API.md) for detailed information.

## License

MIT License

