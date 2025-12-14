# Field Management Mappers Summary

## Overview

All mappers follow the same pattern: they accept **overloadable value retrieval functions** in their constructors, allowing you to provide custom data retrieval logic for any property. If not provided, they use default implementations from the `ValueRetrievers` class.

## Available Mappers

### 1. NodalAnalysisMapper
**Maps to:** `WellboreProperties`, `ReservoirProperties`

**Usage:**
```csharp
var mapper = new NodalAnalysisMapper(
    getWaterCut: (well) => await GetWaterCutFromProduction(well.UWI),
    getGasOilRatio: (well) => await GetGORFromProduction(well.UWI)
);

var wellboreProps = mapper.MapToDomain(well, tubular, wellPressure);
var reservoirProps = mapper.MapToReservoirProperties(well, wellPressure);
```

### 2. ProductionForecastingMapper
**Maps to:** `ReservoirForecastProperties`

**Usage:**
```csharp
var mapper = new ProductionForecastingMapper(
    getPermeability: (well) => await GetPermeabilityFromReservoir(well.UWI),
    getThickness: (well) => await GetThicknessFromPayzone(well.UWI)
);

var forecastProps = mapper.MapToDomain(well, wellPressure, tubular);
```

### 3. WellTestAnalysisMapper
**Maps to:** `WellTestData`

**Usage:**
```csharp
var mapper = new WellTestAnalysisMapper(
    getFormationThickness: (well) => await GetThicknessFromReservoir(well.UWI),
    getPorosity: (well) => await GetPorosityFromReservoir(well.UWI)
);

var wellTestData = mapper.MapToWellTestData(wellTest, pressureData, flowData, well, tubular);
```

### 4. DCAMapper
**Maps to:** `ProductionDataPoint`

**Usage:**
```csharp
var mapper = new DCAMapper(
    getDate: (production) => production.PERIOD_END_DATE ?? DateTime.MinValue,
    getOilRate: (production) => (double)(production.OIL_PRODUCTION ?? 0m)
);

var dataPoints = mapper.MapToDomain(productionEntities);
```

### 5. PumpPerformanceMapper
**Maps to:** `ESPDesignProperties`

**Usage:**
```csharp
var mapper = new PumpPerformanceMapper(
    getWellDepth: (well, tubular) => well.BASE_DEPTH,
    getCasingDiameter: (well, tubular) => tubular?.OUTSIDE_DIAMETER ?? 0m
);

var espProps = mapper.MapToESPDesignProperties(well, tubular, wellPressure, desiredFlowRate, totalDynamicHead);
```

### 6. GasLiftMapper
**Maps to:** `GasLiftWellProperties`

**Usage:**
```csharp
var mapper = new GasLiftMapper(
    getDesiredProductionRate: (well) => await GetDesiredProductionRate(well.UWI)
);

var gasLiftProps = mapper.MapToGasLiftWellProperties(well, tubular, wellPressure);
```

### 7. SuckerRodPumpingMapper
**Maps to:** `SuckerRodSystemProperties`

**Usage:**
```csharp
var mapper = new SuckerRodPumpingMapper(
    getRodDiameter: (well) => await GetRodDiameterFromEquipment(well.UWI),
    getPumpDiameter: (well) => await GetPumpDiameterFromEquipment(well.UWI),
    getStrokeLength: (well) => await GetStrokeLengthFromEquipment(well.UWI),
    getStrokesPerMinute: (well) => await GetSPMFromEquipment(well.UWI)
);

var suckerRodProps = mapper.MapToSuckerRodSystemProperties(well, tubular, wellPressure);
```

### 8. PlungerLiftMapper
**Maps to:** `PlungerLiftWellProperties`

**Usage:**
```csharp
var mapper = new PlungerLiftMapper(
    getPlungerDiameter: (well) => await GetPlungerDiameterFromEquipment(well.UWI),
    getCasingPressure: (well, wellPressure) => wellPressure?.CASING_PRESSURE ?? 0m,
    getLiquidProductionRate: (well) => await GetLiquidProductionRate(well.UWI)
);

var plungerLiftProps = mapper.MapToPlungerLiftWellProperties(well, tubular, wellPressure);
```

### 9. HydraulicPumpsMapper
**Maps to:** `HydraulicPumpWellProperties`

**Usage:**
```csharp
var mapper = new HydraulicPumpsMapper(
    getDesiredProductionRate: (well) => await GetDesiredProductionRate(well.UWI)
);

var hydraulicPumpProps = mapper.MapToHydraulicPumpWellProperties(well, tubular, wellPressure);
```

