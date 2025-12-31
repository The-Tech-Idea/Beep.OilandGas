# Naming Conventions

## Overview

This document outlines naming conventions for projects, namespaces, files, and code organization in the Beep.OilandGas system.

## Project Naming Convention

### Standard Naming Pattern

All oil and gas projects follow the naming convention:

```
Beep.OilandGas.{ProjectName}
```

### Examples

#### Existing Projects
- ✅ `Beep.OilandGas.ProductionAccounting`
- ✅ `Beep.OilandGas.Drawing`
- ✅ `Beep.OilandGas.Accounting`
- ✅ `Beep.OilandGas.PPDM39`
- ✅ `Beep.OilandGas.PPDM39.DataManagement`
- ✅ `Beep.OilandGas.Web`
- ✅ `Beep.OilandGas.ApiService`

#### Projects to Create/Enhance

**High Priority:**
- `Beep.OilandGas.Properties` - Gas property calculations
- `Beep.OilandGas.NodalAnalysis` - Nodal analysis (enhance existing)
- `Beep.OilandGas.ProductionForecasting` - Production forecasting
- `Beep.OilandGas.ChokeAnalysis` - Choke flow analysis

**Medium Priority:**
- `Beep.OilandGas.GasLift` - Gas lift analysis
- `Beep.OilandGas.SuckerRodPumping` - Sucker rod pumping
- `Beep.OilandGas.PumpPerformance` - Pump performance (enhance existing)
- `Beep.OilandGas.PlungerLift` - Plunger lift

**Lower Priority:**
- `Beep.OilandGas.CompressorAnalysis` - Compressor analysis
- `Beep.OilandGas.HydraulicPumps` - Hydraulic pumps
- `Beep.OilandGas.PipelineAnalysis` - Pipeline capacity
- `Beep.OilandGas.FlashCalculations` - Flash calculations
- `Beep.OilandGas.WellTestAnalysis` - Well test analysis (enhance existing)

## Namespace Conventions

### Standard Namespace Pattern

```
Beep.OilandGas.{ProjectName}[.{SubNamespace}]
```

### Examples

```csharp
// Main project namespace
namespace Beep.OilandGas.ProductionAccounting
{
    // Main classes
}

// Sub-namespaces for organization
namespace Beep.OilandGas.ProductionAccounting.Financial
{
    // Financial accounting classes
}

namespace Beep.OilandGas.ProductionAccounting.Financial.SuccessfulEfforts
{
    // Successful efforts specific classes
}

namespace Beep.OilandGas.PPDM39.Core.Interfaces
{
    // Core interfaces
}

namespace Beep.OilandGas.PPDM39.Core.DTOs
{
    // DTOs
}

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    // Service implementations
}
```

## File Organization Patterns

### Project Structure

```
Beep.OilandGas.{ProjectName}/
├── Core/                          # Core functionality
│   ├── Interfaces/               # Service interfaces
│   └── DTOs/                     # Data transfer objects
├── Services/                      # Service implementations
│   ├── Exploration/
│   ├── Development/
│   └── Production/
├── Models/                        # Entity models (if needed)
├── Repositories/                 # Repository implementations
├── Controllers/                   # API controllers (if API project)
├── Pages/                         # Razor pages (if Web project)
├── Components/                    # UI components (if Web project)
├── Constants/                     # Constants
├── Exceptions/                   # Exception classes
└── README.md                      # Project documentation
```

### File Naming Conventions

**Classes:**
- Use PascalCase: `ProductionManager.cs`, `AllocationEngine.cs`
- Match class name: `class ProductionManager` → `ProductionManager.cs`

**Interfaces:**
- Prefix with `I`: `IProductionService.cs`
- Match interface name: `interface IProductionService` → `IProductionService.cs`

**DTOs:**
- Use descriptive names: `CreateProspectRequest.cs`, `ProspectResponse.cs`
- Suffix with purpose: `Request`, `Response`, `DTO`

