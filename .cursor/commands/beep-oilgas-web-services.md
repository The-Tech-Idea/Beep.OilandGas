# Beep.OilandGas.Web Services Documentation

## Overview

The service layer provides abstraction between the UI components and the API service. All business logic and API communication is handled through services, ensuring separation of concerns and testability.

## Service Architecture

### Service Layer Pattern

```
Pages/Components
    ↓
Services (Business Logic)
    ↓
ApiClient (HTTP Communication)
    ↓
API Service
```

## Core Services

### ApiClient

Generic HTTP client for API communication.

**Location**: `Services/ApiClient.cs`

**Responsibilities**:
- HTTP request/response handling
- JSON serialization/deserialization
- Error handling and logging

**Methods**:
- `GetAsync<T>()` - GET requests
- `PostAsync<TRequest, TResponse>()` - POST requests
- `PutAsync<TRequest, TResponse>()` - PUT requests
- `DeleteAsync<T>()` - DELETE requests
- `PostStreamAsync<TRequest>()` - Stream responses

**Registration**:
```csharp
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiServiceUrl);
});
```

### DataManagementService

Centralized service for data management operations.

**Location**: `Services/DataManagementService.cs`

**Interface**: `IDataManagementService`

**Responsibilities**:
- Connection management
- Entity CRUD operations
- Import/export operations
- Validation operations
- Quality operations
- Versioning operations

**Key Methods**:

```csharp
// Connection Management
Task<string?> GetCurrentConnectionNameAsync();
Task<SetCurrentDatabaseResult> SetCurrentConnectionAsync(string connectionName);
Task<List<DatabaseConnectionListItem>> GetAllConnectionsAsync();

// Entity Operations
Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, List<AppFilter>? filters = null);
Task<GenericEntityResponse> GetEntityByIdAsync(string tableName, object id);
Task<GenericEntityResponse> InsertEntityAsync(string tableName, Dictionary<string, object> entityData, string userId);
Task<GenericEntityResponse> UpdateEntityAsync(string tableName, string entityId, Dictionary<string, object> entityData, string userId);
Task<GenericEntityResponse> DeleteEntityAsync(string tableName, object id, string userId);

// Import/Export
Task<OperationStartResponse> ImportFromCsvAsync(string tableName, Stream csvStream, string fileName, string userId, Action<ProgressUpdate>? onProgress = null);
Task<Stream> ExportToCsvAsync(string tableName, List<AppFilter>? filters = null);

// Validation
Task<ValidationResult> ValidateEntityAsync(string tableName, Dictionary<string, object> entityData);
Task<List<ValidationResult>> ValidateBatchAsync(string tableName, List<Dictionary<string, object>> entities);

// Quality
Task<DataQualityResult> GetTableQualityMetricsAsync(string tableName);
Task<DataQualityDashboardResult> GetQualityDashboardAsync();

// Versioning
Task<VersioningResult> CreateVersionAsync(string tableName, string entityId, Dictionary<string, object>? entityData, string userId);
Task<List<VersionInfo>> GetVersionHistoryAsync(string tableName, string entityId);
Task<VersioningResult> RestoreVersionAsync(string tableName, string entityId, string versionId, string userId);
```

**Registration**:
```csharp
builder.Services.AddScoped<IDataManagementService, DataManagementService>();
```

### EntityMetadataService

Service for managing entity metadata.

**Location**: `Services/EntityMetadataService.cs`

**Responsibilities**:
- Entity metadata retrieval
- Table schema information
- Field definitions

### ProgressTrackingClient

SignalR client for real-time progress tracking.

**Location**: `Services/ProgressTrackingClient.cs`

**Interface**: `IProgressTrackingClient`

**Responsibilities**:
- Connect to SignalR hub
- Subscribe to progress updates
- Track operation progress

