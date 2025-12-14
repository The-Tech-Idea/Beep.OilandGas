# Beep.PumpPerformance - Pump Performance Calculation Library

A comprehensive .NET library for pump performance calculations in oil and gas operations, including H-Q (Head-Quantity) curves, efficiency calculations, power analysis, system curves, affinity laws, and NPSH (Net Positive Suction Head) calculations.

## Features

### Core Functionality
- **H-Q Curve Calculations**: Generate and analyze Head-Quantity performance curves
- **Efficiency Calculations**: Overall, hydraulic, mechanical, and volumetric efficiency
- **Power Calculations**: Brake horsepower, motor power, energy consumption
- **System Curve Analysis**: System resistance curves and operating point determination
- **Affinity Laws**: Performance prediction at different speeds and impeller diameters
- **NPSH Calculations**: Net Positive Suction Head and cavitation analysis

### Pump Type Support
- **Centrifugal Pumps**: Multi-stage, specific speed, impeller design, performance degradation
- **Positive Displacement Pumps**: Reciprocating, rotary, slip calculations, pulsation analysis
- **Submersible Pumps (ESP)**: Stage count optimization, motor sizing, production optimization
- **Jet Pumps**: Nozzle/throat sizing, power fluid requirements, efficiency calculations

### Code Quality
- Comprehensive input validation
- Custom exception handling
- XML documentation for all APIs
- Industry-standard formulas and constants
- Backward compatible with existing code

## Quick Start

### Basic H-Q Calculation

```csharp
using Beep.PumpPerformance;
using Beep.PumpPerformance.Calculations;

// Flow rates in GPM, heads in feet, power in horsepower
double[] flowRates = { 100, 200, 300, 400, 500 };
double[] heads = { 100, 90, 75, 60, 45 };
double[] powers = { 80, 150, 230, 310, 380 };

// Calculate efficiency
double[] efficiencies = PumpPerformanceCalc.HQCalc(flowRates, heads, powers);

// Or use the enhanced calculations
var curve = HeadQuantityCalculations.GenerateHQCurve(flowRates, heads, powers);
var bep = HeadQuantityCalculations.FindBestEfficiencyPoint(curve);
```

### Efficiency Calculations

```csharp
// Calculate overall efficiency
double efficiency = EfficiencyCalculations.CalculateOverallEfficiency(
    flowRate: 300, 
    head: 75, 
    brakeHorsepower: 230, 
    specificGravity: 1.0);

// Calculate efficiency array
double[] efficiencies = EfficiencyCalculations.CalculateOverallEfficiency(
    flowRates, heads, powers);

// Find Best Efficiency Point
var (flowRate, eff) = EfficiencyCalculations.FindBestEfficiencyPoint(
    flowRates, efficiencies);
```

### Power Calculations

```csharp
// Calculate brake horsepower
double bhp = PowerCalculations.CalculateBrakeHorsepower(
    flowRate: 300, 
    head: 75, 
    efficiency: 0.75);

// Calculate motor input power
double motorPower = PowerCalculations.CalculateMotorInputPower(
    bhp, motorEfficiency: 0.90);

// Calculate power consumption in kW
double powerKW = PowerCalculations.CalculatePowerConsumption(
    flowRate: 300, 
    head: 75, 
    pumpEfficiency: 0.75, 
    motorEfficiency: 0.90);
```

### System Curve Analysis

```csharp
// Calculate system resistance curve
var systemCurve = SystemCurveCalculations.CalculateSystemCurve(
    staticHead: 50, 
    systemResistanceCoefficient: 0.001, 
    flowRates);

// Find operating point (intersection of pump and system curves)
var operatingPoint = SystemCurveCalculations.FindOperatingPoint(
    pumpCurve, systemCurve);

if (operatingPoint.HasValue)
{
    Console.WriteLine($"Operating Point: {operatingPoint.Value.flowRate} GPM " +
                     $"at {operatingPoint.Value.head} ft");
}
```

### Affinity Laws

```csharp
// Calculate performance at new speed
double newFlowRate = AffinityLaws.CalculateFlowRateAtNewSpeed(
    originalFlowRate: 300, 
    originalSpeed: 1750, 
    newSpeed: 2000);

double newHead = AffinityLaws.CalculateHeadAtNewSpeed(
    originalHead: 75, 
    originalSpeed: 1750, 
    newSpeed: 2000);

double newPower = AffinityLaws.CalculatePowerAtNewSpeed(
    originalPower: 230, 
    originalSpeed: 1750, 
    newSpeed: 2000);

// Scale entire curve to new speed
var scaledCurve = AffinityLaws.ScaleCurveToNewSpeed(
    originalCurve, 1750, 2000);
```

### NPSH Calculations

