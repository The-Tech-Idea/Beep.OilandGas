# Beep.OilandGas.NodalAnalysis - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.NodalAnalysis** is a comprehensive .NET library for Inflow Performance Relationship (IPR) and Vertical Lift Performance (VLP) analysis for production system optimization.

### Key Capabilities
- **IPR Curves**: Generate inflow performance relationships using multiple methods
- **VLP Curves**: Vertical lift performance calculations
- **Nodal Analysis**: System optimization at operating point
- **Gas Lift Optimization**: Gas lift design and optimization
- **ESP Optimization**: Electric submersible pump optimization
- **Choke Performance**: Choke sizing and performance
- **Tubing Optimization**: Optimal tubing size selection
- **Production Prediction**: Rate prediction at different conditions

### Current Status
⚠️ **Not Yet Integrated** - Should be integrated into `PPDMCalculationService` for centralized well performance analysis

### Architecture Decision
**Service:** `PPDMCalculationService` (Centralized Calculation Service)  
**Rationale:** Pure calculation/analysis project - performs IPR/VLP calculations and analysis without operational data management. Centralized service provides consistent data mapping and result storage.

---

## Key Classes and Interfaces

### Main Classes

#### `IPRCalculator`
IPR curve generation.

**Key Methods:**
```csharp
public static class IPRCalculator
{
    public static List<IPRPoint> GenerateVogelIPR(
        ReservoirProperties reservoir, 
        double maxFlowRate);
    public static List<IPRPoint> GenerateFetkovichIPR(
        ReservoirProperties reservoir, 
        double[] flowRates, 
        double[] pressures);
    public static List<IPRPoint> GenerateWigginsIPR(
        ReservoirProperties reservoir, 
        double maxFlowRate);
    public static List<IPRPoint> GenerateGasWellIPR(
        ReservoirProperties reservoir, 
        double maxFlowRate);
}
```

#### `VLPCalculator`
VLP curve generation.

**Key Methods:**
```csharp
public static class VLPCalculator
{
    public static List<VLPPoint> GenerateVLP(
        WellboreProperties wellbore, 
        double[] flowRates);
    public static List<VLPPoint> GenerateVLPWithCorrelation(
        WellboreProperties wellbore, 
        double[] flowRates, 
        MultiphaseCorrelation correlation);
}
```

#### `NodalAnalyzer`
Nodal analysis operations.

**Key Methods:**
```csharp
public static class NodalAnalyzer
{
    public static OperatingPoint FindOperatingPoint(
        List<IPRPoint> iprCurve, 
        List<VLPPoint> vlpCurve);
    public static double PredictProductionRate(
        ReservoirProperties reservoir, 
        WellboreProperties wellbore);
    public static OptimizationResult OptimizeTubingSize(
        ReservoirProperties reservoir, 
        WellboreProperties wellbore, 
        double[] tubingSizes);
}
```

#### `ReservoirProperties`
Reservoir properties for IPR.

**Key Properties:**
```csharp
public class ReservoirProperties
{
    public double ReservoirPressure { get; set; }      // psi
    public double BubblePointPressure { get; set; }     // psi
    public double ProductivityIndex { get; set; }      // BPD/psi
    public double WaterCut { get; set; }                // fraction
    public double GasOilRatio { get; set; }             // SCF/STB
}
```

#### `WellboreProperties`
Wellbore properties for VLP.

**Key Properties:**
```csharp
public class WellboreProperties
{
    public double TubingDiameter { get; set; }         // inches
    public double TubingLength { get; set; }            // feet
    public double WellheadPressure { get; set; }        // psi
    public double WaterCut { get; set; }                 // fraction
    public double GasOilRatio { get; set; }              // SCF/STB
    public double OilGravity { get; set; }               // API
    public double GasSpecificGravity { get; set; }
}
```

#### `OperatingPoint`
Operating point result.

```csharp
public class OperatingPoint
{
    public double FlowRate { get; set; }    // BPD
    public double Pressure { get; set; }    // psi
}
```

---

## Integration with LifeCycle Services

### Planned Integration

**Service:** `PPDMCalculationService`  
**Location:** `Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService`

### Integration Points

1. **Nodal Analysis Method**
   - Method: `PerformNodalAnalysisAsync(NodalAnalysisRequest request)`
   - Retrieves well and reservoir data from PPDM database
   - Performs IPR/VLP analysis
   - Stores results in `NODAL_ANALYSIS` table

