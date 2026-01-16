# Core Calculation Projects Enhancement Plan

## Objective
Enhance and standardize core calculation projects for maintainability, consistency, and alignment with Beep.OilandGas architecture. Ensure compatibility across all platforms (Web, WinForms, WPF, etc.).

## Scope: Core Calculation Projects
1. **Beep.OilandGas.ChokeAnalysis** - Choke flow calculations
2. **Beep.OilandGas.DCA** - Decline curve analysis
3. **Beep.OilandGas.NodalAnalysis** - Nodal analysis and optimization
4. **Beep.OilandGas.ProductionForecasting** - Production forecasting
5. **Beep.OilandGas.GasLift** - Gas lift design and analysis
6. **Beep.OilandGas.PipelineAnalysis** - Pipeline flow calculations
7. **Beep.OilandGas.EconomicAnalysis** - NPV, IRR, cash flow analysis
8. **Beep.OilandGas.FlashCalculations** - PVT flash calculations

---

## Phase 1: Audit & Baseline

### 1.1 Project Structure Review
For each core project:
- Identify current folder structure (Calculations/, Models/, Services/, etc.)
- List all classes, interfaces, and DTOs
- Document current service/controller patterns
- Identify platform-specific vs. platform-agnostic code

### 1.2 Code Inventory
- List all public methods and their input/output types
- Document any external dependencies (beyond .NET BCL)
- Identify any legacy patterns (static classes, direct SQL, hardcoded logic)
- Note any platform-specific code (Web, WinForms, etc.)

### 1.3 Dependency Analysis
- Check what each project depends on (PPDM39? Beep Framework?)
- Identify shared dependencies across calculation projects
- Check for circular dependencies

---

## Phase 2: Architecture Standardization

### 2.1 Project Structure Template
Each core project should follow:
```
Beep.OilandGas.{ProjectName}/
├── Calculations/
│   └── {Name}Calculator.cs
├── Services/
│   └── {Name}Service.cs
├── {ProjectName}.csproj
└── README.md
```

### 2.2 DTOs & Interfaces Location (CENTRALIZED IN MODELS)
**Check `Beep.OilandGas.Models` project FIRST**:
- If DTOs/interfaces already exist → Use them
- If DTOs/interfaces don't exist → Create them in `Beep.OilandGas.Models/Core/DTOs/{ProjectName}/`
- Move all DTOs and interfaces FROM calculation projects → TO `Beep.OilandGas.Models`

**Structure in Beep.OilandGas.Models:**
```
Beep.OilandGas.Models/
├── Core/
│   ├── DTOs/
│   │   ├── ChokeAnalysis/
│   │   │   ├── ChokeInputsDTOs.cs
│   │   │   └── ChokeOutputDTOs.cs
│   │   ├── GasLift/
│   │   │   └── GasLiftDTOs.cs
│   │   └── {ProjectName}/
│   │       └── {ProjectName}DTOs.cs
│   └── Interfaces/
│       ├── IChokeAnalysisService.cs
│       ├── IGasLiftService.cs
│       └── I{ProjectName}Service.cs
```

### 2.3 Service Pattern
Each calculation project exposes a service interface (defined in Models):
```csharp
// In Beep.OilandGas.Models/Core/Interfaces/
public interface IChokeAnalysisService
{
    Task<ChokeFlowResult> AnalyzeFlowAsync(ChokeInputs inputs);
    Task<List<ChokeFlowResult>> AnalyzeRangeAsync(ChokeInputs baseInputs, ParameterRange range);
}

// Implementation in Beep.OilandGas.ChokeAnalysis/Services/
public class ChokeAnalysisService : IChokeAnalysisService
{
    public async Task<ChokeFlowResult> AnalyzeFlowAsync(ChokeInputs inputs)
    {
        // Core calculation logic
    }
}
```

### 2.4 Data Class Inheritance & Interface Implementation

**ALL data classes in calculation projects MUST inherit from `Entity` AND implement `IPPDMEntity` interface:**