**Controllers:**
- Suffix with `Controller`: `ExplorationController.cs`
- Match route: `[Route("api/exploration")]` → `ExplorationController.cs`

**Pages (Razor):**
- Use PascalCase: `Prospects.razor`, `ProspectDetail.razor`
- Match page purpose

**Components (Razor):**
- Use PascalCase: `FieldSelector.razor`, `ProgressDisplay.razor`
- Descriptive names

## Code Naming Conventions

### Classes and Interfaces

```csharp
// Classes: PascalCase
public class ProductionManager { }
public class AllocationEngine { }

// Interfaces: PascalCase with I prefix
public interface IProductionService { }
public interface IFieldOrchestrator { }
```

### Methods

```csharp
// Methods: PascalCase
public async Task<List<object>> GetProspectsForFieldAsync(string fieldId) { }
public async Task<object> CreateProspectAsync(object prospectData) { }
```

### Properties

```csharp
// Properties: PascalCase
public string CurrentFieldId { get; set; }
public string ConnectionName { get; private set; }
```

### Variables

```csharp
// Local variables: camelCase
var prospectId = "PROSPECT-001";
var fieldId = "FIELD-001";

// Private fields: camelCase with _ prefix
private readonly IDMEEditor _editor;
private string _currentFieldId;
```

### Constants

```csharp
// Constants: PascalCase
public const string DefaultConnectionName = "ProductionDB";
public const int MaxRetryAttempts = 3;
```

### Enums

```csharp
// Enums: PascalCase
public enum DispositionType
{
    Sale,
    Transfer,
    Inventory
}

// Enum values: PascalCase
DispositionType.Sale
```

## Database Naming Conventions

### Tables

- Use UPPER_CASE with underscores: `WELL`, `PROSPECT`, `REVENUE_TRANSACTION`
- Match PPDM standard naming: `BUSINESS_ASSOCIATE`, `OBLIGATION`

### Columns

- Use UPPER_CASE with underscores: `WELL_ID`, `FIELD_ID`, `ROW_CREATED_DATE`
- Match PPDM standard naming: `ACTIVE_IND`, `PPDM_GUID`

### Foreign Keys

- Reference table name + `_ID`: `FIELD_ID`, `WELL_ID`, `BUSINESS_ASSOCIATE_ID`

## API Endpoint Naming

### RESTful Conventions

```
GET    /api/field/current/exploration/prospects        # List resources
GET    /api/field/current/exploration/prospects/{id}  # Get single resource
POST   /api/field/current/exploration/prospects       # Create resource
PUT    /api/field/current/exploration/prospects/{id}  # Update resource
DELETE /api/field/current/exploration/prospects/{id}  # Delete resource
```

### Field-Scoped Endpoints

All phase endpoints are field-scoped:
- `/api/field/current/{phase}/{resource}`

Examples:
- `/api/field/current/exploration/prospects`
- `/api/field/current/development/pools`
- `/api/field/current/production/production`

## Benefits

1. **Namespace Consistency** - All projects under `Beep.OilandGas` namespace
2. **Easy Discovery** - Clear organization in solution explorer
3. **Professional Structure** - Industry-standard naming convention
4. **Scalability** - Easy to add new projects following the pattern
5. **Maintainability** - Consistent naming makes code easier to understand and maintain

## Key Principles

1. **Consistency**: Follow the same naming pattern throughout the codebase
2. **Clarity**: Names should clearly indicate purpose and functionality
3. **Standards**: Follow C# and .NET naming conventions
4. **PPDM Compliance**: Database names match PPDM standards
5. **RESTful**: API endpoints follow RESTful conventions

## References

- See `Beep.OilandGas.ProductionAccounting/PROJECT_NAMING_CONVENTION.md` for project naming details
- See Microsoft C# Naming Conventions: https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
- See PPDM documentation for database naming standards