### 10. ChokeAnalysisMapper
**Maps to:** `ChokeProperties`, `GasChokeProperties`

**Usage:**
```csharp
var mapper = new ChokeAnalysisMapper(
    getChokeDiameter: (well) => await GetChokeDiameterFromEquipment(well.UWI),
    getChokeType: (well) => ChokeType.Bean,
    getZFactor: (well) => await GetZFactorFromGasProperties(well.UWI),
    getGasFlowRate: (well) => await GetGasFlowRate(well.UWI)
);

var chokeProps = mapper.MapToChokeProperties(well, wellPressure);
var gasChokeProps = mapper.MapToGasChokeProperties(well, wellPressure);
```

### 11. CompressorAnalysisMapper
**Maps to:** `CompressorOperatingConditions`

**Usage:**
```csharp
var mapper = new CompressorAnalysisMapper(
    getSuctionPressure: (well, wellPressure) => wellPressure?.FLOW_TUBING_PRESSURE ?? 0m,
    getDischargePressure: (well, wellPressure) => await GetDischargePressure(well.UWI),
    getGasFlowRate: (well) => await GetGasFlowRate(well.UWI)
);

var compressorConditions = mapper.MapToCompressorOperatingConditions(well, wellPressure);
```

### 12. PipelineAnalysisMapper
**Maps to:** `PipelineProperties`, `GasPipelineFlowProperties`, `LiquidPipelineFlowProperties`

**Usage:**
```csharp
var mapper = new PipelineAnalysisMapper(
    getPipelineDiameter: (well) => await GetPipelineDiameter(well.UWI),
    getPipelineLength: (well) => await GetPipelineLength(well.UWI),
    getGasFlowRate: (well) => await GetGasFlowRate(well.UWI),
    getLiquidFlowRate: (well) => await GetLiquidFlowRate(well.UWI)
);

var pipelineProps = mapper.MapToPipelineProperties(well, wellPressure);
var gasPipelineProps = mapper.MapToGasPipelineFlowProperties(well, wellPressure);
var liquidPipelineProps = mapper.MapToLiquidPipelineFlowProperties(well, wellPressure);
```

### 13. FlashCalculationsMapper
**Maps to:** `FlashConditions`

**Usage:**
```csharp
var mapper = new FlashCalculationsMapper(
    getPressure: (well, wellPressure) => wellPressure?.FLOW_TUBING_PRESSURE ?? 0m,
    getTemperature: (well, wellPressure) => await GetTemperature(well.UWI),
    getFeedComposition: (well) => await GetFeedCompositionFromFluidProperties(well.UWI)
);

var flashConditions = mapper.MapToFlashConditions(well, wellPressure);
```

## Common Value Retrievers

All mappers can use these common retrievers from `ValueRetrievers` class:

- `GetTubingDiameter` - Gets tubing diameter in inches
- `GetTubingLength` - Gets tubing length in feet
- `GetWellheadPressure` - Gets wellhead pressure in psi
- `GetWellheadTemperature` - Gets wellhead temperature in Fahrenheit
- `GetBottomholeTemperature` - Gets bottomhole temperature in Fahrenheit
- `GetReservoirPressure` - Gets reservoir pressure in psi
- `GetWaterCut` - Gets water cut (fraction)
- `GetGasOilRatio` - Gets gas-oil ratio in SCF/STB
- `GetOilGravity` - Gets oil gravity in API
- `GetGasSpecificGravity` - Gets gas specific gravity
- `GetFormationVolumeFactor` - Gets formation volume factor in RB/STB
- `GetOilViscosity` - Gets oil viscosity in cp
- `GetBubblePointPressure` - Gets bubble point pressure in psi
- `GetProductivityIndex` - Gets productivity index in BPD/psi
- `GetWellboreRadius` - Gets wellbore radius in feet

## Pattern

All mappers follow this pattern:

1. **Default Constructor**: Uses `ValueRetrievers` defaults
2. **Parameterized Constructor**: Accepts custom value retrieval functions
3. **Map Methods**: Map PPDM39 entities to domain models
4. **Error Handling**: Throws `InvalidOperationException` when required data is missing

## Benefits

- ✅ **Flexible**: Override any value retrieval
- ✅ **Testable**: Inject mock functions for testing
- ✅ **No Hidden Defaults**: Missing data throws clear exceptions
- ✅ **Extensible**: Add custom logic for any property
- ✅ **Type-Safe**: Strongly typed function signatures

## Next Steps

1. Create repository layer to automatically query related entities
2. Integrate mappers into `DataFlowService`
3. Add async support for value retrievers (if needed)
4. Create mapper factory for dependency injection

