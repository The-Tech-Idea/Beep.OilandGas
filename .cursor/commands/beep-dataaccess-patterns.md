# Patterns and Best Practices

## Overview

This document outlines common patterns and best practices for using the PPDM39 Data Access Framework.

## Repository Patterns

### 1. Use Generic Repository for Most Operations

```csharp
// ✅ Good - Use GenericRepository for standard CRUD
var repository = new PPDMGenericRepository(editor, commonColumnHandler, defaults, metadata,
    typeof(WELL), connectionName: "PPDM39", tableName: "WELL");

// ❌ Bad - Creating specific repository for every table
public class WellRepository { ... }
public class FieldRepository { ... }
// Too many classes, maintenance overhead
```

### 2. Always Use AppFilter for Queries

```csharp
// ✅ Good - Database-agnostic queries
var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = "FIELD-001" }
};
var wells = await repository.GetAsync(filters);

// ❌ Bad - Raw SQL breaks database independence
var sql = "SELECT * FROM WELL WHERE FIELD_ID = 'FIELD-001'";
```

### 3. Use Parent-Child Methods for Relationships

```csharp
// ✅ Good - Use relationship methods
var wells = await repository.GetChildrenByParentKeyAsync("FIELD", "FIELD-001");

// ❌ Bad - Manual foreign key queries
var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = "FIELD-001" }
};
```

## Service Patterns

### 1. Validate Before Database Operations

```csharp
// ✅ Good - Validate first
var result = await validationService.ValidateAsync(well, "WELL");
if (result.IsValid)
{
    await repository.InsertAsync(well, userId);
}

// ❌ Bad - Insert without validation
await repository.InsertAsync(well, userId);
```

### 2. Use CommonColumnHandler for All Entities

```csharp
// ✅ Good - Use CommonColumnHandler
commonColumnHandler.PrepareForInsert(well, userId);

// ❌ Bad - Manual column setting
well.ROW_CREATED_BY = userId;
well.ROW_CREATED_DATE = DateTime.UtcNow;
// Missing other columns
```

## Error Handling Patterns

### 1. Comprehensive Error Handling

```csharp
// ✅ Good - Try-catch with logging
try
{
    var result = await repository.InsertAsync(well, userId);
    return result;
}
catch (ArgumentException ex)
{
    _logger.LogWarning(ex, "Invalid parameters");
    throw;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error");
    throw;
}
```

## Performance Patterns

### 1. Use Batch Operations for Multiple Records

```csharp
// ✅ Good - Batch operations
var wells = new List<WELL> { well1, well2, well3 };
await repository.InsertBatchAsync(wells, userId);

// ❌ Bad - Individual operations
foreach (var well in wells)
{
    await repository.InsertAsync(well, userId);
}
```

## Related Documentation

- [Overview](beep-dataaccess-overview.md) - Framework overview
- [Usage Examples](beep-dataaccess-examples.md) - Practical examples

