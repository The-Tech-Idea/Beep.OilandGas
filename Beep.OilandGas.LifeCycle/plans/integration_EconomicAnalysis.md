# Beep.OilandGas.EconomicAnalysis - LifeCycle Integration Guide

## Overview

**Beep.OilandGas.EconomicAnalysis** is a comprehensive .NET library for economic analysis and financial evaluation of oil and gas projects. It provides industry-standard methods for project valuation, risk analysis, and decision-making.

### Key Capabilities
- **NPV Calculation**: Net Present Value analysis
- **IRR Calculation**: Internal Rate of Return
- **Payback Period**: Simple and discounted payback
- **Cash Flow Modeling**: Time-series cash flow analysis
- **DCF Analysis**: Discounted Cash Flow calculations
- **Economic Indicators**: PI, MIRR, ROI, etc.
- **Sensitivity Analysis**: Parameter sensitivity evaluation
- **Scenario Comparison**: Multiple scenario analysis
- **Risk Analysis**: Monte Carlo simulation (planned)

### Current Status
⚠️ **Not Yet Integrated** - Should be integrated into `PPDMCalculationService` for centralized economic evaluation

### Architecture Decision
**Service:** `PPDMCalculationService` (Centralized Calculation Service)  
**Rationale:** Pure calculation/analysis project - performs mathematical computations without operational data management. Centralized service provides consistent data mapping and result storage.

---

## Key Classes and Interfaces

### Main Classes

#### `EconomicAnalyzer`
Main analyzer class for economic analysis.

**Key Methods:**
```csharp
public static class EconomicAnalyzer
{
    public static double CalculateNPV(CashFlow[] cashFlows, double discountRate);
    public static double CalculateIRR(CashFlow[] cashFlows, double initialGuess = 0.1);
    public static EconomicResult Analyze(
        CashFlow[] cashFlows, 
        double discountRate,
        double financeRate = 0.1, 
        double reinvestRate = 0.1);
    public static List<NPVProfilePoint> GenerateNPVProfile(
        CashFlow[] cashFlows, 
        double minRate = 0.0, 
        double maxRate = 1.0, 
        int points = 50);
}
```

#### `EconomicCalculator`
Static calculator class for economic calculations.

**Key Methods:**
```csharp
public static class EconomicCalculator
{
    public static double CalculateNPV(CashFlow[] cashFlows, double discountRate);
    public static double CalculateIRR(CashFlow[] cashFlows, double initialGuess = 0.1);
    public static double CalculateMIRR(CashFlow[] cashFlows, double financeRate, double reinvestRate);
    public static double CalculatePaybackPeriod(CashFlow[] cashFlows);
    public static double CalculateDiscountedPaybackPeriod(CashFlow[] cashFlows, double discountRate);
    public static double CalculateProfitabilityIndex(CashFlow[] cashFlows, double discountRate);
    public static double CalculateROI(CashFlow[] cashFlows);
    public static EconomicResult Analyze(
        CashFlow[] cashFlows, 
        double discountRate,
        double financeRate = 0.1, 
        double reinvestRate = 0.1);
    public static List<NPVProfilePoint> GenerateNPVProfile(
        CashFlow[] cashFlows, 
        double minRate = 0.0, 
        double maxRate = 1.0, 
        int points = 50);
    public static SensitivityResult SensitivityAnalysis(
        CashFlow[] cashFlows, 
        double discountRate, 
        string[] parameters);
}
```

#### `CashFlow`
Cash flow model.

**Key Properties:**
```csharp
public class CashFlow
{
    public int Period { get; set; }      // Time period (0, 1, 2, ...)
    public double Amount { get; set; }    // Cash flow amount (negative for outflows)
}
```

#### `EconomicResult`
Comprehensive economic analysis results.

**Key Properties:**
```csharp
public class EconomicResult
{
    public double NPV { get; set; }
    public double IRR { get; set; }
    public double MIRR { get; set; }
    public double PaybackPeriod { get; set; }
    public double DiscountedPaybackPeriod { get; set; }
    public double ProfitabilityIndex { get; set; }
    public double ROI { get; set; }
    public List<NPVProfilePoint> NPVProfile { get; set; }
}
```

#### `NPVProfilePoint`
NPV profile data point.

```csharp
public class NPVProfilePoint
{
    public double DiscountRate { get; set; }
    public double NPV { get; set; }
}
```