```csharp
// In Beep.OilandGas.Models/Core/DTOs/ChokeAnalysis/
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

public class ChokeInputs : Entity, IPPDMEntity
{
    private string _wellIdValue;
    public string WellId
    {
        get { return _wellIdValue; }
        set { SetProperty(ref _wellIdValue, value); }
    }

    private double _chokeSizeValue;
    public double ChokeSize
    {
        get { return _chokeSizeValue; }
        set { SetProperty(ref _chokeSizeValue, value); }
    }

    // IPPDMEntity interface properties
    private string _activeIndValue = "Y";
    public string ACTIVE_IND
    {
        get { return _activeIndValue; }
        set { SetProperty(ref _activeIndValue, value); }
    }

    private string _rowCreatedByValue;
    public string ROW_CREATED_BY
    {
        get { return _rowCreatedByValue; }
        set { SetProperty(ref _rowCreatedByValue, value); }
    }

    private DateTime? _rowCreatedDateValue;
    public DateTime? ROW_CREATED_DATE
    {
        get { return _rowCreatedDateValue; }
        set { SetProperty(ref _rowCreatedDateValue, value); }
    }

    private string _rowChangedByValue;
    public string ROW_CHANGED_BY
    {
        get { return _rowChangedByValue; }
        set { SetProperty(ref _rowChangedByValue, value); }
    }

    private DateTime? _rowChangedDateValue;
    public DateTime? ROW_CHANGED_DATE
    {
        get { return _rowChangedDateValue; }
        set { SetProperty(ref _rowChangedDateValue, value); }
    }

    private DateTime? _rowEffectiveDateValue;
    public DateTime? ROW_EFFECTIVE_DATE
    {
        get { return _rowEffectiveDateValue; }
        set { SetProperty(ref _rowEffectiveDateValue, value); }
    }

    private DateTime? _rowExpiryDateValue;
    public DateTime? ROW_EXPIRY_DATE
    {
        get { return _rowExpiryDateValue; }
        set { SetProperty(ref _rowExpiryDateValue, value); }
    }

    private string _rowQualityValue;
    public string ROW_QUALITY
    {
        get { return _rowQualityValue; }
        set { SetProperty(ref _rowQualityValue, value); }
    }

    private string _ppdmGuidValue;
    public string PPDM_GUID
    {
        get { return _ppdmGuidValue; }
        set { SetProperty(ref _ppdmGuidValue, value); }
    }
}

public class ChokeFlowResult : Entity, IPPDMEntity
{
    private double _pressureDropValue;
    public double PressureDrop
    {
        get { return _pressureDropValue; }
        set { SetProperty(ref _pressureDropValue, value); }
    }

    private double _flowVelocityValue;
    public double FlowVelocity
    {
        get { return _flowVelocityValue; }
        set { SetProperty(ref _flowVelocityValue, value); }
    }

    // IPPDMEntity interface properties (implement as shown above)
    // ACTIVE_IND, ROW_CREATED_BY, ROW_CREATED_DATE, ROW_CHANGED_BY, ROW_CHANGED_DATE, etc.
}
```

**Template for all classes in calculation DTOs:**
- Always inherit from: `Entity, IPPDMEntity`
- Use `SetProperty<T>()` for all properties to enable change notification
- Always implement all `IPPDMEntity` members (ACTIVE_IND, ROW_* audit fields, PPDM_GUID, ROW_QUALITY, ROW_EFFECTIVE_DATE, ROW_EXPIRY_DATE)
- Initialize ACTIVE_IND to "Y" by default

**Benefits:**
- Consistent audit trail across all data classes (ROW_CREATED_BY, ROW_CREATED_DATE, ROW_CHANGED_BY, ROW_CHANGED_DATE)
- PPDM compliance (ACTIVE_IND, PPDM_GUID, ROW_QUALITY)
- Property change notification via `SetProperty()` for WPF/Blazor/WinForms data binding
- Temporal data support (ROW_EFFECTIVE_DATE, ROW_EXPIRY_DATE)
- Full compatibility with PPDMGenericRepository, validation services, and audit services

