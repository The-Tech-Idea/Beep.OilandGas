# PPDMGenericRepository

## Overview

The `PPDMGenericRepository` is a non-generic repository implementation that provides CRUD operations for any PPDM entity without requiring specific repository classes for every table. It uses metadata to automatically determine table names, primary keys, and relationships.

## Key Features

- **Generic CRUD Operations**: Insert, Update, Delete, Get operations for any entity type
- **Metadata-Driven**: Automatically uses PPDM metadata for table names and primary keys
- **Parent-Child Relationships**: Built-in support for parent-child relationships through metadata
- **UnitOfWork Integration**: Creates UnitOfWork instances internally using UnitOfWorkFactory
- **Database-Agnostic**: Works with any data source through `IDMEEditor` abstraction
- **AppFilter Support**: All queries use `AppFilter` for data source independence

## Constructor

```csharp
public PPDMGenericRepository(
    IDMEEditor editor,
    ICommonColumnHandler commonColumnHandler,
    IPPDM39DefaultsRepository defaults,
    IPPDMMetadataRepository metadata,
    Type entityType,
    string connectionName = "PPDM39",
    string tableName = null,
    ILogger<PPDMGenericRepository>? logger = null)
```

**Parameters:**
- `editor`: IDMEEditor instance for data access
- `commonColumnHandler`: Handler for common PPDM columns
- `defaults`: PPDM39 defaults repository for ID generation
- `metadata`: PPDM metadata repository
- `entityType`: Type of entity this repository works with
- `connectionName`: Database connection name (default: "PPDM39")
- `tableName`: Optional table name override
- `logger`: Optional logger instance

## Key Properties

- `EntityType`: Gets the entity type this repository works with
- `TableName`: Gets the table name this repository works with
- `UnitOfWork`: Gets the UnitOfWork instance for this repository
- `CommonColumnHandler`: Gets the common column handler
- `Defaults`: Gets the PPDM39 defaults repository
- `Metadata`: Gets the metadata repository

## Core Methods

### Get Operations

#### GetByIdAsync

Gets a single entity by its primary key.

```csharp
public virtual async Task<object> GetByIdAsync(object id)
```

**Example:**
```csharp
var repository = new PPDMGenericRepository(editor, commonColumnHandler, defaults, metadata,
    typeof(WELL), connectionName: "PPDM39", tableName: "WELL");

var well = await repository.GetByIdAsync("WELL-001");
```

#### GetAsync

Gets entities using AppFilter conditions.

```csharp
public virtual async Task<IEnumerable<object>> GetAsync(List<AppFilter> filters)
```

**Example:**
```csharp
var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = "FIELD-001" },
    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
};

var wells = await repository.GetAsync(filters);
```

#### GetActiveAsync

Gets all active entities (ACTIVE_IND = 'Y').

```csharp
public virtual async Task<IEnumerable<object>> GetActiveAsync()
```

**Example:**
```csharp
var activeWells = await repository.GetActiveAsync();
```

### Insert Operations

#### InsertAsync

Inserts a new entity. Automatically sets common columns.

```csharp
public virtual async Task<object> InsertAsync(object entity, string userId)
```

**Example:**
```csharp
var well = new WELL
{
    WELL_ID = defaults.GenerateId("WELL"),
    WELL_NAME = "Test Well",
    FIELD_ID = "FIELD-001"
};

var inserted = await repository.InsertAsync(well, userId);
```

#### InsertBatchAsync

Inserts multiple entities in a batch.

```csharp
public virtual async Task<IEnumerable<object>> InsertBatchAsync(IEnumerable<object> entities, string userId)
```

**Example:**
```csharp
var wells = new List<WELL> { well1, well2, well3 };
var inserted = await repository.InsertBatchAsync(wells, userId);
```

### Update Operations

#### UpdateAsync

Updates an existing entity. Automatically updates common columns.

```csharp
public virtual async Task<object> UpdateAsync(object entity, string userId)
```

**Example:**
```csharp
well.WELL_NAME = "Updated Name";
var updated = await repository.UpdateAsync(well, userId);
```