#### `SensitivityResult`
Sensitivity analysis results.

```csharp
public class SensitivityResult
{
    public Dictionary<string, double> ParameterSensitivity { get; set; }
    public List<SensitivityPoint> SensitivityPoints { get; set; }
}
```

---

## Integration with LifeCycle Services

### Planned Integration

**Service:** `PPDMCalculationService`  
**Location:** `Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService`

### Integration Points

1. **Economic Analysis Method**
   - Method: `PerformEconomicAnalysisAsync(EconomicAnalysisRequest request)`
   - Calculates NPV, IRR, payback period, etc.
   - Stores results in `ECONOMIC_ANALYSIS` table

2. **Data Flow**
   ```
   Cash Flow Data → Economic Calculator → EconomicResult → EconomicAnalysisResult DTO → PPDM Database
   ```

3. **Storage**
   - Results stored in `ECONOMIC_ANALYSIS` table
   - Uses `PPDMGenericRepository` for data access
   - Links to `PROSPECT`, `WELL`, `POOL`, or `FIELD` via IDs

### Service Methods (To Be Added)

```csharp
public interface ICalculationService
{
    Task<EconomicAnalysisResult> PerformEconomicAnalysisAsync(EconomicAnalysisRequest request);
    Task<SensitivityResult> PerformSensitivityAnalysisAsync(SensitivityAnalysisRequest request);
    Task<List<EconomicAnalysisResult>> CompareScenariosAsync(ScenarioComparisonRequest request);
}
```

---

## Usage Examples

### Example 1: Basic NPV and IRR Calculation

```csharp
using Beep.OilandGas.EconomicAnalysis;
using Beep.OilandGas.EconomicAnalysis.Models;

// Define project cash flows
var cashFlows = new CashFlow[]
{
    new CashFlow { Period = 0, Amount = -1000000 },  // Initial investment
    new CashFlow { Period = 1, Amount = 300000 },
    new CashFlow { Period = 2, Amount = 400000 },
    new CashFlow { Period = 3, Amount = 500000 },
    new CashFlow { Period = 4, Amount = 300000 },
    new CashFlow { Period = 5, Amount = 200000 }
};

// Calculate NPV at 10% discount rate
double discountRate = 0.10;
double npv = EconomicAnalyzer.CalculateNPV(cashFlows, discountRate);
Console.WriteLine($"NPV: ${npv:N2}");

// Calculate IRR
double irr = EconomicAnalyzer.CalculateIRR(cashFlows);
Console.WriteLine($"IRR: {irr:P2}");

// Calculate payback period
double payback = EconomicCalculator.CalculatePaybackPeriod(cashFlows);
Console.WriteLine($"Payback Period: {payback:F2} years");
```

### Example 2: Comprehensive Economic Analysis

```csharp
using Beep.OilandGas.EconomicAnalysis.Calculations;

// Perform comprehensive analysis
var result = EconomicAnalyzer.Analyze(
    cashFlows,
    discountRate: 0.10,
    financeRate: 0.08,
    reinvestRate: 0.12);

Console.WriteLine($"NPV: ${result.NPV:N2}");
Console.WriteLine($"IRR: {result.IRR:P2}");
Console.WriteLine($"MIRR: {result.MIRR:P2}");
Console.WriteLine($"Payback Period: {result.PaybackPeriod:F2} years");
Console.WriteLine($"Profitability Index: {result.ProfitabilityIndex:F2}");
Console.WriteLine($"ROI: {result.ROI:P2}");
```

### Example 3: NPV Profile Generation

```csharp
// Generate NPV profile (NPV vs discount rate)
var npvProfile = EconomicAnalyzer.GenerateNPVProfile(
    cashFlows,
    minRate: 0.0,
    maxRate: 0.5,
    points: 50);

foreach (var point in npvProfile)
{
    Console.WriteLine($"Discount Rate: {point.DiscountRate:P2}, NPV: ${point.NPV:N2}");
}
```

### Example 4: Sensitivity Analysis

```csharp
// Perform sensitivity analysis
var sensitivity = EconomicCalculator.SensitivityAnalysis(
    cashFlows,
    discountRate: 0.10,
    parameters: new[] { "Initial Investment", "Revenue", "Operating Cost" });

// Access sensitivity results
foreach (var kvp in sensitivity.ParameterSensitivity)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value:P2}");
}
```

### Example 5: Integration with LifeCycle Service (Planned)

