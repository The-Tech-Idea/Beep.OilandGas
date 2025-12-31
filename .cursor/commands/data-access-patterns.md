# Data Access Patterns

## Overview

This document outlines best practices for data access in the Beep.OilandGas system, focusing on PPDM39 database operations using IDMEEditor, IDataSource, and PPDMGenericRepository.

## Core Principles

### 1. Use GetEntityAsync with AppFilter (PREFERRED)

**For simple single-table queries** - Clean, maintainable, handles delimiters automatically.

```csharp
// BEST: Use GetEntityAsync with AppFilter
var dataSource = _editor.GetDataSource(connectionName);
var filters = new List<AppFilter> 
{ 
    new AppFilter { FieldName = "ID", Operator = "=", FilterValue = id } 
};
var results = await dataSource.GetEntityAsync("TABLE", filters); // Returns IEnumerable<object>
```

### 2. Use GetScalar/GetScalarAsync for Scalar Queries

**For COUNT, SUM, EXISTS checks** - Returns double, use for single-value queries.

```csharp
// Good: Use GetScalar for scalar queries
var paramDelim = dataSource.ParameterDelimiter;
var sql = $"SELECT COUNT(*) FROM TABLE WHERE ID = {paramDelim}id";
var count = dataSource.GetScalar(sql); // Returns double
```

### 3. Use RunQuery for Complex Multi-Table Queries

**Only when GetEntityAsync is insufficient** - Returns IEnumerable<object>.

```csharp
// Use RunQuery only for complex multi-table queries or custom SQL
var sql2 = $"SELECT t1.*, t2.name FROM TABLE1 t1 JOIN TABLE2 t2 ON t1.id = t2.id";
var results2 = dataSource.RunQuery(sql2); // Returns IEnumerable<object>
```

### 4. Use ExecuteSql ONLY for DDL

**CREATE, ALTER, DROP statements** - DDL doesn't return data.

```csharp
// Good: ExecuteSql ONLY for DDL
var createSql = "CREATE TABLE ...";
dataSource.ExecuteSql(createSql); // CORRECT - DDL doesn't return data
```

## Anti-Patterns (What NOT to Do)

```csharp
// Bad: Using ExecuteSql for SELECT queries
var sql = "SELECT * FROM TABLE WHERE ID = @id";
dataSource.ExecuteSql(sql); // WRONG - ExecuteSql is for DDL only

// Bad: Using GetDataTable
var dataTable = dataSource.GetDataTable(sql); // WRONG - should use RunQuery

// Bad: Hardcoding parameter delimiters
var sql = "SELECT * FROM TABLE WHERE ID = @id"; // WRONG - use ParameterDelimiter
```

## PPDMGenericRepository Pattern

**Use PPDMGenericRepository when possible** - It handles delimiters automatically and uses GetEntityAsync internally.

```csharp
// Get table metadata
var metadata = await _metadata.GetTableMetadataAsync("TABLE_NAME");
var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");

// Create repository
var repo = new PPDMGenericRepository(
    _editor, 
    _commonColumnHandler, 
    _defaults, 
    _metadata,
    entityType, 
    _connectionName, 
    "TABLE_NAME");

// CRUD operations
var entities = await repo.GetAsync(filters);
var entity = await repo.GetByIdAsync(id);
await repo.InsertAsync(entity, userId);
await repo.UpdateAsync(entity, userId);
await repo.SoftDeleteAsync(id, userId);
```

## Entity vs DTO Pattern

### Key Principle

- **Entity objects** are passed directly to `IDataSource.InsertEntity(tableName, entity)` and `IDataSource.UpdateEntity(tableName, entity)`
- **Entity objects** are returned directly from `IDataSource.GetEntityAsync(tableName, filters)`
- **NO Dictionary<string, object> conversions** - remove all `Convert*ToDictionary` and `ConvertDictionaryTo*` methods
- **Use DTOs for API input/output**, convert DTOs ↔ Entity for database operations

### Example: DTO to Entity Conversion

```csharp
// DTO for API input
public class CreateWellRequest
{
    public string WellName { get; set; }
    public string FieldId { get; set; }
}

// Convert DTO to Entity for database operation
var entity = new WELL
{
    WELL_ID = _defaults.FormatIdForTable("WELL", Guid.NewGuid().ToString()),
    WELL_NAME = request.WellName,
    FIELD_ID = _defaults.FormatIdForTable("FIELD", request.FieldId),
    ACTIVE_IND = "Y",
    ROW_CREATED_BY = userId,
    ROW_CREATED_DATE = DateTime.UtcNow
};

// Insert entity directly
await repo.InsertAsync(entity, userId);
```

