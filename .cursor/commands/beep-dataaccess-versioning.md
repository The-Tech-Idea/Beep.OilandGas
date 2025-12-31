# Data Versioning Service

## Overview

The `PPDMDataVersioningService` tracks entity versions and changes over time, providing versioning capabilities for audit and rollback operations.

## Key Features

- **Version Snapshots**: Creates version snapshots of entities
- **Version Comparison**: Compares different versions
- **Rollback Capabilities**: Rollback to previous versions
- **Version History Tracking**: Tracks complete version history

## Key Methods

### CreateVersionAsync

Creates a version snapshot of an entity.

```csharp
public async Task<VersionSnapshot> CreateVersionAsync(string tableName, object entity, string userId, string versionLabel = null)
```

**Example:**
```csharp
var versioningService = new PPDMDataVersioningService(editor, commonColumnHandler, defaults, metadata);
var snapshot = await versioningService.CreateVersionAsync("WELL", well, userId, "Initial Version");
Console.WriteLine($"Version {snapshot.VersionNumber} created");
```

### GetVersionsAsync

Gets all versions for an entity.

```csharp
public async Task<List<VersionSnapshot>> GetVersionsAsync(string tableName, object entityId)
```

**Example:**
```csharp
var versions = await versioningService.GetVersionsAsync("WELL", "WELL-001");
foreach (var version in versions)
{
    Console.WriteLine($"Version {version.VersionNumber}: {version.VersionLabel} - {version.CreatedDate}");
}
```

### CompareVersionsAsync

Compares two versions.

```csharp
public async Task<VersionComparisonResult> CompareVersionsAsync(string tableName, object entityId, int version1, int version2)
```

**Example:**
```csharp
var comparison = await versioningService.CompareVersionsAsync("WELL", "WELL-001", 1, 2);
foreach (var change in comparison.Changes)
{
    Console.WriteLine($"{change.FieldName}: {change.OldValue} -> {change.NewValue}");
}
```

### RollbackToVersionAsync

Rolls back to a previous version.

```csharp
public async Task<object> RollbackToVersionAsync(string tableName, object entityId, int versionNumber, string userId)
```

**Example:**
```csharp
var rolledBack = await versioningService.RollbackToVersionAsync("WELL", "WELL-001", 1, userId);
Console.WriteLine("Entity rolled back to version 1");
```

## Best Practices

### 1. Create Versions Before Major Changes

```csharp
// ✅ Good - Create version before major changes
await versioningService.CreateVersionAsync("WELL", well, userId, "Before major update");
well.WELL_NAME = "Updated Name";
await repository.UpdateAsync(well, userId);
```

### 2. Use Version Labels

```csharp
// ✅ Good - Use descriptive version labels
await versioningService.CreateVersionAsync("WELL", well, userId, "Before ownership change");
```

## Related Documentation

- [Overview](beep-dataaccess-overview.md) - Framework overview
- [Data Access Audit](beep-dataaccess-audit.md) - Audit service
- [Usage Examples](beep-dataaccess-examples.md) - Practical examples

