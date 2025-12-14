# Mapper Usage Guide

## Overview

All mappers now use **overloadable value retrieval functions** instead of fixed defaults. This allows you to provide custom data retrieval logic for any property.

## Architecture

### ValueRetrievers Class

The `ValueRetrievers` class provides default implementations that extract values from PPDM39 entities. You can override any of these by providing custom functions to the mapper constructor.

### Mapper Pattern

Each mapper accepts value retrieval functions in its constructor. If not provided, it uses the default `ValueRetrievers` implementations.

---

## Usage Examples

### Example 1: Using Default Value Retrievers

```csharp
// Create mapper with default retrievers
var mapper = new NodalAnalysisMapper();

// Map with related entities
var well = await wellRepository.GetByUWIAsync("123456789012345");
var tubular = await tubularRepository.GetByUWIAsync("123456789012345");
var wellPressure = await wellPressureRepository.GetByUWIAsync("123456789012345");

var wellboreProperties = mapper.MapToDomain(well, tubular, wellPressure);
```

### Example 2: Overriding Specific Value Retrievers

```csharp
// Create mapper with custom retrievers for specific properties
var mapper = new NodalAnalysisMapper(
    getWaterCut: (well) => 
    {
        // Custom logic: Query production data
        var production = await productionRepository.GetLatestProductionAsync(well.UWI);
        return production.WaterCut;
    },
    getGasOilRatio: (well) => 
    {
        // Custom logic: Query production data
        var production = await productionRepository.GetLatestProductionAsync(well.UWI);
        return production.GasOilRatio;
    },
    getOilGravity: (well) => 
    {
        // Custom logic: Query fluid properties
        var fluidProps = await fluidPropertiesRepository.GetByWellAsync(well.UWI);
        return fluidProps.OilGravity;
    }
);

// Use the mapper
var wellboreProperties = mapper.MapToDomain(well, tubular, wellPressure);
```

### Example 3: Complete Custom Implementation

```csharp
// Create mapper with all custom retrievers
var mapper = new NodalAnalysisMapper(
    getTubingDiameter: (well, tubular) => 
    {
        // Your custom logic
        if (tubular != null)
            return (double)tubular.INSIDE_DIAMETER;
        return await GetTubingDiameterFromDatabase(well.UWI);
    },
    getTubingLength: (well, tubular) => 
    {
        // Your custom logic
        return CalculateTubingLength(well, tubular);
    },
    getWellheadPressure: (well, wellPressure) => 
    {
        // Your custom logic
        return await GetLatestWellheadPressure(well.UWI);
    },
    getWaterCut: (well) => await GetWaterCutFromProduction(well.UWI),
    getGasOilRatio: (well) => await GetGORFromProduction(well.UWI),
    getOilGravity: (well) => await GetOilGravityFromFluidProps(well.UWI),
    getGasSpecificGravity: (well) => await GetGasGravityFromFluidProps(well.UWI),
    getWellheadTemperature: (well, wellPressure) => await GetTemperatureFromSensor(well.UWI),
    getBottomholeTemperature: (well, wellPressure) => await CalculateBHT(well)
);
```

### Example 4: ProductionForecastingMapper with Custom Retrievers

```csharp
var mapper = new ProductionForecastingMapper(
    getInitialPressure: (well, wellPressure) => 
    {
        if (wellPressure != null && wellPressure.INIT_RESERVOIR_PRESSURE > 0)
            return wellPressure.INIT_RESERVOIR_PRESSURE;
        return await GetReservoirPressureFromReservoir(well.UWI);
    },
    getPermeability: (well) => await GetPermeabilityFromReservoir(well.UWI),
    getThickness: (well) => await GetThicknessFromPayzone(well.UWI),
    getDrainageRadius: (well) => CalculateDrainageRadius(well),
    getPorosity: (well) => await GetPorosityFromReservoir(well.UWI),
    getSkinFactor: (well) => await GetSkinFactorFromWellTest(well.UWI)
);

var forecastProperties = mapper.MapToDomain(well, wellPressure, tubular);
```