### Example: Entity to DTO Conversion

```csharp
// Get entity from database
var entity = await repo.GetByIdAsync(id);

// Convert entity to DTO for API response
var response = new WellResponse
{
    WellId = entity.WELL_ID,
    WellName = entity.WELL_NAME,
    FieldId = entity.FIELD_ID
};
```

## AppFilter Usage

### Basic Filtering

```csharp
var filters = new List<AppFilter>
{
    new AppFilter 
    { 
        FieldName = "FIELD_ID", 
        FilterValue = fieldId, 
        Operator = "=" 
    },
    new AppFilter 
    { 
        FieldName = "ACTIVE_IND", 
        FilterValue = "Y", 
        Operator = "=" 
    }
};

var results = await dataSource.GetEntityAsync("WELL", filters);
```

### Complex Filtering

```csharp
var filters = new List<AppFilter>
{
    new AppFilter 
    { 
        FieldName = "PRODUCTION_DATE", 
        FilterValue = startDate, 
        Operator = ">=" 
    },
    new AppFilter 
    { 
        FieldName = "PRODUCTION_DATE", 
        FilterValue = endDate, 
        Operator = "<=" 
    },
    new AppFilter 
    { 
        FieldName = "OIL_VOLUME", 
        FilterValue = 0, 
        Operator = ">" 
    }
};
```

## QueryBuilder for Custom Queries

**Use QueryBuilder for parameterized queries** - Provides database-agnostic query construction (only when needed).

```csharp
// Better: Use QueryBuilder for parameterized queries
var sql = QueryBuilder.BuildSelectQuery(
    dataSource, 
    "TABLE", 
    null, 
    new Dictionary<string, object> { { "ID", id } }, 
    out var parameters);
var results = dataSource.RunQuery(sql);
```

## Accessing IDataSource

**Access IDataSource via IDMEEditor**: `_editor.GetDataSource(connectionName)`

```csharp
var dataSource = _editor.GetDataSource(connectionName);
var paramDelim = dataSource.ParameterDelimiter;
var colDelim = dataSource.ColumnDelimiter;
```

## Column Delimiters

**Use IDataSource.ColumnDelimiter for column names if needed** - Handled automatically by GetEntityAsync.

```csharp
// Usually not needed with GetEntityAsync
var colDelim = dataSource.ColumnDelimiter; // e.g., "[" for SQL Server, "" for PostgreSQL
```

## Field-Scoped Queries Pattern

**For field-scoped operations** - Always include FIELD_ID filter.

```csharp
// In field-scoped service methods
public async Task<List<object>> GetEntitiesForFieldAsync(string fieldId, List<AppFilter> additionalFilters = null)
{
    var filters = new List<AppFilter>
    {
        new AppFilter 
        { 
            FieldName = "FIELD_ID", 
            FilterValue = _defaults.FormatIdForTable("TABLE_NAME", fieldId), 
            Operator = "=" 
        }
    };
    
    if (additionalFilters != null)
        filters.AddRange(additionalFilters);
        
    var repo = new PPDMGenericRepository(...);
    return await repo.GetAsync(filters);
}
```

## Key Principles Summary

1. **Use GetEntityAsync with AppFilter** for simple single-table queries - PREFERRED
2. **Use GetScalar/GetScalarAsync** for scalar queries - Returns double
3. **Use RunQuery** for complex multi-table queries - Only when GetEntityAsync is insufficient
4. **Use ExecuteSql ONLY for DDL** - CREATE, ALTER, DROP statements
5. **Never hardcode @ in SQL queries** - Always use IDataSource.ParameterDelimiter (not needed with GetEntityAsync + AppFilter)
6. **Use PPDMGenericRepository when possible** - It handles delimiters automatically and uses GetEntityAsync internally
7. **Use QueryBuilder for custom queries** - Provides database-agnostic query construction (only when needed)
8. **Access IDataSource via IDMEEditor**: `_editor.GetDataSource(connectionName)`
9. **Column delimiters**: Use IDataSource.ColumnDelimiter for column names if needed (handled automatically by GetEntityAsync)
10. **Entity objects** are passed directly - NO Dictionary conversions
11. **Use DTOs for API input/output** - Convert DTOs ↔ Entity for database operations

## References

- See `ARCHITECTURE.md` for system architecture
- See `oil_field_lifecycle_implementation_expansion_461a75e4.plan.md` for FieldOrchestrator patterns
- See `DATA_MANAGEMENT_SERVICES.md` for repository patterns

