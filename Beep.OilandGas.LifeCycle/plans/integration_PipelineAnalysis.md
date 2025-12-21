# Beep.OilandGas.PipelineAnalysis - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.PipelineAnalysis** is a comprehensive library for pipeline capacity and flow analysis in oil and gas operations, supporting both gas and liquid pipelines.

### Key Capabilities
- **Gas Pipeline Analysis**: Pipeline capacity calculations (Weymouth equation)
- **Liquid Pipeline Analysis**: Pipeline capacity calculations (Darcy-Weisbach equation)
- **Flow Rate Calculations**: Flow rate for given pressure drop
- **Pressure Drop Calculations**: Pressure drop for given flow rate
- **Friction Factor Calculations**: Friction factor calculations
- **Flow Regime Analysis**: Flow regime determination

### Current Status
⚠️ **Not Yet Integrated** - Should be integrated into `PPDMDevelopmentService` for pipeline management

---

## Key Classes and Interfaces

### Main Classes

#### `PipelineCapacityCalculator`
Pipeline capacity calculations.

**Key Methods:**
```csharp
public static class PipelineCapacityCalculator
{
    public static PipelineCapacityResult CalculateGasPipelineCapacity(
        GasPipelineFlowProperties flowProperties);
    
    public static PipelineCapacityResult CalculateLiquidPipelineCapacity(
        LiquidPipelineFlowProperties flowProperties);
}
```

#### `PipelineFlowCalculator`
Flow rate and pressure drop calculations.

**Key Methods:**
```csharp
public static class PipelineFlowCalculator
{
    public static PipelineFlowAnalysisResult CalculateGasFlow(
        GasPipelineFlowProperties flowProperties);
    
    public static PipelineFlowAnalysisResult CalculateLiquidFlow(
        LiquidPipelineFlowProperties flowProperties);
    
    public static decimal CalculateGasPressureDrop(
        GasPipelineFlowProperties flowProperties);
}
```

#### `PipelineProperties`
Pipeline physical properties.

**Key Properties:**
```csharp
public class PipelineProperties
{
    public decimal Diameter { get; set; }              // inches
    public decimal Length { get; set; }                // feet
    public decimal Roughness { get; set; }              // feet
    public decimal ElevationChange { get; set; }         // feet
    public decimal InletPressure { get; set; }           // psia
    public decimal OutletPressure { get; set; }         // psia
    public decimal AverageTemperature { get; set; }     // Rankine
}
```

---

## Integration with LifeCycle Services

### Planned Integration

**Service:** `PPDMDevelopmentService` or `PipelineService`  
**Location:** `Beep.OilandGas.LifeCycle.Services.Development.PPDMDevelopmentService`

### Integration Points

1. **Pipeline Analysis Method**
   - Method: `AnalyzePipelineCapacityAsync(PipelineAnalysisRequest request)`
   - Retrieves pipeline data from PPDM database
   - Performs capacity or flow analysis
   - Stores results in `PIPELINE` or related table

2. **Data Flow**
   ```
   Pipeline Data (PPDM) → PipelineCalculator → PipelineResult → PipelineAnalysisResult DTO → PPDM Database
   ```

### Service Methods (To Be Added)

```csharp
public interface IPipelineService
{
    Task<PipelineCapacityResult> AnalyzePipelineCapacityAsync(PipelineCapacityRequest request);
    Task<PipelineFlowAnalysisResult> AnalyzePipelineFlowAsync(PipelineFlowRequest request);
}
```

---

## Usage Examples

### Example 1: Gas Pipeline Capacity

```csharp
using Beep.OilandGas.PipelineAnalysis.Models;
using Beep.OilandGas.PipelineAnalysis.Calculations;

var pipeline = new PipelineProperties
{
    Diameter = 12m,
    Length = 50000m,
    Roughness = 0.00015m,
    ElevationChange = 100m,
    InletPressure = 1000m,
    OutletPressure = 500m,
    AverageTemperature = 540m
};

var gasFlowProperties = new GasPipelineFlowProperties
{
    Pipeline = pipeline,
    GasFlowRate = 5000m,
    GasSpecificGravity = 0.65m,
    GasMolecularWeight = 18.8m,
    BasePressure = 14.7m,
    BaseTemperature = 520m
};

var capacityResult = PipelineCapacityCalculator.CalculateGasPipelineCapacity(
    gasFlowProperties);

Console.WriteLine($"Maximum Flow Rate: {capacityResult.MaximumFlowRate:F2} Mscf/day");
Console.WriteLine($"Pressure Drop: {capacityResult.PressureDrop:F2} psi");
```

### Example 2: Integration with LifeCycle Service (Planned)

```csharp
var pipelineService = serviceProvider.GetRequiredService<IPipelineService>();

var request = new PipelineCapacityRequest
{
    PipelineId = "PIPELINE-001",
    AnalysisType = "GAS",
    UserId = "user123"
};

var result = await pipelineService.AnalyzePipelineCapacityAsync(request);
```

---

## Integration Patterns

### Adding Pipeline Analysis to LifeCycle Services

1. **Add Dependency**
   ```csharp
   using Beep.OilandGas.PipelineAnalysis;
   using Beep.OilandGas.PipelineAnalysis.Calculations;
   ```

2. **Add to Development Service**
   ```csharp
   public async Task<PipelineCapacityResult> AnalyzePipelineCapacityAsync(
       PipelineCapacityRequest request)
   {
       // Retrieve pipeline data from PPDM
       // Build PipelineProperties
       // Perform analysis
       // Store results
       // Return result
   }
   ```

---

## Data Storage

### PPDM Tables

#### Existing Tables: `PIPELINE` and Related

**Status:** ✅ **Existing PPDM Tables** - Already exist in PPDM39.

1. **`PIPELINE`** - Pipeline data
   - Stores pipeline infrastructure data
   - Links to `FIELD` via `FIELD_ID`
   - Use for: Pipeline properties and analysis results

2. **`PIPELINE_SEGMENT`** - Pipeline segments
   - Stores segment data
   - Links to `PIPELINE`
   - Use for: Segment-level analysis

3. **`PIPELINE_FIELD`** - Pipeline-field relationships
   - Links pipelines to fields
   - Use for: Field-pipeline associations

### Relationships

- `PIPELINE.FIELD_ID` → `FIELD.FIELD_ID`
- `PIPELINE_SEGMENT.PIPELINE_ID` → `PIPELINE.PIPELINE_ID`
- `PIPELINE_FIELD.PIPELINE_ID` → `PIPELINE.PIPELINE_ID`
- `PIPELINE_FIELD.FIELD_ID` → `FIELD.FIELD_ID`

---

## Best Practices

1. **Pipeline Properties**
   - Use accurate diameter and length
   - Account for elevation changes
   - Consider roughness effects

2. **Flow Properties**
   - Use correct gas properties
   - Account for temperature effects
   - Consider flow regime

---

## Future Enhancements

### Planned Integrations

1. **Development Service Integration**
   - Automatic pipeline capacity analysis
   - Integration with pipeline design
   - System optimization

2. **Production Service Integration**
   - Pipeline performance monitoring
   - Flow optimization
   - Integration with production forecasting

---

## References

- **Project Location:** `Beep.OilandGas.PipelineAnalysis`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Development.PPDMDevelopmentService` (planned)
- **Documentation:** `Beep.OilandGas.PipelineAnalysis/README.md`
- **PPDM Table:** `PIPELINE`

---

**Last Updated:** 2024  
**Status:** ⚠️ Not Yet Integrated (Should be integrated)