```csharp
// In a controller or service
var calculationService = serviceProvider.GetRequiredService<ICalculationService>();

var request = new EconomicAnalysisRequest
{
    ProspectId = "PROSPECT-001",
    CashFlows = new[]
    {
        new CashFlow { Period = 0, Amount = -5000000 },
        new CashFlow { Period = 1, Amount = 2000000 },
        new CashFlow { Period = 2, Amount = 3000000 },
        new CashFlow { Period = 3, Amount = 2500000 },
        new CashFlow { Period = 4, Amount = 1500000 }
    },
    DiscountRate = 0.10,
    FinanceRate = 0.08,
    ReinvestRate = 0.12,
    UserId = "user123"
};

var result = await calculationService.PerformEconomicAnalysisAsync(request);

Console.WriteLine($"NPV: ${result.NPV:N2}");
Console.WriteLine($"IRR: {result.IRR:P2}");
```

### Example 6: Prospect Economic Evaluation

```csharp
// Evaluate a prospect economically
public async Task<EconomicAnalysisResult> EvaluateProspectAsync(string prospectId)
{
    // Get prospect data
    var explorationService = serviceProvider.GetRequiredService<IFieldExplorationService>();
    var prospect = await explorationService.GetProspectAsync(prospectId);
    
    // Build cash flows from prospect data
    var cashFlows = BuildCashFlowsFromProspect(prospect);
    
    // Perform economic analysis
    var calculationService = serviceProvider.GetRequiredService<ICalculationService>();
    var request = new EconomicAnalysisRequest
    {
        ProspectId = prospectId,
        CashFlows = cashFlows,
        DiscountRate = 0.10,
        UserId = GetCurrentUserId()
    };
    
    return await calculationService.PerformEconomicAnalysisAsync(request);
}
```

---

## Integration Patterns

### Adding Economic Analysis to LifeCycle Services

1. **Add Dependency**
   ```csharp
   using Beep.OilandGas.EconomicAnalysis;
   using Beep.OilandGas.EconomicAnalysis.Calculations;
   using Beep.OilandGas.EconomicAnalysis.Models;
   ```

2. **Add to Calculation Service**
   ```csharp
   public class PPDMCalculationService : ICalculationService
   {
       public async Task<EconomicAnalysisResult> PerformEconomicAnalysisAsync(
           EconomicAnalysisRequest request)
       {
           // Validate request
           // Build cash flows
           // Perform analysis
           // Store results
           // Return result
       }
   }
   ```

3. **Use in Other Services**
   ```csharp
   public class PPDMExplorationService
   {
       private readonly ICalculationService _calculationService;
       
       public async Task<EconomicAnalysisResult> EvaluateProspectEconomicsAsync(
           string prospectId)
       {
           // Get prospect data
           // Build cash flows
           // Perform economic analysis
           return await _calculationService.PerformEconomicAnalysisAsync(request);
       }
   }
   ```

### Error Handling

```csharp
try
{
    var result = EconomicAnalyzer.CalculateNPV(cashFlows, discountRate);
    
    if (result < 0)
    {
        _logger.LogWarning("Negative NPV for project {ProjectId}", projectId);
    }
}
catch (ArgumentException ex)
{
    _logger.LogError(ex, "Invalid cash flow data");
    throw;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error performing economic analysis");
    throw;
}
```

---

## Data Storage

### PPDM Tables

#### New Table Required: `ECONOMIC_ANALYSIS`

**Status:** ⚠️ **New Table Needed** - Does not exist in PPDM39, must be created following PPDM patterns.

**Table Structure:**
```sql
CREATE TABLE ECONOMIC_ANALYSIS (
    ECONOMIC_ANALYSIS_ID VARCHAR(50) PRIMARY KEY,
    PROSPECT_ID VARCHAR(50) NULL,
    WELL_ID VARCHAR(50) NULL,
    POOL_ID VARCHAR(50) NULL,
    FIELD_ID VARCHAR(50) NULL,
    CALCULATION_DATE DATETIME,
    DISCOUNT_RATE DECIMAL(10,4),
    NPV DECIMAL(18,2),
    IRR DECIMAL(10,4),
    MIRR DECIMAL(10,4),
    PAYBACK_PERIOD DECIMAL(10,2),
    DISCOUNTED_PAYBACK_PERIOD DECIMAL(10,2),
    PROFITABILITY_INDEX DECIMAL(10,4),
    ROI DECIMAL(10,4),
    CASH_FLOWS_JSON NVARCHAR(MAX),  -- JSON serialized cash flows
    NPV_PROFILE_JSON NVARCHAR(MAX),  -- JSON serialized NPV profile
    -- Standard PPDM columns
    ROW_ID VARCHAR(50),
    ROW_CHANGED_BY VARCHAR(50),
    ROW_CHANGED_DATE DATETIME,
    ROW_CREATED_BY VARCHAR(50),
    ROW_CREATED_DATE DATETIME
);
```

