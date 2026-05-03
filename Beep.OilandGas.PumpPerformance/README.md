# Beep.OilandGas.PumpPerformance — pump performance library

A .NET library for pump performance screening in oil and gas: H–Q (head–quantity) curves, efficiency and power, system curves, affinity laws, NPSH, viscosity corrections, ESP design helpers, pump-type abstractions, and optional **SkiaSharp** curve rendering.

**Planning:** [`.plans/README.md`](.plans/README.md) · **Tracker:** [`MASTER-TODO-TRACKER.md`](MASTER-TODO-TRACKER.md)

## Features

### Core functionality

- **H–Q curve calculations**: generate and analyze head–quantity performance curves
- **Efficiency calculations**: overall efficiency from Q, H, BHP, SG
- **Power calculations**: brake horsepower, motor input, kW
- **System curve analysis**: system resistance and operating point (where implemented)
- **Affinity laws**: performance at different speeds / impeller scaling
- **NPSH calculations**: NPSHa / NPSHr approximations and cavitation screening

### Pump type support

- **Centrifugal**, **positive displacement**, **submersible (ESP)**, **jet** — specialized calculators under `PumpTypes/`

### Code quality

- Input validation and custom exceptions
- XML documentation on public APIs
- US field units documented on public methods (GPM, ft, HP unless noted)

## Quick start

### Basic H–Q / efficiency

```csharp
using Beep.OilandGas.PumpPerformance;
using Beep.OilandGas.PumpPerformance.Calculations;

// Flow rates in GPM, heads in feet, power in brake horsepower
double[] flowRates = { 100, 200, 300, 400, 500 };
double[] heads = { 100, 90, 75, 60, 45 };
double[] powers = { 80, 150, 230, 310, 380 };

double[] efficiencies = PumpPerformanceCalc.HQCalc(flowRates, heads, powers);

var curve = HeadQuantityCalculations.GenerateHQCurve(flowRates, heads, powers);
var bep = HeadQuantityCalculations.FindBestEfficiencyPoint(curve);
```

### Efficiency and power

```csharp
double efficiency = EfficiencyCalculations.CalculateOverallEfficiency(
    flowRate: 300,
    head: 75,
    brakeHorsepower: 230,
    specificGravity: 1.0);

double bhp = PowerCalculations.CalculateBrakeHorsepower(
    flowRate: 300,
    head: 75,
    specificGravity: 1.0,
    efficiency: 0.75);
```

### Affinity laws

```csharp
double newFlow = AffinityLaws.CalculateFlowRateAtNewSpeed(300, 1750, 2000);
double newHead = AffinityLaws.CalculateHeadAtNewSpeed(75, 1750, 2000);
double newPower = AffinityLaws.CalculatePowerAtNewSpeed(230, 1750, 2000);
```

### C-factor model (screening)

The **C-factor** here is the coefficient **C = P_motor / Q₀³** (HP / GPM³) used with **P = C·Q³** and parabolic **H** scaling along a synthetic curve — see `PumpPerformanceCalc.CFactorCalc`. It is **not** an IPR productivity index.

```csharp
double motorInputPower = 200;
double[] flowRates = { 100, 200, 300, 400, 500 };
double[] heads = { 100, 90, 75, 60, 45 };

var results = PumpPerformanceCalc.CFactorCalc(motorInputPower, flowRates, heads);
```

### System analysis

```csharp
using Beep.OilandGas.PumpPerformance.SystemAnalysis;

// MultiPumpConfiguration, PumpSelection — see XML docs on those types
```

### Optional service façade

`PumpPerformanceService` delegates curve math to **`Calculations`** and uses **`PumpDataValidator`** for domain checks (including **`ValidateNpshMargin`** for NPSHa vs NPSHr screening). Expect **`InvalidInputException`** from invalid numeric domains in addition to **`ArgumentException`** for null IDs. **`GeneratePerformanceCurveAsync`** accepts **`specificGravity`** (default 1.0) and stores it on **`PumpPerformanceCurve.SpecificGravity`**. NPSH cavitation screening defaults use **`PumpConstants.DefaultNpshSafetyMarginFeet`** (2 ft) in **`NPSHCalculations.IsCavitationLikely`** / **`CalculateMaxAllowableSuctionLift`**.

```csharp
using Beep.OilandGas.PumpPerformance.Services;

var service = new PumpPerformanceService(logger: null);
double c = await service.CalculateCFactorAsync(200, 100, 100);
```

## Requirements

- **.NET 10** (`net10.0`)
- **SkiaSharp** (+ Extended, Svg) for `Rendering/`
- **Project references:** `Beep.OilandGas.PPDM.Models`, `Beep.OilandGas.GasProperties` (ESP / gas calculations in `ESPDesignCalculator`)

## Units (default public APIs)

- **Conversion factors:** `PumpConstants.HorsepowerConversionFactor` (GPM·ft·SG → BHP) and `PumpConstants.HorsepowerFromGpmPsiFactor` (GPM·psi → BHP for PD-style screening)
- **Flow rate:** US gallons per minute (GPM); conversion helpers in `PumpConstants` (e.g. BPD, m³/h)
- **Head:** feet
- **Power:** horsepower (HP) unless a method states kW
- **Speed:** RPM

## ESP and shared models

ESP table-style and design DTOs live in **`Beep.OilandGas.Models`** under `Data/PumpPerformance`. This library references **`Beep.OilandGas.GasProperties`** for gas calculations used in ESP design paths.

## Related domains (do not duplicate)

- **Hydraulic jet / piston lift** — `Beep.OilandGas.HydraulicPumps`
- **Sucker rod / PCP** — `Beep.OilandGas.SuckerRodPumping`
- **Well / intake–outflow** — `Beep.OilandGas.NodalAnalysis`

## Documentation in this folder

- **`ENHANCEMENT_PLAN.md`** — historical roadmap (namespaces in older docs may still show legacy titles; code uses **`Beep.OilandGas.PumpPerformance`**)
- **`ESP_DESIGN_IMPLEMENTATION.md`**, **`PHASE*.md`**, **`COMPLETE_IMPLEMENTATION_SUMMARY.md`** — implementation notes
- **`Rendering/RENDERING_IMPLEMENTATION.md`** — Skia rendering

## License

MIT License