```csharp
// Calculate NPSH Available
double npsha = NPSHCalculations.CalculateNPSHAvailable(
    atmosphericPressure: 14.696,
    suctionPressure: 5.0,
    vaporPressure: 0.256,
    suctionLift: 10.0,
    frictionLoss: 2.0,
    specificGravity: 1.0);

// Calculate NPSH Required (approximation)
double npshr = NPSHCalculations.CalculateNPSHRequired(
    flowRate: 300, 
    speed: 1750, 
    suctionSpecificSpeed: 8500);

// Check for cavitation
bool cavitationLikely = NPSHCalculations.IsCavitationLikely(
    npsha, npshr, safetyMargin: 2.0);

// Calculate maximum allowable suction lift
double maxLift = NPSHCalculations.CalculateMaxAllowableSuctionLift(
    npshr: 15.0, 
    frictionLoss: 2.0, 
    safetyMargin: 2.0);
```

### C-Factor Calculation

```csharp
// Calculate C-Factor and generate performance data
double motorInputPower = 200; // HP
double[] flowRates = { 100, 200, 300, 400, 500 };
double[] heads = { 100, 90, 75, 60, 45 };

var results = PumpPerformanceCalc.CFactorCalc(
    motorInputPower, flowRates, heads);

foreach (var result in results)
{
    Console.WriteLine($"Head: {result.PumpHead} ft, Power: {result.PumpPower} HP");
}
```

### Multi-Pump Configurations

```csharp
using Beep.PumpPerformance.SystemAnalysis;

// Create pump curves
var pump1Curve = HeadQuantityCalculations.GenerateHQCurve(flowRates1, heads1, powers1);
var pump2Curve = HeadQuantityCalculations.GenerateHQCurve(flowRates2, heads2, powers2);
var pumpCurves = new List<List<HeadQuantityPoint>> { pump1Curve, pump2Curve };

// Calculate series configuration
var seriesResult = MultiPumpConfiguration.CalculateSeriesConfiguration(
    pumpCurves, systemCurve);

// Calculate parallel configuration
var parallelResult = MultiPumpConfiguration.CalculateParallelConfiguration(
    pumpCurves, systemCurve);

// Compare configurations
var (series, parallel) = MultiPumpConfiguration.CompareConfigurations(
    pumpCurves, systemCurve);
```

### Pump Selection

```csharp
using Beep.PumpPerformance.SystemAnalysis;

// Create pump candidates
var candidates = new List<PumpCandidate>
{
    new PumpCandidate
    {
        PumpId = "Pump A",
        Curve = curve1,
        Cost = 10000,
        BestEfficiency = 0.75
    }
};

// Select best pump
var selection = PumpSelection.SelectBestPump(candidates, systemCurve, preferEfficiency: true);

// Find suitable pumps
var suitablePumps = PumpSelection.FindSuitablePumps(
    candidates, requiredFlowRate: 300, requiredHead: 75);
```

## API Reference

### Main Classes

**Core Calculations:**
- **`PumpPerformanceCalc`**: Main API class with `HQCalc` and `CFactorCalc` methods
- **`EfficiencyCalculations`**: Comprehensive efficiency calculations
- **`PowerCalculations`**: Power and energy calculations
- **`HeadQuantityCalculations`**: H-Q curve generation and analysis
- **`SystemCurveCalculations`**: System resistance curve analysis
- **`AffinityLaws`**: Affinity laws for speed and diameter changes
- **`NPSHCalculations`**: NPSH and cavitation analysis

**Pump Type Classes:**
- **`CentrifugalPump`**: Centrifugal pump specialized calculations
- **`PositiveDisplacementPump`**: Positive displacement pump calculations
- **`SubmersiblePump`**: ESP (Electric Submersible Pump) calculations
- **`JetPump`**: Jet pump performance and sizing

**System Analysis:**
- **`MultiPumpConfiguration`**: Series and parallel pump configurations
- **`PumpSelection`**: Pump selection and optimization algorithms

### Validation

All methods include comprehensive input validation. Invalid inputs will throw `InvalidInputException` with descriptive error messages.

### Units

- **Flow Rate**: Gallons per minute (GPM)
- **Head**: Feet
- **Power**: Horsepower (HP) or Kilowatts (kW)
- **Pressure**: Pounds per square inch absolute (psia) or gauge (psig)
- **Speed**: Revolutions per minute (RPM)
- **Diameter**: Inches

## Requirements

- .NET 6.0 or later
- No external dependencies (pure .NET implementation)

## Documentation

- **COMPLETE_IMPLEMENTATION_SUMMARY.md**: Complete feature overview
- **PHASE1_2_SUMMARY.md**: Phase 1 & 2 implementation details
- **PHASE3_SUMMARY.md**: Phase 3 (Pump Types) implementation
- **PHASE4_SYSTEM_ANALYSIS_SUMMARY.md**: Phase 4 (System Analysis) implementation
- **ENHANCEMENT_PLAN.md**: Original enhancement roadmap

## License

MIT License

## Contributing

Contributions are welcome! Please ensure all code follows the existing style and includes appropriate XML documentation.

