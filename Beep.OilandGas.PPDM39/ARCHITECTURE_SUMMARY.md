# PPDM39 Data Management Architecture Summary

## Project Separation

The PPDM39 data management solution is split into two projects following clean architecture principles:

### 1. Beep.OilandGas.PPDM39 (Contracts Only)
**Purpose**: Contains models, interfaces, and DTOs - no implementations

### 2. Beep.OilandGas.PPDM39.DataManagement (Implementations)
**Purpose**: Contains all business logic and data access implementations

---

## Complete Tree Structure

```
Beep.OilandGas.PPDM39/                                    # Contracts Project
│
├── Models/                                                # PPDM 3.9 Entity Models (2600+)
│   ├── STRAT_UNIT.cs
│   ├── STRAT_COLUMN.cs
│   ├── STRAT_COLUMN_UNIT.cs
│   ├── STRAT_HIERARCHY.cs
│   ├── STRAT_WELL_SECTION.cs
│   ├── WELL.cs
│   └── ... (all PPDM 3.9 entities)
│
├── Core/
│   └── Interfaces/                                       # Core Interfaces
│       ├── IPPDMEntity.cs                               # Base interface for all entities
│       ├── IPPDMRepository<T>.cs                       # Generic repository interface
│       └── ICommonColumnHandler.cs                      # Common column handler interface
│
├── Repositories/                                         # Repository Interfaces
│   └── Stratigraphy/
│       ├── IStratUnitRepository.cs
│       ├── IStratColumnRepository.cs
│       └── ... (other repository interfaces)
│
├── Services/                                             # Service Interfaces
│   └── Stratigraphy/
│       ├── IStratUnitService.cs
│       ├── IStratColumnService.cs
│       └── ... (other service interfaces)
│
└── DTOs/                                                 # Data Transfer Objects
    └── Stratigraphy/
        ├── StratUnitDto.cs
        ├── StratColumnDto.cs
        └── ... (other DTOs)

Beep.OilandGas.PPDM39.DataManagement/                     # Implementations Project
│
├── Core/
│   ├── Base/
│   │   └── PPDMRepositoryBase<T>.cs                     # Base repository implementation
│   └── Common/
│       └── CommonColumnHandler.cs                        # Common column handler implementation
│
├── Repositories/                                         # Repository Implementations
│   └── Stratigraphy/
│       ├── StratUnitRepository.cs                      # Implements IStratUnitRepository
│       ├── StratColumnRepository.cs                     # Implements IStratColumnRepository
│       └── ... (other repository implementations)
│
└── Services/                                             # Service Implementations
    └── Stratigraphy/
        ├── StratUnitService.cs                          # Implements IStratUnitService
        ├── StratColumnService.cs                         # Implements IStratColumnService
        └── ... (other service implementations)
```

---

## Stratigraphy Domain Hierarchy

### Core Entities

```
Stratigraphy Domain
│
├── Stratigraphic Columns (STRAT_COLUMN)
│   ├── Column Units (STRAT_COLUMN_UNIT)
│   ├── Column Ages (STRAT_COL_UNIT_AGE)
│   └── Column References (STRAT_COLUMN_XREF)
│
├── Stratigraphic Units (STRAT_UNIT)
│   ├── Unit Descriptions (STRAT_UNIT_DESCRIPTION)
│   ├── Unit Components (STRAT_UNIT_COMPONENT)
│   ├── Unit Ages (STRAT_UNIT_AGE)
│   └── Unit Aliases (STRAT_ALIAS)
│
├── Stratigraphic Hierarchy (STRAT_HIERARCHY)
│   └── Hierarchy Descriptions (STRAT_HIERARCHY_DESC)
│
├── Stratigraphic Name Sets (STRAT_NAME_SET)
│   └── Name Set References (STRAT_NAME_SET_XREF)
│
├── Well Stratigraphy (STRAT_WELL_SECTION)
│   ├── Well Interpretations (STRAT_WELL_INTERP_AGE)
│   ├── Well Acquisitions (STRAT_WELL_ACQTN)
│   ├── Well Node Units (WELL_NODE_STRAT_UNIT)
│   ├── Well Core Units (WELL_CORE_STRAT_UNIT)
│   └── Well Test Units (WELL_TEST_STRAT_UNIT)
│
├── Field Stratigraphy (STRAT_FIELD_SECTION)
│   ├── Field Stations (STRAT_FIELD_STATION)
│   ├── Field Nodes (STRAT_FIELD_NODE)
│   ├── Field Acquisitions (STRAT_FIELD_ACQTN)
│   └── Field Interpretation Ages (STRAT_FLD_INTERP_AGE)
│
├── Stratigraphic Equivalences (STRAT_EQUIVALENCE)
│
├── Stratigraphic Correlations (STRAT_INTERP_CORR)
│
└── Stratigraphic Topographic Relations (STRAT_TOPO_RELATION)
```

---

## Common Columns Pattern

All PPDM entities automatically handle these common columns through `CommonColumnHandler`:

