# Beep.OilandGas.WellTestAnalysis - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.WellTestAnalysis** is a comprehensive .NET library for well test analysis and pressure transient analysis (PTA) in oil and gas operations.

### Key Capabilities
- **Build-up Analysis**: Pressure build-up test interpretation
- **Drawdown Analysis**: Pressure drawdown test interpretation
- **Diagnostic Plots**: Log-log, semi-log, and derivative plots
- **Reservoir Model Identification**: Automatic model recognition
- **Permeability Calculation**: Formation permeability estimation
- **Skin Factor Calculation**: Wellbore damage/improvement quantification
- **Boundary Detection**: Reservoir boundaries and faults
- **Multi-rate Analysis**: Variable rate test analysis
- **Type Curve Matching**: Automated type curve matching

### Current Status
⚠️ **Not Yet Integrated** - Should be integrated into `PPDMCalculationService` for centralized well test interpretation

### Architecture Decision
**Service:** `PPDMCalculationService` (Centralized Calculation Service)  
**Rationale:** Pure calculation/analysis project - performs pressure transient analysis and interpretation without operational data management. Centralized service provides consistent data mapping and result storage.

---

## Key Classes and Interfaces

### Main Classes

#### `WellTestAnalyzer`
Well test analysis operations.

**Key Methods:**
```csharp
public static class WellTestAnalyzer
{
    public static WellTestResult AnalyzeBuildUp(WellTestData testData);
    public static WellTestResult AnalyzeDrawdown(WellTestData testData);
    public static WellTestResult AnalyzeMultiRate(WellTestData testData);
    public static ReservoirModel IdentifyReservoirModel(WellTestData testData);
}
```

#### `WellTestData`
Well test data model.

**Key Properties:**
```csharp
public class WellTestData
{
    public double[] Time { get; set; }
    public double[] Pressure { get; set; }
    public double FlowRate { get; set; }              // BPD
    public double WellboreRadius { get; set; }        // feet
    public double FormationThickness { get; set; }     // feet
    public double Porosity { get; set; }
    public double TotalCompressibility { get; set; }  // psi^-1
    public double OilViscosity { get; set; }          // cp
    public double OilFormationVolumeFactor { get; set; } // RB/STB
}
```

#### `WellTestResult`
Well test analysis results.

**Key Properties:**
```csharp
public class WellTestResult
{
    public double Permeability { get; set; }          // md
    public double SkinFactor { get; set; }
    public double ReservoirPressure { get; set; }     // psi
    public double ProductivityIndex { get; set; }      // BPD/psi
    public ReservoirModel ReservoirModel { get; set; }
    public List<DiagnosticPoint> DiagnosticPoints { get; set; }
}
```

---

## Integration with LifeCycle Services

### Planned Integration

**Service:** `PPDMCalculationService`  
**Location:** `Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService`

### Integration Points

1. **Well Test Analysis Method**
   - Method: `PerformWellTestAnalysisAsync(WellTestAnalysisRequest request)`
   - Retrieves well test data from PPDM database
   - Performs build-up or drawdown analysis
   - Stores results in `WELL_TEST` or similar table

2. **Data Flow**
   ```
   Well Test Data (PPDM) → WellTestAnalyzer → WellTestResult → WellTestAnalysisResult DTO → PPDM Database
   ```

### Service Methods (To Be Added)

```csharp
public interface ICalculationService
{
    Task<WellTestAnalysisResult> PerformWellTestAnalysisAsync(WellTestAnalysisRequest request);
    Task<ReservoirModel> IdentifyReservoirModelAsync(string wellId, string testId);
}
```

---

## Usage Examples

### Example 1: Build-up Analysis

```csharp
using Beep.OilandGas.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis.Analysis;

var testData = new WellTestData
{
    Time = new double[] { 0, 0.1, 0.5, 1, 2, 5, 10, 20, 50, 100 },
    Pressure = new double[] { 3000, 2950, 2900, 2850, 2800, 2750, 2720, 2700, 2680, 2670 },
    FlowRate = 1000,              // BPD
    WellboreRadius = 0.25,         // feet
    FormationThickness = 50,       // feet
    Porosity = 0.20,
    TotalCompressibility = 1e-5,  // psi^-1
    OilViscosity = 1.5,            // cp
    OilFormationVolumeFactor = 1.2 // RB/STB
};

var analysis = WellTestAnalyzer.AnalyzeBuildUp(testData);
Console.WriteLine($"Permeability: {analysis.Permeability} md");
Console.WriteLine($"Skin Factor: {analysis.SkinFactor}");
Console.WriteLine($"Reservoir Pressure: {analysis.ReservoirPressure} psi");
```

### Example 2: Integration with LifeCycle Service (Planned)

```csharp
var calculationService = serviceProvider.GetRequiredService<ICalculationService>();

var request = new WellTestAnalysisRequest
{
    WellId = "WELL-001",
    TestId = "TEST-001",
    AnalysisType = "BUILDUP",
    UserId = "user123"
};

var result = await calculationService.PerformWellTestAnalysisAsync(request);

Console.WriteLine($"Permeability: {result.Permeability} md");
Console.WriteLine($"Skin Factor: {result.SkinFactor}");
Console.WriteLine($"Productivity Index: {result.ProductivityIndex} BPD/psi");
```

---

## Integration Patterns

### Adding Well Test Analysis to LifeCycle Services

1. **Add Dependency**
   ```csharp
   using Beep.OilandGas.WellTestAnalysis;
   using Beep.OilandGas.WellTestAnalysis.Analysis;
   ```

2. **Add to Calculation Service**
   ```csharp
   public async Task<WellTestAnalysisResult> PerformWellTestAnalysisAsync(
       WellTestAnalysisRequest request)
   {
       // Retrieve well test data from PPDM
       // Build WellTestData
       // Perform analysis
       // Store results
       // Return result
   }
   ```

---

## Data Storage

### PPDM Tables

#### Existing Table: `WELL_TEST`

**Status:** ✅ **Existing PPDM Table** - Already exists in PPDM39, can be used for well test data.

**Usage:**
- Store well test raw data (pressure, time, flow rate)
- Store analysis results (permeability, skin factor, reservoir pressure)
- Use existing `WELL_TEST` table structure

**Note:** If additional analysis result fields are needed beyond what `WELL_TEST` provides, consider:
- Using `WELL_TEST_ANALYSIS` (exists in PPDM) for detailed analysis results
- Or extending `WELL_TEST` with additional columns if following PPDM extension patterns

### Relationships

- `WELL_TEST.WELL_ID` → `WELL.WELL_ID`
- `WELL_TEST_ANALYSIS.WELL_TEST_ID` → `WELL_TEST.WELL_TEST_ID` (if using analysis table)

---

## Best Practices

1. **Test Data Quality**
   - Ensure sufficient data points
   - Validate pressure and time data
   - Check for data anomalies

2. **Reservoir Properties**
   - Use accurate formation properties
   - Update properties based on test results
   - Consider wellbore storage effects

---

## Future Enhancements

### Planned Integrations

1. **Production Service Integration**
   - Automatic test analysis when test data is updated
   - Integration with production optimization
   - Reservoir characterization

2. **Exploration Service Integration**
   - Test analysis for exploratory wells
   - Integration with prospect evaluation

---

## References

- **Project Location:** `Beep.OilandGas.WellTestAnalysis`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService` (planned)
- **Documentation:** `Beep.OilandGas.WellTestAnalysis/README.md`
- **PPDM Table:** `WELL_TEST`

---

**Last Updated:** 2024  
**Status:** ⚠️ Not Yet Integrated (Should be integrated)