### Example 5: WellTestAnalysisMapper with Custom Retrievers

```csharp
var mapper = new WellTestAnalysisMapper(
    getFormationThickness: (well) => await GetThicknessFromReservoir(well.UWI),
    getPorosity: (well) => await GetPorosityFromReservoir(well.UWI),
    getTotalCompressibility: (well) => await GetCompressibilityFromReservoir(well.UWI),
    getOilViscosity: (well) => await GetOilViscosityFromFluidProps(well.UWI),
    getOilFormationVolumeFactor: (well) => await GetBoFromFluidProps(well.UWI)
);

var wellTestData = mapper.MapToWellTestData(
    wellTest, 
    pressureData, 
    flowData, 
    well, 
    tubular
);
```

---

## Available Value Retrievers

### NodalAnalysisMapper

| Function | Signature | Description |
|----------|-----------|-------------|
| `getTubingDiameter` | `Func<WELL, WELL_TUBULAR?, double>` | Gets tubing diameter in inches |
| `getTubingLength` | `Func<WELL, WELL_TUBULAR?, double>` | Gets tubing length in feet |
| `getWellheadPressure` | `Func<WELL, WELL_PRESSURE?, double>` | Gets wellhead pressure in psi |
| `getWellheadTemperature` | `Func<WELL, WELL_PRESSURE?, double>` | Gets wellhead temperature in Fahrenheit |
| `getBottomholeTemperature` | `Func<WELL, WELL_PRESSURE?, double>` | Gets bottomhole temperature in Fahrenheit |
| `getWaterCut` | `Func<WELL, double>` | Gets water cut (fraction) |
| `getGasOilRatio` | `Func<WELL, double>` | Gets gas-oil ratio in SCF/STB |
| `getOilGravity` | `Func<WELL, double>` | Gets oil gravity in API |
| `getGasSpecificGravity` | `Func<WELL, double>` | Gets gas specific gravity |
| `getReservoirPressure` | `Func<WELL, WELL_PRESSURE?, double>` | Gets reservoir pressure in psi |
| `getBubblePointPressure` | `Func<WELL, double>` | Gets bubble point pressure in psi |
| `getProductivityIndex` | `Func<WELL, double>` | Gets productivity index in BPD/psi |
| `getFormationVolumeFactor` | `Func<WELL, double>` | Gets formation volume factor in RB/STB |
| `getOilViscosity` | `Func<WELL, double>` | Gets oil viscosity in cp |

### ProductionForecastingMapper

| Function | Signature | Description |
|----------|-----------|-------------|
| `getInitialPressure` | `Func<WELL, WELL_PRESSURE?, decimal>` | Gets initial reservoir pressure in psi |
| `getPermeability` | `Func<WELL, decimal>` | Gets permeability in md |
| `getThickness` | `Func<WELL, decimal>` | Gets formation thickness in feet |
| `getDrainageRadius` | `Func<WELL, decimal>` | Gets drainage radius in feet |
| `getWellboreRadius` | `Func<WELL, WELL_TUBULAR?, decimal>` | Gets wellbore radius in feet |
| `getFormationVolumeFactor` | `Func<WELL, decimal>` | Gets formation volume factor in RB/STB |
| `getOilViscosity` | `Func<WELL, decimal>` | Gets oil viscosity in cp |
| `getTotalCompressibility` | `Func<WELL, decimal>` | Gets total compressibility in 1/psi |
| `getPorosity` | `Func<WELL, decimal>` | Gets porosity (fraction) |
| `getSkinFactor` | `Func<WELL, decimal>` | Gets skin factor |
| `getGasSpecificGravity` | `Func<WELL, decimal>` | Gets gas specific gravity |
| `getTemperature` | `Func<WELL, WELL_PRESSURE?, decimal>` | Gets reservoir temperature in Rankine |

### WellTestAnalysisMapper

