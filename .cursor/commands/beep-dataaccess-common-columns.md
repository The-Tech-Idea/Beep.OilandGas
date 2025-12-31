# CommonColumnHandler

## Overview

The `CommonColumnHandler` manages common columns that exist across all PPDM entities. It automatically handles audit columns, metadata, and temporal tracking without requiring manual column management in each operation.

## Key Features

- **Automatic Audit Columns**: Sets creation and change audit columns
- **Active Indicator Management**: Manages ACTIVE_IND column
- **Temporal Tracking**: Handles effective and expiry dates
- **PPDM GUID Generation**: Generates unique identifiers

## Common Columns

All PPDM entities have these common columns:

### Audit Columns

- `ROW_CREATED_BY` (string): User who created the record
- `ROW_CREATED_DATE` (DateTime): Creation timestamp
- `ROW_CHANGED_BY` (string): User who last changed the record
- `ROW_CHANGED_DATE` (DateTime): Last change timestamp

### Status Columns

- `ACTIVE_IND` (string): Active indicator ('Y' or 'N')

### Temporal Columns

- `ROW_EFFECTIVE_DATE` (DateTime): Effective date for temporal tracking
- `ROW_EXPIRY_DATE` (DateTime): Expiry date for temporal tracking

### Identifier Columns

- `PPDM_GUID` (string): Unique PPDM identifier

## Key Methods

### SetCreatedColumns

Sets the created columns for a new entity.

```csharp
public void SetCreatedColumns(IPPDMEntity entity, string userId)
```

**Example:**
```csharp
var well = new WELL { WELL_ID = "WELL-001" };
commonColumnHandler.SetCreatedColumns(well, userId);
// Sets: ROW_CREATED_BY = userId, ROW_CREATED_DATE = DateTime.UtcNow
// Also sets: ROW_CHANGED_BY and ROW_CHANGED_DATE
```

### SetChangedColumns

Sets the changed columns for an updated entity.

```csharp
public void SetChangedColumns(IPPDMEntity entity, string userId)
```

**Example:**
```csharp
well.WELL_NAME = "Updated Name";
commonColumnHandler.SetChangedColumns(well, userId);
// Sets: ROW_CHANGED_BY = userId, ROW_CHANGED_DATE = DateTime.UtcNow
```

### SetActiveIndicator

Sets the active indicator.

```csharp
public void SetActiveIndicator(IPPDMEntity entity, bool isActive)
```

**Example:**
```csharp
// Activate entity
commonColumnHandler.SetActiveIndicator(well, true);
// Sets: ACTIVE_IND = "Y"

// Deactivate entity
commonColumnHandler.SetActiveIndicator(well, false);
// Sets: ACTIVE_IND = "N"
```

### SetRowEffectiveDates

Sets the effective and expiry dates for row-level temporal tracking.

```csharp
public void SetRowEffectiveDates(IPPDMEntity entity, DateTime? effectiveDate = null, DateTime? expiryDate = null)
```

**Example:**
```csharp
var effectiveDate = DateTime.UtcNow;
var expiryDate = DateTime.UtcNow.AddYears(10);
commonColumnHandler.SetRowEffectiveDates(well, effectiveDate, expiryDate);
// Sets: ROW_EFFECTIVE_DATE = effectiveDate, ROW_EXPIRY_DATE = expiryDate
```

### GeneratePPDMGuid

Generates a PPDM GUID if one doesn't exist.

```csharp
public void GeneratePPDMGuid(IPPDMEntity entity)
```

**Example:**
```csharp
var well = new WELL { WELL_ID = "WELL-001" };
commonColumnHandler.GeneratePPDMGuid(well);
// Sets: PPDM_GUID = Guid.NewGuid().ToString().ToUpper() (if not already set)
```

### PrepareForInsert

Prepares an entity for insertion by setting all common columns.

```csharp
public void PrepareForInsert(IPPDMEntity entity, string userId)
```

**Example:**
```csharp
var well = new WELL { WELL_ID = "WELL-001" };
commonColumnHandler.PrepareForInsert(well, userId);
// Sets: All creation columns, active indicator, effective dates, and GUID
```

### PrepareForUpdate

Prepares an entity for update by setting change columns.

```csharp
public void PrepareForUpdate(IPPDMEntity entity, string userId)
```

**Example:**
```csharp
well.WELL_NAME = "Updated Name";
commonColumnHandler.PrepareForUpdate(well, userId);
// Sets: ROW_CHANGED_BY and ROW_CHANGED_DATE
```

## Usage Patterns

### In Repository Operations

The `PPDMGenericRepository` automatically uses `CommonColumnHandler`:

```csharp
// Insert - automatically sets creation columns
var well = new WELL { WELL_ID = "WELL-001" };
await repository.InsertAsync(well, userId);
// CommonColumnHandler.SetCreatedColumns is called internally

// Update - automatically sets change columns
well.WELL_NAME = "Updated";
await repository.UpdateAsync(well, userId);
// CommonColumnHandler.SetChangedColumns is called internally
```

### Manual Usage

You can also use it manually:

```csharp
// Prepare entity before custom operations
var well = new WELL { WELL_ID = "WELL-001" };
commonColumnHandler.PrepareForInsert(well, userId);

// Perform custom insert logic
await customInsertLogic(well);
```

## Best Practices

### 1. Always Use CommonColumnHandler

```csharp
// ✅ Good - Use CommonColumnHandler for all entities
commonColumnHandler.PrepareForInsert(well, userId);

// ❌ Bad - Manual column setting
well.ROW_CREATED_BY = userId;
well.ROW_CREATED_DATE = DateTime.UtcNow;
// Missing other columns and consistency
```

### 2. Provide Valid User ID

```csharp
// ✅ Good - Always provide valid userId
commonColumnHandler.SetCreatedColumns(well, userId);

// ❌ Bad - Empty or null userId
commonColumnHandler.SetCreatedColumns(well, "");
// Breaks audit trail
```

### 3. Use Prepare Methods

```csharp
// ✅ Good - Use PrepareForInsert/PrepareForUpdate
commonColumnHandler.PrepareForInsert(well, userId);

// ❌ Bad - Setting columns individually
commonColumnHandler.SetCreatedColumns(well, userId);
commonColumnHandler.SetActiveIndicator(well, true);
commonColumnHandler.GeneratePPDMGuid(well);
// More verbose and error-prone
```

## Integration Points

- **PPDMGenericRepository**: Automatically uses CommonColumnHandler
- **All PPDM Entities**: Must implement `IPPDMEntity` interface
- **Audit Services**: Uses common columns for audit trails
- **Versioning Services**: Uses common columns for version tracking

## Related Documentation

- [Overview](beep-dataaccess-overview.md) - Framework overview
- [Generic Repository](beep-dataaccess-generic-repository.md) - Repository usage
- [Usage Examples](beep-dataaccess-examples.md) - Practical examples
- [Patterns and Best Practices](beep-dataaccess-patterns.md) - Best practices