---

## Phase 3: Data Access Refactoring

### 3.1 Input Data Handling
- If calculation inputs come from PPDM tables, use `PPDMGenericRepository` with `AppFilter`
- Example: Load well properties before gas lift analysis

### 3.2 Result Storage (Optional)
- Calculation results may be stored in PPDM if needed by downstream processes
- Use `CommonColumnHandler` for audit columns if storing results
- Do NOT version or audit calculation results (no requirement)

### 3.3 Data Access Pattern
```csharp
// If loading input data from PPDM
var metadata = await _metadata.GetTableMetadataAsync("WELL");
var repo = new PPDMGenericRepository(..., "WELL");
var wells = await repo.GetAsync(filters);

// Core calculation (platform-agnostic)
var result = _calculator.Calculate(wells);

// If storing result (optional)
var resultRepo = new PPDMGenericRepository(..., "CALCULATION_RESULT");
await resultRepo.InsertAsync(result, userId);
```

---

### 5.2 Field-Scoped Integration (if applicable)
- If calculation is field/well-specific, use `FieldOrchestrator` to ensure context
- Example: Gas lift analysis for current field's wells only

---

## Phase 6: Platform Compatibility

### 6.1 Code Organization
- **Core calculation logic**: .NET 6+ class library (no platform-specific code)
- **Data access layer**: Optional, depends on whether PPDM data is needed
- **API wrapper**: ASP.NET Core for Web consumers
- **Service client**: Can be generated for other platforms (WinForms, WPF, Console)

### 6.2 Dependencies to Avoid in Core
- No `System.Web`, `System.Windows`, `System.Reactive`
- No async UI frameworks (Blazor, MVVM)
- No platform-specific OS calls unless abstracted

### 6.3 Reusability
- Core calculation library can be referenced by:
  - Web API (via dependency injection)
  - WinForms app (via direct instantiation or DI)
  - WPF app (via service clients)
  - Console tools (via direct instantiation)

---

## Phase 7: Validation & Error Handling

### 7.1 Input Validation
Use `PPDMDataValidationService` for PPDM entity validation before calculation:
```csharp
var validationService = new PPDMDataValidationService(...);
var validationResult = await validationService.ValidateAsync(well, "WELL");
if (!validationResult.IsValid)
    throw new InvalidOperationException($"Invalid input: {string.Join(", ", validationResult.Errors)}");
```

### 7.2 Calculation Error Handling
- Wrap calculation logic in try/catch
- Log exceptions with context (well ID, inputs, etc.)
- Return meaningful error messages to client
- Avoid exposing stack traces in API responses

---

## Phase 8: Testing

### 8.1 Unit Tests
- Test core calculation logic (isolated, no dependencies)
- Test edge cases (zero division, negative values, etc.)
- Test input validation

### 8.2 Integration Tests (if using PPDM data)
- Test loading inputs from PPDM
- Test calculation with real data
- Test result storage (if applicable)

### 8.3 API Tests (if exposing as Web API)
- Test endpoints with valid/invalid inputs
- Test authorization
- Test response formats

---

## Deliverables

1. ✅ Audited baseline for each core project
2. ✅ Standardized project structure
3. ✅ DTOs/interfaces moved OilandGas.Models
4. ✅ Service interfaces defined for each project
5. ✅ Data access refactored to use PPDMGenericRepository
6. ✅ DI registration updated in Program.cs
8. ✅ Unit and integration tests
9. ✅ Documentation for each calculation project

---

## References
- `.github/copilot-instructions.md`, `CLAUDE.md`
- `.cursor/commands/architecture-patterns.md`, `.cursor/commands/beep-dataaccess-generic-repository.md`
- `.cursor/commands/naming-conventions.md`, `.cursor/commands/best-practices.md`