| Function | Signature | Description |
|----------|-----------|-------------|
| `getTestType` | `Func<WELL_TEST, WellTestType>` | Determines test type |
| `getFlowRate` | `Func<IEnumerable<WELL_TEST_FLOW>?, double>` | Gets flow rate in BPD |
| `getWellboreRadius` | `Func<WELL, WELL_TUBULAR?, double>` | Gets wellbore radius in feet |
| `getFormationThickness` | `Func<WELL, double>` | Gets formation thickness in feet |
| `getPorosity` | `Func<WELL, double>` | Gets porosity (fraction) |
| `getTotalCompressibility` | `Func<WELL, double>` | Gets total compressibility in 1/psi |
| `getOilViscosity` | `Func<WELL, double>` | Gets oil viscosity in cp |
| `getOilFormationVolumeFactor` | `Func<WELL, double>` | Gets oil formation volume factor in RB/STB |
| `getProductionTime` | `Func<IEnumerable<WELL_TEST_FLOW>?, double>` | Gets production time in hours |
| `isGasWell` | `Func<WELL, bool>` | Determines if well is gas well |
| `getGasSpecificGravity` | `Func<WELL, double>` | Gets gas specific gravity |
| `getReservoirTemperature` | `Func<WELL, double>` | Gets reservoir temperature in Fahrenheit |
| `getInitialReservoirPressure` | `Func<IEnumerable<WELL_TEST_PRESSURE>?, double>` | Gets initial reservoir pressure in psi |

---

## Default Behavior

### Default ValueRetrievers

The `ValueRetrievers` class provides default implementations that:
- Extract values from PPDM39 entities when available
- Handle unit conversions automatically
- Throw `InvalidOperationException` when data is not available (no silent defaults)

### Error Handling

If a required value is not available and no custom retriever is provided, the mapper will throw an `InvalidOperationException` with a clear message indicating which value is missing and how to provide it.

---

## Best Practices

1. **Provide Custom Retrievers for Missing Data**: If your data source doesn't have certain properties, provide custom retrievers that query alternative sources.

2. **Use Async Patterns**: When creating custom retrievers that query databases, consider using async/await patterns in your implementation.

3. **Cache Frequently Accessed Data**: For performance, consider caching retrieved values if they don't change frequently.

4. **Validate Data**: Add validation in your custom retrievers to ensure data quality.

5. **Handle Missing Data Gracefully**: Decide whether to throw exceptions or return default values based on your business requirements.

---

## Integration with Repository Layer

When you implement the repository layer, you can create mapper instances with repository-based retrievers:

```csharp
public class WellService
{
    private readonly IWellRepository _wellRepository;
    private readonly IProductionRepository _productionRepository;
    private readonly IFluidPropertiesRepository _fluidPropertiesRepository;
    private readonly NodalAnalysisMapper _mapper;

    public WellService(
        IWellRepository wellRepository,
        IProductionRepository productionRepository,
        IFluidPropertiesRepository fluidPropertiesRepository)
    {
        _wellRepository = wellRepository;
        _productionRepository = productionRepository;
        _fluidPropertiesRepository = fluidPropertiesRepository;

        // Create mapper with repository-based retrievers
        _mapper = new NodalAnalysisMapper(
            getWaterCut: async (well) => 
            {
                var production = await _productionRepository.GetLatestAsync(well.UWI);
                return production?.WaterCut ?? 0.0;
            },
            getGasOilRatio: async (well) => 
            {
                var production = await _productionRepository.GetLatestAsync(well.UWI);
                return production?.GasOilRatio ?? 0.0;
            },
            getOilGravity: async (well) => 
            {
                var fluidProps = await _fluidPropertiesRepository.GetByWellAsync(well.UWI);
                return fluidProps?.OilGravity ?? 35.0;
            }
            // ... other retrievers
        );
    }
}
```

---

**Note**: The current implementation uses synchronous functions. For async operations, you would need to modify the mapper pattern to support async/await, or handle async operations within the synchronous function wrappers.