**Usage**:
```csharp
_progressTrackingClient.OnProgressUpdate += (progress) =>
{
    if (progress.OperationId == operationId)
    {
        // Update UI with progress
    }
};
await _progressTrackingClient.JoinOperationAsync(operationId);
```

**Registration**:
```csharp
builder.Services.AddScoped<IProgressTrackingClient, ProgressTrackingClient>();
```

### BrandingRegistrationService

Hosted service for registering branding with Identity Server.

**Location**: `Services/BrandingRegistrationService.cs`

**Responsibilities**:
- Register branding on startup
- Update Identity Server with theme information

**Registration**:
```csharp
builder.Services.AddHostedService<BrandingRegistrationService>();
```

## Service Patterns

### Dependency Injection

All services are registered in `Program.cs` and injected into components:

```razor
@inject IDataManagementService DataManagementService
@inject ApiClient ApiClient
```

### Error Handling

Services handle errors and return appropriate responses:

```csharp
public async Task<GetEntitiesResponse> GetEntitiesAsync(string tableName)
{
    try
    {
        return await _apiClient.PostAsync<GetEntitiesRequest, GetEntitiesResponse>(
            $"/api/ppdm39/data/{tableName}", request);
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Error getting entities from {TableName}", tableName);
        return new GetEntitiesResponse 
        { 
            Success = false, 
            ErrorMessage = ex.Message 
        };
    }
}
```

### Caching

Services implement caching where appropriate:

```csharp
private List<DatabaseConnectionListItem> _connections = new();
private DateTime _lastRefreshTime = DateTime.MinValue;
private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

public async Task<List<DatabaseConnectionListItem>> GetAllConnectionsAsync()
{
    if (_connections.Any() && DateTime.UtcNow - _lastRefreshTime < _cacheTimeout)
    {
        return _connections.ToList();
    }
    
    await RefreshConnectionsAsync();
    return _connections.ToList();
}
```

### Events

Services can fire events for state changes:

```csharp
public event EventHandler<string?>? CurrentConnectionChanged;

private void OnConnectionChanged(string? connectionName)
{
    CurrentConnectionChanged?.Invoke(this, connectionName);
}
```

## Service Lifecycle

### Scoped Services

Most services are registered as scoped (one per HTTP request):

```csharp
builder.Services.AddScoped<IDataManagementService, DataManagementService>();
```

### Singleton Services

Services that maintain state across requests:

```csharp
builder.Services.AddSingleton<IThemeProvider, ThemeProvider>();
```

### Transient Services

Services created fresh for each use:

```csharp
builder.Services.AddTransient<SomeService>();
```

## Creating New Services

### Service Interface

```csharp
public interface IMyService
{
    Task<MyResponse> DoSomethingAsync(MyRequest request);
}
```

### Service Implementation

```csharp
public class MyService : IMyService
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<MyService> _logger;
    
    public MyService(ApiClient apiClient, ILogger<MyService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }
    
    public async Task<MyResponse> DoSomethingAsync(MyRequest request)
    {
        try
        {
            return await _apiClient.PostAsync<MyRequest, MyResponse>(
                "/api/my-endpoint", request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DoSomethingAsync");
            throw;
        }
    }
}
```

### Service Registration

```csharp
builder.Services.AddScoped<IMyService, MyService>();
```

## Best Practices

### 1. Single Responsibility

Each service should have a single, well-defined responsibility.

### 2. Error Handling

Always handle errors gracefully and return appropriate responses.

### 3. Logging

Log important operations and errors for debugging.

### 4. Caching

Cache frequently accessed, rarely changing data.

### 5. Async/Await

Use async/await for all I/O operations.

### 6. Dependency Injection

Use constructor injection for dependencies.

### 7. Interface-Based Design

Define interfaces for services to enable testing.

## Related Documentation

- [API Integration](beep-oilgas-web-api-integration.md)
- [Architecture](beep-oilgas-web-architecture.md)
- [Components](beep-oilgas-web-components.md)

