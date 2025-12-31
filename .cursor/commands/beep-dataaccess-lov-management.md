# LOV Management Service

## Overview

The `LOVManagementService` provides CRUD operations and querying capabilities for the `LIST_OF_VALUE` table, managing List of Values (LOV) data used throughout PPDM39.

## Key Features

- **LOV Retrieval**: Get LOVs by type, category, or value
- **LOV Creation**: Create new LOV entries
- **LOV Updates**: Update existing LOV entries
- **LOV Deletion**: Soft delete LOV entries
- **Active LOV Filtering**: Filter by active indicator

## Key Methods

### GetLOVByTypeAsync

Gets LOVs by value type.

```csharp
public async Task<List<ListOfValueDto>> GetLOVByTypeAsync(string valueType, string? category = null, string connectionName = null)
```

**Example:**
```csharp
var lovService = new LOVManagementService(editor, commonColumnHandler, defaults, metadata);
var wellTypes = await lovService.GetLOVByTypeAsync("WELL_TYPE");
foreach (var lov in wellTypes)
{
    Console.WriteLine($"{lov.Value}: {lov.Description}");
}
```

### CreateLOVAsync

Creates a new LOV entry.

```csharp
public async Task<LIST_OF_VALUE> CreateLOVAsync(CreateLOVRequest request, string userId)
```

**Example:**
```csharp
var request = new CreateLOVRequest
{
    ValueType = "WELL_TYPE",
    Value = "HORIZONTAL",
    Description = "Horizontal Well",
    Category = "WELL_CLASSIFICATION"
};

var lov = await lovService.CreateLOVAsync(request, userId);
```

### UpdateLOVAsync

Updates an existing LOV entry.

```csharp
public async Task<LIST_OF_VALUE> UpdateLOVAsync(string lovId, UpdateLOVRequest request, string userId)
```

### DeleteLOVAsync

Soft deletes a LOV entry.

```csharp
public async Task<bool> DeleteLOVAsync(string lovId, string userId)
```

## Related Documentation

- [Overview](beep-dataaccess-overview.md) - Framework overview
- [Usage Examples](beep-dataaccess-examples.md) - Practical examples