#### UpdateBatchAsync

Updates multiple entities in a batch.

```csharp
public virtual async Task<IEnumerable<object>> UpdateBatchAsync(IEnumerable<object> entities, string userId)
```

### Delete Operations

#### DeleteAsync

Deletes an entity by its primary key.

```csharp
public virtual async Task<bool> DeleteAsync(object id, string userId)
```

**Example:**
```csharp
var deleted = await repository.DeleteAsync("WELL-001", userId);
```

## Parent-Child Relationships

### GetChildrenByParentKeyAsync

Gets child entities by parent primary key.

```csharp
public virtual async Task<IEnumerable<object>> GetChildrenByParentKeyAsync(
    string parentTableName, 
    object parentKey)
```

**Example:**
```csharp
// Get all wells for a field
var wells = await repository.GetChildrenByParentKeyAsync("FIELD", "FIELD-001");
```

### GetChildrenByParentEntityAsync

Gets child entities by parent entity instance.

```csharp
public virtual async Task<IEnumerable<object>> GetChildrenByParentEntityAsync(
    object parentEntity, 
    string parentTableName = null)
```

**Example:**
```csharp
var field = await fieldRepository.GetByIdAsync("FIELD-001");
var wells = await wellRepository.GetChildrenByParentEntityAsync(field);
```

### InsertWithParentKeysAsync

Inserts an entity and automatically sets parent foreign keys.

```csharp
public virtual async Task<object> InsertWithParentKeysAsync(
    object entity, 
    string userId, 
    Dictionary<string, object> parentKeys = null)
```

**Example:**
```csharp
var well = new WELL { WELL_NAME = "New Well" };
var parentKeys = new Dictionary<string, object> { { "FIELD_ID", "FIELD-001" } };
var inserted = await repository.InsertWithParentKeysAsync(well, userId, parentKeys);
```

## Query Building

### BuildAppFiltersFromSql

Converts SQL WHERE clause to AppFilter list.

```csharp
public virtual List<AppFilter> BuildAppFiltersFromSql(string sql)
```

**Example:**
```csharp
var sql = "WHERE FIELD_ID = 'FIELD-001' AND ACTIVE_IND = 'Y'";
var filters = repository.BuildAppFiltersFromSql(sql);
```

## Best Practices

### 1. Use Type-Safe Entities

```csharp
// ✅ Good - Use strongly-typed entities
var repository = new PPDMGenericRepository(editor, commonColumnHandler, defaults, metadata,
    typeof(WELL), connectionName: "PPDM39", tableName: "WELL");

// ❌ Bad - Don't use Dictionary or dynamic types
```

### 2. Always Provide User ID

```csharp
// ✅ Good - Always provide userId for audit trails
await repository.InsertAsync(well, userId);

// ❌ Bad - Missing userId breaks audit trail
await repository.InsertAsync(well, null);
```

### 3. Use AppFilter for Queries

```csharp
// ✅ Good - Use AppFilter for database-agnostic queries
var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = "FIELD-001" }
};
var wells = await repository.GetAsync(filters);

// ❌ Bad - Don't use raw SQL
```

### 4. Handle Parent-Child Relationships

```csharp
// ✅ Good - Use parent-child methods for relationships
var wells = await repository.GetChildrenByParentKeyAsync("FIELD", "FIELD-001");

// ❌ Bad - Manual foreign key queries
```

## Integration Points

- **CommonColumnHandler**: Automatically sets audit columns
- **PPDM39DefaultsRepository**: Generates IDs for new entities
- **PPDMMetadataRepository**: Provides table metadata
- **IDMEEditor**: Provides data access abstraction
- **UnitOfWorkFactory**: Creates UnitOfWork instances

## Related Documentation

- [Overview](beep-dataaccess-overview.md) - Framework overview
- [Common Columns](beep-dataaccess-common-columns.md) - Common column management
- [Usage Examples](beep-dataaccess-examples.md) - Practical examples
- [Patterns and Best Practices](beep-dataaccess-patterns.md) - Best practices

