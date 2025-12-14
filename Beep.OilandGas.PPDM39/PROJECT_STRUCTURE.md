# PPDM39 Project Structure

## Overview

This document describes the separation of concerns between the two main projects:

1. **Beep.OilandGas.PPDM39** - Models, Interfaces, and DTOs only (no implementations)
2. **Beep.OilandGas.PPDM39.DataManagement** - Business access layer with all implementations

## Project Structure

```
Beep.OilandGas.PPDM39/                          # Models, Interfaces, DTOs Only
├── Models/                                      # PPDM 3.9 Entity Models (2600+ entities)
│   ├── STRAT_UNIT.cs
│   ├── STRAT_COLUMN.cs
│   ├── WELL.cs
│   └── ... (all PPDM entities)
│
├── Core/
│   └── Interfaces/                             # Core Interfaces Only
│       ├── IPPDMEntity.cs                      # Base interface for all PPDM entities
│       ├── IPPDMRepository.cs                 # Generic repository interface
│       └── ICommonColumnHandler.cs            # Common column handler interface
│
├── Repositories/                                # Repository Interfaces Only
│   └── Stratigraphy/
│       ├── IStratUnitRepository.cs
│       ├── IStratColumnRepository.cs
│       └── ... (other repository interfaces)
│
├── Services/                                    # Service Interfaces Only
│   └── Stratigraphy/
│       ├── IStratUnitService.cs
│       ├── IStratColumnService.cs
│       └── ... (other service interfaces)
│
└── DTOs/                                        # Data Transfer Objects
    └── Stratigraphy/
        ├── StratUnitDto.cs
        ├── StratColumnDto.cs
        └── ... (other DTOs)

Beep.OilandGas.PPDM39.DataManagement/           # Implementations Only
├── Core/
│   ├── Base/
│   │   └── PPDMRepositoryBase.cs              # Base repository implementation
│   └── Common/
│       └── CommonColumnHandler.cs             # Common column handler implementation
│
├── Repositories/                                # Repository Implementations
│   └── Stratigraphy/
│       ├── StratUnitRepository.cs
│       ├── StratColumnRepository.cs
│       └── ... (other repository implementations)
│
└── Services/                                    # Service Implementations
    └── Stratigraphy/
        ├── StratUnitService.cs
        ├── StratColumnService.cs
        └── ... (other service implementations)
```

## Key Principles

### Beep.OilandGas.PPDM39 (Contracts Only)
- ✅ Contains all PPDM 3.9 entity models
- ✅ Contains all interfaces (repositories, services, handlers)
- ✅ Contains all DTOs
- ❌ NO implementations
- ❌ NO business logic
- ❌ NO data access code

### Beep.OilandGas.PPDM39.DataManagement (Implementations)
- ✅ Contains all repository implementations
- ✅ Contains all service implementations
- ✅ Contains common column handler implementation
- ✅ Contains base classes
- ✅ Contains business logic
- ✅ Contains data access code
- ❌ NO models (references Beep.OilandGas.PPDM39)
- ❌ NO interfaces (references Beep.OilandGas.PPDM39)

## Dependencies

```
Beep.OilandGas.PPDM39
└── (No project dependencies - only NuGet packages)

Beep.OilandGas.PPDM39.DataManagement
├── Project Reference: Beep.OilandGas.PPDM39
└── NuGet: TheTechIdea.Beep.DataManagementEngine
```

## Common Columns Pattern

All PPDM entities share common audit and metadata columns that are handled automatically:

### Standard Common Columns:
- `ACTIVE_IND` - Active indicator ('Y'/'N')
- `ROW_CREATED_BY` - User who created the row
- `ROW_CREATED_DATE` - Creation timestamp
- `ROW_CHANGED_BY` - User who last changed the row
- `ROW_CHANGED_DATE` - Last change timestamp
- `ROW_EFFECTIVE_DATE` - Effective date
- `ROW_EXPIRY_DATE` - Expiry date
- `ROW_QUALITY` - Data quality indicator
- `PPDM_GUID` - Unique identifier
- `AREA_ID` - Area identifier
- `AREA_TYPE` - Area type
- `BUSINESS_ASSOCIATE_ID` - Business associate reference
- `EFFECTIVE_DATE` - Business effective date
- `EXPIRY_DATE` - Business expiry date
- `SOURCE` - Data source
- `REMARK` - Remarks/notes

The `CommonColumnHandler` automatically manages these columns for all entities.

## Usage Example

```csharp
// In your application project
using Beep.OilandGas.PPDM39.Repositories.Stratigraphy;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.Stratigraphy;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;

// Register dependencies (e.g., in DI container)
services.AddScoped<ICommonColumnHandler, CommonColumnHandler>();
services.AddScoped<IStratUnitRepository, StratUnitRepository>();
services.AddScoped<IStratUnitService, StratUnitService>();

// Use in your code
public class MyService
{
    private readonly IStratUnitService _stratUnitService;
    
    public MyService(IStratUnitService stratUnitService)
    {
        _stratUnitService = stratUnitService;
    }
    
    public async Task CreateStratUnit(StratUnitDto dto, string userId)
    {
        var result = await _stratUnitService.CreateAsync(dto, userId);
        // Common columns are automatically handled
    }
}
```

## Benefits of This Structure

1. **Separation of Concerns**: Clear separation between contracts and implementations
2. **Testability**: Easy to mock interfaces for unit testing
3. **Flexibility**: Can swap implementations without changing contracts
4. **Maintainability**: Changes to implementations don't affect consumers
5. **Reusability**: Models and interfaces can be shared across multiple projects
6. **Clean Architecture**: Follows dependency inversion principle

## Next Steps

1. Implement remaining repository interfaces
2. Implement service interfaces
3. Add validation logic
4. Add mapping between entities and DTOs
5. Add unit tests
6. Add integration tests

