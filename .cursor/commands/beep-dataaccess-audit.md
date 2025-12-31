# Data Access Audit Service

## Overview

The `PPDMDataAccessAuditService` tracks data access for compliance and auditing, recording who accessed what data and when.

## Key Features

- **Access Event Recording**: Records access events (Read, Write, Delete, Export)
- **Access History**: Access history by entity and by user
- **Compliance Reporting**: Access statistics for compliance
- **Real-Time Tracking**: Real-time access tracking

## Key Methods

### RecordAccessAsync

Records a data access event.

```csharp
public async Task RecordAccessAsync(DataAccessEvent accessEvent)
```

**Example:**
```csharp
var auditService = new PPDMDataAccessAuditService(editor, commonColumnHandler, defaults, metadata);

var accessEvent = new DataAccessEvent
{
    EventId = Guid.NewGuid().ToString(),
    UserId = userId,
    TableName = "WELL",
    EntityId = "WELL-001",
    AccessType = AccessType.Read,
    Timestamp = DateTime.UtcNow
};

await auditService.RecordAccessAsync(accessEvent);
```

### GetAccessHistoryByEntityAsync

Gets access history for an entity.

```csharp
public async Task<List<DataAccessEvent>> GetAccessHistoryByEntityAsync(string tableName, object entityId, DateTime? startDate = null, DateTime? endDate = null)
```

**Example:**
```csharp
var history = await auditService.GetAccessHistoryByEntityAsync("WELL", "WELL-001", startDate, endDate);
foreach (var access in history)
{
    Console.WriteLine($"{access.Timestamp}: {access.UserId} - {access.AccessType}");
}
```

### GetAccessHistoryByUserAsync

Gets access history for a user.

```csharp
public async Task<List<DataAccessEvent>> GetAccessHistoryByUserAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
```

**Example:**
```csharp
var userHistory = await auditService.GetAccessHistoryByUserAsync(userId, startDate, endDate);
Console.WriteLine($"User accessed {userHistory.Count} entities");
```

## Access Types

- `Read`: Entity was read
- `Write`: Entity was created or updated
- `Delete`: Entity was deleted
- `Export`: Entity data was exported

## Best Practices

### 1. Record All Access Events

```csharp
// ✅ Good - Record all access events
await auditService.RecordAccessAsync(accessEvent);

// ❌ Bad - Missing audit trail
// No audit recording
```

### 2. Use Access History for Compliance

```csharp
// ✅ Good - Use access history for compliance reporting
var history = await auditService.GetAccessHistoryByEntityAsync("WELL", "WELL-001");
// Generate compliance report from history
```

## Related Documentation

- [Overview](beep-dataaccess-overview.md) - Framework overview
- [Data Versioning](beep-dataaccess-versioning.md) - Versioning service
- [Usage Examples](beep-dataaccess-examples.md) - Practical examples