2. **Data Flow**
   ```
   Well/Reservoir Data (PPDM) → IPR/VLP Calculation → OperatingPoint → NodalAnalysisResult DTO → PPDM Database
   ```

3. **Storage**
   - Results stored in `NODAL_ANALYSIS` table
   - Uses `PPDMGenericRepository` for data access
   - Links to `WELL` via `WELL_ID`

### Service Methods (To Be Added)

```csharp
public interface ICalculationService
{
    Task<NodalAnalysisResult> PerformNodalAnalysisAsync(NodalAnalysisRequest request);
    Task<OptimizationResult> OptimizeWellPerformanceAsync(WellOptimizationRequest request);
    Task<ProductionPrediction> PredictProductionRateAsync(ProductionPredictionRequest request);
}
```

---

## Usage Examples

### Example 1: Basic Nodal Analysis

```csharp
using Beep.OilandGas.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis.Calculations;

// Define reservoir properties
var reservoir = new ReservoirProperties
{
    ReservoirPressure = 3000,      // psi
    BubblePointPressure = 2500,   // psi
    ProductivityIndex = 2.5,      // BPD/psi
    WaterCut = 0.1                // fraction
};

// Generate IPR curve
var iprCurve = IPRCalculator.GenerateVogelIPR(reservoir, maxFlowRate: 5000);

// Define wellbore properties
var wellbore = new WellboreProperties
{
    TubingDiameter = 2.875,       // inches
    TubingLength = 8000,          // feet
    WellheadPressure = 500,       // psi
    WaterCut = 0.1,
    GasOilRatio = 500             // SCF/STB
};

// Generate VLP curve
var vlpCurve = VLPCalculator.GenerateVLP(
    wellbore, 
    flowRates: iprCurve.Select(p => p.FlowRate).ToArray());

// Perform nodal analysis
var operatingPoint = NodalAnalyzer.FindOperatingPoint(iprCurve, vlpCurve);
Console.WriteLine($"Operating Flow Rate: {operatingPoint.FlowRate} BPD");
Console.WriteLine($"Operating Pressure: {operatingPoint.Pressure} psi");
```

### Example 2: Integration with LifeCycle Service (Planned)

```csharp
// In a controller or service
var calculationService = serviceProvider.GetRequiredService<ICalculationService>();

var request = new NodalAnalysisRequest
{
    WellId = "WELL-001",
    AnalysisType = "IPR_VLP",
    AdditionalParameters = new Dictionary<string, object>
    {
        { "IPRMethod", "Vogel" },
        { "VLPCorrelation", "HagedornBrown" },
        { "MaxFlowRate", 5000.0 }
    },
    UserId = "user123"
};

var result = await calculationService.PerformNodalAnalysisAsync(request);

Console.WriteLine($"Operating Flow Rate: {result.OperatingFlowRate} BPD");
Console.WriteLine($"Operating Pressure: {result.OperatingPressure} psi");
Console.WriteLine($"IPR Points: {result.IPRCurve.Count}");
Console.WriteLine($"VLP Points: {result.VLPCurve.Count}");
```

### Example 3: Production Rate Prediction

```csharp
// Predict production rate at different wellhead pressures
var prediction = NodalAnalyzer.PredictProductionRate(reservoir, wellbore);
Console.WriteLine($"Predicted Production Rate: {prediction} BPD");
```

### Example 4: Tubing Size Optimization

```csharp
// Optimize tubing size
var tubingSizes = new double[] { 2.375, 2.875, 3.5, 4.0 }; // inches
var optimizationResult = NodalAnalyzer.OptimizeTubingSize(
    reservoir, 
    wellbore, 
    tubingSizes);

Console.WriteLine($"Optimal Tubing Size: {optimizationResult.OptimalTubingSize} inches");
Console.WriteLine($"Maximum Production Rate: {optimizationResult.MaximumProductionRate} BPD");
```

---

## Integration Patterns

### Adding Nodal Analysis to LifeCycle Services

1. **Add Dependency**
   ```csharp
   using Beep.OilandGas.NodalAnalysis;
   using Beep.OilandGas.NodalAnalysis.Calculations;
   ```

2. **Add to Calculation Service**
   ```csharp
   public class PPDMCalculationService : ICalculationService
   {
       public async Task<NodalAnalysisResult> PerformNodalAnalysisAsync(
           NodalAnalysisRequest request)
       {
           // Retrieve well and reservoir data from PPDM
           // Build reservoir and wellbore properties
           // Perform IPR/VLP analysis
           // Store results
           // Return result
       }
   }
   ```

