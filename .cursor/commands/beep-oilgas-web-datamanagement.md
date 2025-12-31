# Beep.OilandGas.Web Data Management Documentation

## Overview

The Blazor web application provides comprehensive data management capabilities through integration with the API service. All data operations, including CRUD, validation, quality monitoring, versioning, and auditing, are performed through the API service.

## Data Management Service

### IDataManagementService Interface

The `IDataManagementService` provides a centralized interface for all data management operations.

#### Connection Management

```csharp
Task<string?> GetCurrentConnectionNameAsync();
Task<SetCurrentDatabaseResult> SetCurrentConnectionAsync(string connectionName);
Task<List<DatabaseConnectionListItem>> GetAllConnectionsAsync();
Task<ConnectionConfig?> GetConnectionByNameAsync(string connectionName);
```

#### Entity Operations

```csharp
Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, List<AppFilter>? filters = null);
Task<GenericEntityResponse> GetEntityByIdAsync(string tableName, object id);
Task<GenericEntityResponse> InsertEntityAsync(string tableName, Dictionary<string, object> entityData, string userId);
Task<GenericEntityResponse> UpdateEntityAsync(string tableName, string entityId, Dictionary<string, object> entityData, string userId);
Task<GenericEntityResponse> DeleteEntityAsync(string tableName, object id, string userId);
```

#### Import/Export Operations

```csharp
Task<OperationStartResponse> ImportFromCsvAsync(string tableName, Stream csvStream, string fileName, string userId, Dictionary<string, string>? columnMapping = null);
Task<Stream> ExportToCsvAsync(string tableName, List<AppFilter>? filters = null);
```

## Data Management Pages

### 1. Quality Dashboard

**Route**: `/data/quality-dashboard`

Displays data quality metrics and alerts.

#### Features

- Overall quality score
- Quality metrics by table
- Data completeness indicators
- Quality alerts and warnings

#### API Integration

```razor
@inject IDataManagementService DataManagementService

@code {
    private DataQualityDashboardResult? _qualityDashboard;
    
    protected override async Task OnInitializedAsync()
    {
        _qualityDashboard = await DataManagementService.GetQualityDashboardAsync();
    }
}
```

### 2. Validation

**Route**: `/data/validation`

Provides entity validation and validation rule management.

#### Features

- Real-time validation feedback
- Validation rule management
- Batch validation
- Validation error resolution

#### API Integration

```razor
@code {
    private async Task ValidateEntity(string tableName, Dictionary<string, object> entityData)
    {
        var result = await DataManagementService.ValidateEntityAsync(tableName, entityData);
        if (!result.IsValid)
        {
            // Display validation errors
            foreach (var error in result.Errors)
            {
                // Show error
            }
        }
    }
    
    private async Task ValidateBatch(string tableName, List<Dictionary<string, object>> entities)
    {
        var results = await DataManagementService.ValidateBatchAsync(tableName, entities);
        // Process validation results
    }
}
```

### 3. Versioning

**Route**: `/data/versioning`

Manages entity version history and restoration.

#### Features

- Version history timeline
- Version comparison
- Version restore workflows
- Version labeling

#### API Integration

```razor
@code {
    private async Task CreateVersion(string tableName, string entityId, string userId)
    {
        var result = await DataManagementService.CreateVersionAsync(
            tableName, entityId, null, userId, "Checkpoint");
    }
    
    private async Task LoadVersionHistory(string tableName, string entityId)
    {
        var versions = await DataManagementService.GetVersionHistoryAsync(tableName, entityId);
    }
    
    private async Task RestoreVersion(string tableName, string entityId, string versionId, string userId)
    {
        var result = await DataManagementService.RestoreVersionAsync(
            tableName, entityId, versionId, userId);
    }
}
```

### 4. Audit

**Route**: `/data/audit`

Displays audit logs and change history.

#### Features

- Audit log viewer with filtering
- Change history visualization
- User activity tracking
- Compliance reporting

#### API Integration

```razor
@code {
    private async Task LoadAuditLogs(string tableName, string? entityId = null)
    {
        var logs = await ApiClient.GetAsync<List<AuditLog>>(
            $"/api/datamanagement/audit/{tableName}" + 
            (entityId != null ? $"/{entityId}" : ""));
    }
}
```

## Generic CRUD Operations

### Using PPDMDataGrid Component

The `PPDMDataGrid` component provides a generic data grid for any PPDM entity.

```razor
<PPDMDataGrid TableName="FIELD" 
              OnRowClick="@OnRowClick"
              OnEdit="@OnEdit"
              OnDelete="@OnDelete" />
```

### Using PPDMEntityForm Component