**Foreign Keys:**
- `ECONOMIC_ANALYSIS.PROSPECT_ID` → `PROSPECT.PROSPECT_ID` (if exists)
- `ECONOMIC_ANALYSIS.WELL_ID` → `WELL.WELL_ID`
- `ECONOMIC_ANALYSIS.POOL_ID` → `POOL.POOL_ID`
- `ECONOMIC_ANALYSIS.FIELD_ID` → `FIELD.FIELD_ID`

**Notes:**
- Store cash flows and NPV profile as JSON for flexibility
- At least one of PROSPECT_ID, WELL_ID, POOL_ID, or FIELD_ID must be provided
- Follow PPDM naming conventions and include standard PPDM audit columns

### Data Model

```csharp
public class EconomicAnalysisResult
{
    public string CalculationId { get; set; }
    public string? ProspectId { get; set; }
    public string? WellId { get; set; }
    public string? PoolId { get; set; }
    public string? FieldId { get; set; }
    public double NPV { get; set; }
    public double IRR { get; set; }
    public double MIRR { get; set; }
    public double PaybackPeriod { get; set; }
    public double DiscountedPaybackPeriod { get; set; }
    public double ProfitabilityIndex { get; set; }
    public double ROI { get; set; }
    public List<CashFlow> CashFlows { get; set; }
    public List<NPVProfilePoint> NPVProfile { get; set; }
    // ... other properties
}
```

### Relationships

- `ECONOMIC_ANALYSIS.PROSPECT_ID` → `PROSPECT.PROSPECT_ID`
- `ECONOMIC_ANALYSIS.WELL_ID` → `WELL.WELL_ID`
- `ECONOMIC_ANALYSIS.POOL_ID` → `POOL.POOL_ID`
- `ECONOMIC_ANALYSIS.FIELD_ID` → `FIELD.FIELD_ID`

---

## Best Practices

1. **Cash Flow Construction**
   - Include all relevant cash flows (revenues, costs, investments)
   - Use consistent time periods
   - Account for taxes and royalties

2. **Discount Rate Selection**
   - Use appropriate discount rate (WACC, hurdle rate, etc.)
   - Consider project risk
   - Document rate selection rationale

3. **Sensitivity Analysis**
   - Perform sensitivity analysis on key parameters
   - Identify critical assumptions
   - Test multiple scenarios

4. **Validation**
   - Validate cash flow data before analysis
   - Check for reasonable IRR values
   - Compare with industry benchmarks

---

## Future Enhancements

### Planned Integrations

1. **Exploration Service Integration**
   - Automatic economic evaluation for prospects
   - Integration with PROSPECT risk assessment
   - Portfolio economic analysis

2. **Development Service Integration**
   - Economic evaluation for development projects
   - Cost-benefit analysis
   - Project ranking and prioritization

3. **Production Service Integration**
   - Economic evaluation of production optimization
   - Well economics
   - Field economics

4. **FieldOrchestrator Integration**
   - Field-level economic analysis
   - Portfolio optimization
   - Comparative economic analysis

### Potential Improvements

- Add Monte Carlo simulation for risk analysis
- Integration with DCA for production forecasting
- Integration with accounting for cost inputs
- Enhanced visualization (NPV profiles, tornado diagrams)
- Support for international economic standards
- Real-time economic monitoring

---

## References

- **Project Location:** `Beep.OilandGas.EconomicAnalysis`
- **Service Integration:** `Beep.OilandGas.LifeCycle.Services.Calculations.PPDMCalculationService` (planned)
- **Documentation:** `Beep.OilandGas.EconomicAnalysis/README.md`
- **PPDM Table:** `ECONOMIC_ANALYSIS` (to be created)

---

**Last Updated:** 2024  
**Status:** ⚠️ Not Yet Integrated (Should be integrated)