### Audit Columns
- `ACTIVE_IND` - Active indicator ('Y'/'N')
- `ROW_CREATED_BY` - User who created the row
- `ROW_CREATED_DATE` - Creation timestamp
- `ROW_CHANGED_BY` - User who last changed the row
- `ROW_CHANGED_DATE` - Last change timestamp
- `ROW_EFFECTIVE_DATE` - Effective date
- `ROW_EXPIRY_DATE` - Expiry date
- `ROW_QUALITY` - Data quality indicator

### Metadata Columns
- `PPDM_GUID` - Unique identifier (auto-generated)
- `AREA_ID` - Area identifier
- `AREA_TYPE` - Area type
- `BUSINESS_ASSOCIATE_ID` - Business associate reference
- `EFFECTIVE_DATE` - Business effective date
- `EXPIRY_DATE` - Business expiry date
- `SOURCE` - Data source
- `REMARK` - Remarks/notes

---

## Implementation Flow

### 1. Repository Layer (Data Access)
```csharp
// Interface in PPDM39 project
public interface IStratUnitRepository : IPPDMRepository<STRAT_UNIT>
{
    Task<IEnumerable<STRAT_UNIT>> GetByStratNameSetIdAsync(string stratNameSetId);
    // ... other methods
}

// Implementation in DataManagement project
public class StratUnitRepository : PPDMRepositoryBase<STRAT_UNIT>, IStratUnitRepository
{
    // Inherits common CRUD operations from PPDMRepositoryBase
    // Implements domain-specific queries
}
```

### 2. Service Layer (Business Logic)
```csharp
// Interface in PPDM39 project
public interface IStratUnitService
{
    Task<StratUnitDto> GetByIdAsync(string stratUnitId);
    Task<StratUnitDto> CreateAsync(StratUnitDto dto, string userId);
    // ... other methods
}

// Implementation in DataManagement project
public class StratUnitService : IStratUnitService
{
    private readonly IStratUnitRepository _repository;
    private readonly ICommonColumnHandler _commonColumnHandler;
    
    // Business logic, validation, DTO mapping
}
```

### 3. Common Column Handling
```csharp
// Automatic handling in repository base class
public async Task<T> InsertAsync(T entity, string userId)
{
    _commonColumnHandler.PrepareForInsert(entity, userId);
    // Sets: ROW_CREATED_BY, ROW_CREATED_DATE, ACTIVE_IND, PPDM_GUID, etc.
    await _unitOfWork.InsertAsync(entity, tableName);
    return entity;
}
```

---

## Usage Example

```csharp
// 1. Register dependencies (e.g., in Startup.cs or Program.cs)
services.AddScoped<ICommonColumnHandler, CommonColumnHandler>();
services.AddScoped<IStratUnitRepository, StratUnitRepository>();
services.AddScoped<IStratUnitService, StratUnitService>();

// 2. Use in your application
public class WellDataService
{
    private readonly IStratUnitService _stratUnitService;
    
    public WellDataService(IStratUnitService stratUnitService)
    {
        _stratUnitService = stratUnitService;
    }
    
    public async Task CreateStratigraphicUnit(string name, string nameSetId, string userId)
    {
        var dto = new StratUnitDto
        {
            STRAT_NAME_SET_ID = nameSetId,
            SHORT_NAME = name,
            // Common columns will be automatically set
        };
        
        var result = await _stratUnitService.CreateAsync(dto, userId);
        // ROW_CREATED_BY, ROW_CREATED_DATE, ACTIVE_IND, PPDM_GUID automatically set
    }
}
```

---

## Key Benefits

1. **Separation of Concerns**
   - Contracts (interfaces) separate from implementations
   - Easy to swap implementations
   - Clear boundaries

2. **Automatic Common Column Management**
   - No need to manually set audit columns
   - Consistent across all entities
   - Reduces errors

3. **Testability**
   - Easy to mock interfaces
   - Unit test business logic independently
   - Integration test data access

4. **Maintainability**
   - Changes to implementations don't affect consumers
   - Clear project structure
   - Easy to navigate

5. **Scalability**
   - Add new entities following the same pattern
   - Extend repositories and services easily
   - Support for multiple data sources

---

## Next Steps

1. ✅ Project structure created
2. ✅ Common column handler implemented
3. ✅ Base repository implemented
4. ✅ Stratigraphy repositories (StratUnit, StratColumn) implemented
5. ✅ Service layer started (StratUnitService)
6. ⏳ Complete remaining Stratigraphy repositories
7. ⏳ Complete remaining Stratigraphy services
8. ⏳ Add validation framework
9. ⏳ Add mapping utilities (AutoMapper or manual)
10. ⏳ Add unit tests
11. ⏳ Add integration tests
12. ⏳ Extend to other domains (Well, Production, etc.)

---

## File Count Summary

### Beep.OilandGas.PPDM39
- Models: 2600+ entity files
- Interfaces: ~50+ (growing)
- DTOs: ~50+ (growing)

### Beep.OilandGas.PPDM39.DataManagement
- Repository Implementations: ~50+ (growing)
- Service Implementations: ~50+ (growing)
- Base Classes: 2
- Common Handlers: 1

---

**Last Updated**: 2024  
**Version**: 1.0