The `PPDMEntityForm` component provides a generic form for entity creation/editing.

```razor
<PPDMEntityForm TableName="FIELD"
                EntityId="@_selectedFieldId"
                OnSave="@OnSave" />
```

## Import/Export Operations

### CSV Import

```razor
@code {
    private async Task ImportCsv(IBrowserFile file)
    {
        using var stream = file.OpenReadStream();
        var result = await DataManagementService.ImportFromCsvAsync(
            "FIELD",
            stream,
            file.Name,
            _userId,
            onProgress: (progress) =>
            {
                _importProgress = progress;
                StateHasChanged();
            });
        
        if (result.OperationId != null)
        {
            // Track progress
        }
    }
}
```

### CSV Export

```razor
@code {
    private async Task ExportCsv()
    {
        var stream = await DataManagementService.ExportToCsvAsync("FIELD");
        // Download file
        await DownloadFile(stream, "fields.csv");
    }
}
```

## Data Quality Monitoring

### Quality Metrics

```razor
@code {
    private async Task LoadQualityMetrics(string tableName)
    {
        var metrics = await DataManagementService.GetTableQualityMetricsAsync(tableName);
        // Display metrics
    }
}
```

### Quality Alerts

```razor
@code {
    private async Task LoadQualityAlerts()
    {
        var dashboard = await DataManagementService.GetQualityDashboardAsync();
        // Display alerts
        foreach (var alert in dashboard.Alerts)
        {
            // Show alert
        }
    }
}
```

## Validation Workflows

### Real-Time Validation

```razor
@code {
    private async Task OnFieldChanged(string fieldName, object value)
    {
        var entityData = new Dictionary<string, object> { { fieldName, value } };
        var result = await DataManagementService.ValidateEntityAsync("FIELD", entityData);
        
        if (!result.IsValid)
        {
            // Show validation errors
            _validationErrors[fieldName] = result.Errors
                .Where(e => e.FieldName == fieldName)
                .Select(e => e.ErrorMessage)
                .ToList();
        }
        else
        {
            _validationErrors.Remove(fieldName);
        }
        
        StateHasChanged();
    }
}
```

### Batch Validation

```razor
@code {
    private async Task ValidateAll()
    {
        var entities = _entities.Select(e => e.ToDictionary()).ToList();
        var results = await DataManagementService.ValidateBatchAsync("FIELD", entities);
        
        // Display validation results
        foreach (var result in results)
        {
            if (!result.IsValid)
            {
                // Show errors
            }
        }
    }
}
```

## Version Management

### Version History Display

```razor
@code {
    private List<VersionInfo>? _versions;
    
    private async Task LoadVersions(string entityId)
    {
        _versions = await DataManagementService.GetVersionHistoryAsync("FIELD", entityId);
    }
}
```

### Version Comparison

```razor
@code {
    private async Task CompareVersions(string versionId1, string versionId2)
    {
        var version1 = await ApiClient.GetAsync<VersionData>(
            $"/api/ppdm39/versioning/FIELD/{_entityId}/versions/{versionId1}");
        var version2 = await ApiClient.GetAsync<VersionData>(
            $"/api/ppdm39/versioning/FIELD/{_entityId}/versions/{versionId2}");
        
        // Compare and display differences
    }
}
```

## Progress Tracking

### Long-Running Operations

```razor
@code {
    private ProgressUpdate? _currentProgress;
    
    private async Task ImportLargeFile(IBrowserFile file)
    {
        using var stream = file.OpenReadStream();
        var result = await DataManagementService.ImportFromCsvAsync(
            "FIELD",
            stream,
            file.Name,
            _userId,
            onProgress: (progress) =>
            {
                _currentProgress = progress;
                StateHasChanged();
            });
    }
}
```

## Best Practices

### 1. Always Validate Before Save

- Validate entities before submitting to API
- Show validation errors immediately
- Prevent invalid data submission

### 2. Use Progress Tracking

- Show progress for long-running operations
- Provide cancellation options
- Update UI in real-time

### 3. Handle Errors Gracefully

- Catch and handle API errors
- Provide user-friendly error messages
- Log errors for debugging

### 4. Cache Connection Information

- Cache connection list
- Refresh when needed
- Handle connection changes

### 5. Use Generic Components

- Use PPDMDataGrid for tables
- Use PPDMEntityForm for forms
- Reuse components across pages

## Related Documentation

- [Architecture](beep-oilgas-web-architecture.md)
- [API Integration](beep-oilgas-web-api-integration.md)
- [Services](beep-oilgas-web-services.md)
- [Components](beep-oilgas-web-components.md)