3. **Use in Production Service**
   ```csharp
   public class PPDMProductionService
   {
       private readonly ICalculationService _calculationService;
       
       public async Task<NodalAnalysisResult> AnalyzeWellPerformanceAsync(
           string wellId)
       {
           var request = new NodalAnalysisRequest
           {
               WellId = wellId,
               AnalysisType = "IPR_VLP",
               UserId = GetCurrentUserId()
           };
           
           return await _calculationService.PerformNodalAnalysisAsync(request);
       }
   }
   ```

---

## Data Storage

### PPDM Tables

#### New Table Required: `NODAL_ANALYSIS`

**Status:** ⚠️ **New Table Needed** - Does not exist in PPDM39, must be created following PPDM patterns.

**Table Structure:**
```sql
CREATE TABLE NODAL_ANALYSIS (
    NODAL_ANALYSIS_ID VARCHAR(50) PRIMARY KEY,
    WELL_ID VARCHAR(50) NOT NULL,
    ANALYSIS_DATE DATETIME,
    OPERATING_FLOW_RATE DECIMAL(18,2),  -- BPD
    OPERATING_PRESSURE DECIMAL(18,2),   -- psi
    IPR_METHOD VARCHAR(50),              -- Vogel, Fetkovich, Wiggins, etc.
    VLP_CORRELATION VARCHAR(50),         -- HagedornBrown, BeggsBrill, etc.
    IPR_CURVE_JSON NVARCHAR(MAX),        -- JSON serialized IPR curve points
    VLP_CURVE_JSON NVARCHAR(MAX),        -- JSON serialized VLP curve points
    -- Standard PPDM columns
    ROW_ID VARCHAR(50),
    ROW_CHANGED_BY VARCHAR(50),
    ROW_CHANGED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50),
    ROW_CREATED_DATE DATETIME
);
```

**Foreign Keys:**
- `NODAL_ANALYSIS.WELL_ID` → `WELL.WELL_ID`

**Notes:**
- Store IPR and VLP curves as JSON for flexibility
- Follow PPDM naming conventions and include standard PPDM audit columns

### Data Model

```csharp
public class NodalAnalysisResult
{
    public string CalculationId { get; set; }
    public string WellId { get; set; }
    public double OperatingFlowRate { get; set; }      // BPD
    public double OperatingPressure { get; set; }      // psi
    public List<IPRPoint> IPRCurve { get; set; }
    public List<VLPPoint> VLPCurve { get; set; }
    public OperatingPoint OperatingPoint { get; set; }
    // ... other properties
}
```

### Relationships

- `NODAL_ANALYSIS.WELL_ID` → `WELL.WELL_ID`

---

## Best Practices

1. **Reservoir Properties**
   - Use accurate reservoir pressure and productivity index
   - Consider bubble point for two-phase flow
   - Update properties regularly

2. **Wellbore Properties**
   - Use actual tubing dimensions
   - Account for wellhead pressure
   - Consider water cut and GOR

3. **IPR Method Selection**
   - **Vogel**: Solution gas drive reservoirs
   - **Fetkovich**: Multi-point IPR with test data
   - **Wiggins**: Three-phase IPR
   - **Gas Well**: Gas wells

4. **VLP Correlation Selection**
   - **Hagedorn-Brown**: General purpose
   - **Beggs-Brill**: Horizontal and inclined wells
   - **Duns-Ros**: Vertical wells
   - Choose based on well conditions

---

## Future Enhancements

### Planned Integrations

1. **Production Service Integration**
   - Automatic nodal analysis when well data changes
   - Real-time performance monitoring
   - Production optimization recommendations

2. **Development Service Integration**
   - Well completion optimization
   - Tubing size selection for new wells
   - Artificial lift design

3. **Gas Lift Integration**
   - Integration with `Beep.OilandGas.GasLift`
   - Gas lift optimization
   - Valve placement optimization

---

## References

- **Project Location:** `Beep.OilandGas.NodalAnalysis`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService` (planned)
- **Documentation:** `Beep.OilandGas.NodalAnalysis/README.md`
- **PPDM Table:** `NODAL_ANALYSIS` (to be created)

---

**Last Updated:** 2024  
**Status:** ⚠️ Not Yet Integrated (Should be integrated)

