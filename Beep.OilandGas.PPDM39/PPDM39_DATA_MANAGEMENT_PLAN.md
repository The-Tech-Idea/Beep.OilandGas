# PPDM 3.9 Data Management Architecture Plan

## Executive Summary

This document outlines a comprehensive data management architecture for PPDM 3.9 using the UnitOfWork pattern. The architecture is designed to be scalable, maintainable, and suitable for oil and gas companies managing complex geological and operational data.

## Project Structure Hierarchy

```
Beep.OilandGas.PPDM39/
├── Models/                          # Existing PPDM 3.9 Entity Models (2600+ entities)
│   ├── Stratigraphy/                # Stratigraphy-related entities
│   │   ├── STRAT_UNIT.cs
│   │   ├── STRAT_COLUMN.cs
│   │   ├── STRAT_HIERARCHY.cs
│   │   ├── STRAT_WELL_SECTION.cs
│   │   └── ... (78 stratigraphy entities)
│   └── ... (other domain models)
│
├── Core/                             # Core Infrastructure
│   ├── Interfaces/
│   │   ├── IPPDMEntity.cs           # Base interface for all PPDM entities
│   │   ├── IPPDMRepository.cs      # Generic repository interface
│   │   ├── IPPDMService.cs         # Generic service interface
│   │   └── ICommonColumnHandler.cs  # Common column management
│   │
│   ├── Base/
│   │   ├── PPDMEntityBase.cs       # Base class with common columns
│   │   ├── PPDMRepositoryBase.cs   # Base repository implementation
│   │   └── PPDMServiceBase.cs      # Base service implementation
│   │
│   └── Common/
│       ├── CommonColumnHandler.cs   # Handles ACTIVE_IND, ROW_*, etc.
│       ├── PPDMConstants.cs         # Constants and enums
│       └── PPDMHelpers.cs          # Helper utilities
│
├── Repositories/                     # Data Access Layer
│   ├── Stratigraphy/
│   │   ├── IStratigraphyRepository.cs
│   │   ├── StratigraphyRepository.cs
│   │   │
│   │   ├── StratUnit/
│   │   │   ├── IStratUnitRepository.cs
│   │   │   └── StratUnitRepository.cs
│   │   │
│   │   ├── StratColumn/
│   │   │   ├── IStratColumnRepository.cs
│   │   │   └── StratColumnRepository.cs
│   │   │
│   │   ├── StratHierarchy/
│   │   │   ├── IStratHierarchyRepository.cs
│   │   │   └── StratHierarchyRepository.cs
│   │   │
│   │   ├── StratWellSection/
│   │   │   ├── IStratWellSectionRepository.cs
│   │   │   └── StratWellSectionRepository.cs
│   │   │
│   │   └── ... (other stratigraphy repositories)
│   │
│   └── ... (other domain repositories)
│
├── Services/                         # Business Logic Layer
│   ├── Stratigraphy/
│   │   ├── IStratigraphyService.cs
│   │   ├── StratigraphyService.cs
│   │   │
│   │   ├── StratUnit/
│   │   │   ├── IStratUnitService.cs
│   │   │   └── StratUnitService.cs
│   │   │
│   │   ├── StratColumn/
│   │   │   ├── IStratColumnService.cs
│   │   │   └── StratColumnService.cs
│   │   │
│   │   └── ... (other stratigraphy services)
│   │
│   └── ... (other domain services)
│
├── Validators/                       # Validation Layer
│   ├── Stratigraphy/
│   │   ├── StratUnitValidator.cs
│   │   ├── StratColumnValidator.cs
│   │   └── ... (other validators)
│   │
│   └── ... (other domain validators)
│
└── DTOs/                             # Data Transfer Objects
    ├── Stratigraphy/
    │   ├── StratUnitDto.cs
    │   ├── StratColumnDto.cs
    │   └── ... (other DTOs)
    │
    └── ... (other domain DTOs)
```

## Common Columns Pattern

All PPDM entities share common audit and metadata columns that must be handled consistently:

### Standard Common Columns:
- `ACTIVE_IND` (String) - Active indicator ('Y'/'N')
- `ROW_CREATED_BY` (String) - User who created the row
- `ROW_CREATED_DATE` (DateTime) - Creation timestamp
- `ROW_CHANGED_BY` (String) - User who last changed the row
- `ROW_CHANGED_DATE` (DateTime) - Last change timestamp
- `ROW_EFFECTIVE_DATE` (DateTime) - Effective date
- `ROW_EXPIRY_DATE` (DateTime) - Expiry date
- `ROW_QUALITY` (String) - Data quality indicator
- `PPDM_GUID` (String) - Unique identifier
- `AREA_ID` (String) - Area identifier
- `AREA_TYPE` (String) - Area type
- `BUSINESS_ASSOCIATE_ID` (String) - Business associate reference
- `EFFECTIVE_DATE` (DateTime) - Business effective date
- `EXPIRY_DATE` (DateTime) - Business expiry date
- `SOURCE` (String) - Data source
- `REMARK` (String) - Remarks/notes

