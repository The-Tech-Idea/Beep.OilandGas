# Beep.OilandGas.CompressorAnalysis — LifeCycle integration

## Overview

**Beep.OilandGas.CompressorAnalysis** supports centrifugal and reciprocating compressor power and pressure calculations. **Table-shaped** entities (`COMPRESSOR_*`, **`R_COMPRESSOR_ANALYSIS_REFERENCE_CODE`**) live in **`Beep.OilandGas.CompressorAnalysis.Data`**. **Cross-layer wire** types (**`CompressorAnalysisRequest`**, **`CompressorAnalysisResult`**, **`CompressorAnalysisWellKnown`**) live in **`Beep.OilandGas.Models.Data.Calculations`**.

### Key capabilities

- Centrifugal: polytropic head, power (**`ICompressorAnalysisService.CalculateCentrifugalPowerAsync`**).
- Reciprocating: displacement-based power (**`CalculateReciprocatingPowerAsync`**).
- Pressure: required discharge / constraints (**`CalculateRequiredPressureAsync`** and static **`CompressorPressureCalculator`** where used internally).

### Current status

Integrated for packaged facility runs: **`PPDMCalculationService.PerformCompressorAnalysisAsync`** → **`Beep.OilandGas.CompressorAnalysis.Core.Interfaces.ICompressorAnalysisService`**; HTTP **`POST /api/calculations/compressor`**; **`DataFlowService.RunCompressorAnalysisAsync`** / **`FacilityManagementService.AnalyzeCompressorAsync`**. **`CompressorController`** (`**/api/compressor/**`) uses the **same** **`ICompressorAnalysisService`** for raw **`COMPRESSOR_*`** payloads.

---

## Main types and entry points

### `ICompressorAnalysisService`

**Namespace:** **`Beep.OilandGas.CompressorAnalysis.Core.Interfaces`**

Orchestrates calculations on **`COMPRESSOR_*`** types from **`Beep.OilandGas.CompressorAnalysis.Data`**.

### Static calculators (library internals)

These remain useful for tests and direct calls; production HTTP and facility orchestration prefer **`ICompressorAnalysisService`**.

```csharp
// Beep.OilandGas.CompressorAnalysis.Calculations — uses Data/* table shapes
public static class CentrifugalCompressorCalculator
{
    public static COMPRESSOR_POWER_RESULT CalculatePower(
        CENTRIFUGAL_COMPRESSOR_PROPERTIES compressorProperties,
        bool useSIUnits = false);
}

public static class ReciprocatingCompressorCalculator
{
    public static COMPRESSOR_POWER_RESULT CalculatePower(
        RECIPROCATING_COMPRESSOR_PROPERTIES compressorProperties,
        bool useSIUnits = false);
}

public static class CompressorPressureCalculator
{
    public static COMPRESSOR_PRESSURE_RESULT CalculateRequiredPressure(
        COMPRESSOR_OPERATING_CONDITIONS operatingConditions,
        decimal requiredFlowRate,
        decimal maxPower,
        decimal compressorEfficiency);
}
```

---

## Integration with LifeCycle services

### `PPDMDevelopmentService`

**Namespace:** **`Beep.OilandGas.LifeCycle.Services.Development`**

Optional future persistence/analysis orchestration; facility packaged flows already use **`PerformCompressorAnalysisAsync`** and **`DataFlowService`**.

---

## Usage examples

### Example 1 — static calculator (tests / utilities)

```csharp
using Beep.OilandGas.CompressorAnalysis.Calculations;
using Beep.OilandGas.CompressorAnalysis.Data;

var operatingConditions = new COMPRESSOR_OPERATING_CONDITIONS
{
    COMPRESSOR_OPERATING_CONDITIONS_ID = Guid.NewGuid().ToString("N"),
    SUCTION_PRESSURE = 100m,
    DISCHARGE_PRESSURE = 500m,
    SUCTION_TEMPERATURE = 520m,
    DISCHARGE_TEMPERATURE = 600m,
    GAS_FLOW_RATE = 1000m,
    GAS_SPECIFIC_GRAVITY = 0.65m,
    GAS_MOLECULAR_WEIGHT = 18.8m,
    COMPRESSOR_EFFICIENCY = 0.75m,
    MECHANICAL_EFFICIENCY = 0.95m
};

var compressorProperties = new CENTRIFUGAL_COMPRESSOR_PROPERTIES
{
    CENTRIFUGAL_COMPRESSOR_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
    COMPRESSOR_OPERATING_CONDITIONS_ID = operatingConditions.COMPRESSOR_OPERATING_CONDITIONS_ID,
    OPERATING_CONDITIONS = operatingConditions,
    POLYTROPIC_EFFICIENCY = 0.75m,
    SPECIFIC_HEAT_RATIO = 1.3m,
    NUMBER_OF_STAGES = 1,
    SPEED = 3600m
};

COMPRESSOR_POWER_RESULT result = CentrifugalCompressorCalculator.CalculatePower(
    compressorProperties,
    useSIUnits: false);
```

### Example 2 — facility analysis via `DataFlowService`

```csharp
using Beep.OilandGas.LifeCycle.Services.Integration;
using Beep.OilandGas.Models.Data.Calculations;

var dataFlow = serviceProvider.GetRequiredService<DataFlowService>();

CompressorAnalysisResult result = await dataFlow.RunCompressorAnalysisAsync(
    facilityId: "FACILITY-001",
    userId: "user123",
    equipmentId: null,
    compressorType: CompressorAnalysisWellKnown.CompressorType.Centrifugal,
    analysisType: CompressorAnalysisWellKnown.AnalysisType.Power,
    additionalParameters: null);
```

Alternatively inject **`FacilityManagementService`** and call **`AnalyzeCompressorAsync`**.

---

## Data storage

### Extension tables

Owned by **`CompressorAnalysisModule`** — see **`Beep.OilandGas.CompressorAnalysis/Modules/CompressorAnalysisModule.cs`** for **`EntityTypes`** and reference seeding.

### Facility linkage

**`FACILITY`** / **`FACILITY_EQUIPMENT`** remain standard PPDM paths for tying equipment to facilities; compressor-specific extension rows use the **`COMPRESSOR_*`** entities above.

---

## References

- **Project:** `Beep.OilandGas.CompressorAnalysis`
- **Architecture:** `Plans/Architecture/COMPRESSOR_ANALYSIS_ARCHITECTURE_PLAN.md`
- **Phased plans:** `Beep.OilandGas.CompressorAnalysis/.plans/README.md`

---

**Last updated:** April 2026  
**Status:** Integrated for packaged calculations and **`CompressorController`**; extend **`CompressorAnalysisModule`** when new persisted scenarios are added.