## Stratigraphy Domain Structure

### Core Stratigraphy Entities Hierarchy:

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
├── Stratigraphic Topographic Relations (STRAT_TOPO_RELATION)
│
└── Reference Data (R_STRAT_*, RA_STRAT_*)
    ├── Reference Types
    ├── Reference Qualifiers
    └── Reference Status Codes
```

## Implementation Phases

### Phase 1: Foundation (Week 1-2)
1. **Common Column Handler**
   - Create `ICommonColumnHandler` interface
   - Implement `CommonColumnHandler` class
   - Handle automatic population of common columns
   - Support for audit trail

2. **Base Infrastructure**
   - Create `IPPDMEntity` interface
   - Create `PPDMEntityBase` abstract class
   - Create `IPPDMRepository<T>` generic interface
   - Create `PPDMRepositoryBase<T>` base class

3. **UnitOfWork Integration**
   - Integrate with existing UnitOfWorkManager
   - Create transaction management
   - Implement change tracking

### Phase 2: Stratigraphy Core (Week 3-4)
1. **Stratigraphic Unit Implementation**
   - Repository: `StratUnitRepository`
   - Service: `StratUnitService`
   - Validator: `StratUnitValidator`
   - DTO: `StratUnitDto`

2. **Stratigraphic Column Implementation**
   - Repository: `StratColumnRepository`
   - Service: `StratColumnService`
   - Validator: `StratColumnValidator`
   - DTO: `StratColumnDto`

3. **Stratigraphic Hierarchy Implementation**
   - Repository: `StratHierarchyRepository`
   - Service: `StratHierarchyService`
   - Support for parent-child relationships

### Phase 3: Stratigraphy Extended (Week 5-6)
1. **Well Stratigraphy**
   - `StratWellSectionRepository` and Service
   - Integration with Well entities
   - Depth-based queries

2. **Field Stratigraphy**
   - `StratFieldSectionRepository` and Service
   - Geographic integration

3. **Stratigraphic Relationships**
   - Equivalence management
   - Correlation handling
   - Topographic relations

### Phase 4: Advanced Features (Week 7-8)
1. **Query Builders**
   - Complex stratigraphic queries
   - Depth interval queries
   - Age-based queries
   - Hierarchy traversal

2. **Validation Framework**
   - Business rule validation
   - Cross-entity validation
   - Data quality checks

3. **Reporting & Analytics**
   - Stratigraphic column generation
   - Well section reports
   - Correlation reports

## Detailed Stratigraphy Implementation Plan

### 1. Common Column Handler

**Purpose**: Automatically manage common columns across all PPDM entities.

**Features**:
- Auto-populate `ROW_CREATED_BY`, `ROW_CREATED_DATE` on insert
- Auto-populate `ROW_CHANGED_BY`, `ROW_CHANGED_DATE` on update
- Manage `ACTIVE_IND` flag
- Handle `ROW_EFFECTIVE_DATE` and `ROW_EXPIRY_DATE`
- Generate `PPDM_GUID` if missing
- Track data quality in `ROW_QUALITY`

**Implementation**:
```csharp
public interface ICommonColumnHandler
{
    void SetCreatedColumns(IPPDMEntity entity, string userId);
    void SetChangedColumns(IPPDMEntity entity, string userId);
    void SetActiveIndicator(IPPDMEntity entity, bool isActive);
    void SetEffectiveDates(IPPDMEntity entity, DateTime? effectiveDate, DateTime? expiryDate);
    void GeneratePPDMGuid(IPPDMEntity entity);
    void SetDataQuality(IPPDMEntity entity, string qualityCode);
}
```

### 2. Stratigraphic Unit Repository

**Purpose**: Manage stratigraphic unit data with full CRUD operations.

**Key Methods**:
- `GetByStratNameSetId(string stratNameSetId)`
- `GetByAreaId(string areaId)`
- `GetActiveUnits()`
- `GetByStratType(string stratType)`
- `GetHierarchy(string stratUnitId)` - Get parent/child relationships
- `SearchByName(string name)`
- `GetByAgeRange(DateTime? fromAge, DateTime? toAge)`

**Business Rules**:
- Each stratigraphic unit must belong to a stratigraphic name set
- Units can have parent-child relationships via STRAT_HIERARCHY
- Only one unit per name set can have PREFERRED_IND = 'Y'
- Active units (ACTIVE_IND = 'Y') are preferred for queries

### 3. Stratigraphic Column Repository

**Purpose**: Manage stratigraphic columns and their units.

**Key Methods**:
- `GetColumnWithUnits(string stratColumnId)`
- `GetColumnsByArea(string areaId)`
- `GetColumnUnits(string stratColumnId)`
- `GetColumnAgeRange(string stratColumnId)`
- `BuildStratColumn(string stratColumnId)` - Build complete column with hierarchy

**Business Rules**:
- Columns contain ordered units (STRAT_COLUMN_UNIT)
- Units have depth/age relationships
- Columns can reference other columns (STRAT_COLUMN_XREF)

### 4. Stratigraphic Hierarchy Repository

**Purpose**: Manage parent-child relationships between stratigraphic units.

**Key Methods**:
- `GetParentUnits(string childUnitId)`
- `GetChildUnits(string parentUnitId)`
- `GetAncestors(string unitId)` - Get all ancestors
- `GetDescendants(string unitId)` - Get all descendants
- `GetHierarchyTree(string rootUnitId)` - Get complete tree
- `ValidateHierarchy(string parentId, string childId)` - Check for cycles

**Business Rules**:
- Prevent circular references
- Maintain hierarchy integrity
- Support multiple parent relationships (if allowed)

### 5. Well Stratigraphy Repository

**Purpose**: Manage stratigraphic interpretations for wells.

**Key Methods**:
- `GetWellSections(string uwi)` - Get all sections for a well
- `GetSectionByDepth(string uwi, decimal depth)`
- `GetSectionsByInterval(string uwi, decimal topDepth, decimal baseDepth)`
- `GetStratUnitsForWell(string uwi)`
- `CreateWellSection(string uwi, string stratUnitId, decimal topDepth, decimal baseDepth)`
- `UpdateWellSection(string uwi, string interpId, decimal? topDepth, decimal? baseDepth)`

**Business Rules**:
- Sections must not overlap (or handle overlaps appropriately)
- Depth intervals must be valid (top < base)
- Sections must reference valid stratigraphic units
- Support multiple interpretations per well

## Service Layer Responsibilities

### StratigraphyService

**Purpose**: Orchestrate stratigraphy operations and enforce business rules.

**Key Operations**:
1. **Unit Management**
   - Create/Update/Delete stratigraphic units
   - Validate unit relationships
   - Manage unit hierarchies

2. **Column Management**
   - Build complete stratigraphic columns
   - Validate column structure
   - Manage column references

3. **Well Integration**
   - Create well sections from interpretations
   - Validate depth intervals
   - Generate well stratigraphic reports

4. **Data Quality**
   - Validate stratigraphic data
   - Check for inconsistencies
   - Generate quality reports

## Data Access Patterns

### Repository Pattern
- Each entity type has a dedicated repository
- Repositories inherit from `PPDMRepositoryBase<T>`
- Use UnitOfWork for transaction management
- Support async operations

### Service Pattern
- Services orchestrate multiple repositories
- Enforce business rules
- Handle complex operations
- Provide high-level API

### UnitOfWork Pattern
- All database operations go through UnitOfWork
- Support transactions
- Track changes
- Batch operations

## Validation Strategy

### Entity-Level Validation
- Required fields
- Data type validation
- Range validation
- Format validation

### Business Rule Validation
- Cross-entity validation
- Relationship validation
- Hierarchy validation
- Temporal validation (effective/expiry dates)

### Data Quality Validation
- Completeness checks
- Consistency checks
- Accuracy checks
- Timeliness checks

## Performance Considerations

1. **Caching**
   - Cache reference data (R_STRAT_* tables)
   - Cache frequently accessed units
   - Cache hierarchy trees

2. **Indexing**
   - Index on primary keys
   - Index on foreign keys
   - Index on ACTIVE_IND
   - Index on AREA_ID
   - Index on depth/age columns

3. **Query Optimization**
   - Use projection for large result sets
   - Implement pagination
   - Use compiled queries for repeated operations

4. **Batch Operations**
   - Support bulk inserts
   - Support bulk updates
   - Support bulk deletes

## Security Considerations

1. **Audit Trail**
   - Track all changes via ROW_CHANGED_BY/DATE
   - Maintain change history
   - Support rollback

2. **Access Control**
   - User-based access control
   - Area-based access control
   - Role-based access control

3. **Data Quality**
   - Track data quality indicators
   - Support data quality workflows
   - Flag suspect data

## Testing Strategy

1. **Unit Tests**
   - Repository tests
   - Service tests
   - Validator tests
   - Common column handler tests

2. **Integration Tests**
   - Database integration
   - End-to-end workflows
   - Transaction tests

3. **Performance Tests**
   - Load testing
   - Stress testing
   - Query performance

## Migration Path

1. **Phase 1**: Implement foundation (Common columns, base classes)
2. **Phase 2**: Implement Stratigraphic Unit (proof of concept)
3. **Phase 3**: Implement remaining Stratigraphy entities
4. **Phase 4**: Extend to other domains (Well, Production, etc.)
5. **Phase 5**: Add advanced features (Reporting, Analytics)

## Success Criteria

- ✅ All common columns handled automatically
- ✅ Full CRUD operations for Stratigraphy entities
- ✅ Business rules enforced
- ✅ Validation in place
- ✅ Performance targets met
- ✅ Comprehensive test coverage
- ✅ Documentation complete

## Next Steps

1. Review and approve this plan
2. Set up project structure
3. Implement Phase 1 (Foundation)
4. Implement Phase 2 (Stratigraphy Core)
5. Iterate and refine

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Author**: Beep.PPDM39 Development Team

