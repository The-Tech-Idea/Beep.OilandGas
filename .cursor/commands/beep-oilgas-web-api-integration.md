# Beep.OilandGas.Web API Integration Documentation

## Overview

The Blazor web application integrates with the `Beep.OilandGas.ApiService` through a service-oriented architecture. All data operations, lifecycle management, and business logic are performed through the API service, ensuring separation of concerns and enabling scalability.

## ApiClient Service

### Purpose

The `ApiClient` is a generic HTTP client service that provides a unified interface for making REST API calls to the API service. It handles JSON serialization, error handling, and logging.

### Implementation

```csharp
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;
    
    // JSON serialization options
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
}
```

### Methods

#### GET Requests

```csharp
public async Task<T?> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
```

- **Purpose**: Retrieve data from API
- **Example**: `var fields = await apiClient.GetAsync<List<Field>>("/api/field/fields");`

#### POST Requests

```csharp
// With response
public async Task<TResponse?> PostAsync<TRequest, TResponse>(
    string endpoint, 
    TRequest data, 
    CancellationToken cancellationToken = default)

// Without response (returns bool)
public async Task<bool> PostAsync<TRequest>(
    string endpoint, 
    TRequest data, 
    CancellationToken cancellationToken = default)
```

- **Purpose**: Create or submit data
- **Example**: `var result = await apiClient.PostAsync<CreateFieldRequest, FieldResponse>("/api/field/create", request);`

#### PUT Requests

```csharp
public async Task<TResponse?> PutAsync<TRequest, TResponse>(
    string endpoint, 
    TRequest data, 
    CancellationToken cancellationToken = default)
```

- **Purpose**: Update existing resources
- **Example**: `var result = await apiClient.PutAsync<UpdateFieldRequest, FieldResponse>("/api/field/update", request);`

#### DELETE Requests

```csharp
// With response
public async Task<TResult?> DeleteAsync<TResult>(string endpoint, CancellationToken cancellationToken = default)

// Without response (returns bool)
public async Task<bool> DeleteAsync(string endpoint, CancellationToken cancellationToken = default)
```

- **Purpose**: Delete resources
- **Example**: `var success = await apiClient.DeleteAsync("/api/field/{fieldId}");`

#### File Upload

```csharp
public async Task<TResponse?> PostAsync<TResponse>(
    string endpoint,
    HttpContent content,
    CancellationToken cancellationToken = default)
```

- **Purpose**: Upload files (multipart form data)
- **Example**: Used for CSV imports

#### Stream Response

```csharp
public async Task<Stream?> PostStreamAsync<TRequest>(
    string endpoint,
    TRequest data,
    CancellationToken cancellationToken = default)
```

- **Purpose**: Download files or large data streams
- **Example**: Used for CSV exports

### Error Handling

The `ApiClient` throws `HttpRequestException` for HTTP errors. Services using `ApiClient` should handle these exceptions:

```csharp
try
{
    var result = await _apiClient.GetAsync<Data>("/api/endpoint");
}
catch (HttpRequestException ex)
{
    _logger.LogError(ex, "API request failed");
    // Handle error (show user message, retry, etc.)
}
```

### Configuration

The `ApiClient` is configured in `Program.cs`:

```csharp
var apiServiceUrl = builder.Configuration["ApiService:BaseUrl"] ?? "https://localhost:7001";
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiServiceUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});
```

## Service Layer Pattern

### Service Interface Pattern

Services follow a consistent pattern:

1. **Interface Definition**: Define interface for service
2. **Implementation**: Implement using `ApiClient`
3. **Dependency Injection**: Register in DI container
4. **Usage**: Inject into pages/components

### Example: DataManagementService

```csharp
public interface IDataManagementService
{
    Task<string?> GetCurrentConnectionNameAsync();
    Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, List<AppFilter>? filters = null);
    // ... other methods
}

public class DataManagementService : IDataManagementService
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<DataManagementService> _logger;
    
    public DataManagementService(
        ApiClient apiClient,
        ILogger<DataManagementService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }
    
    public async Task<GetEntitiesResponse> GetEntitiesAsync(
        string tableName, 
        List<AppFilter>? filters = null)
    {
        var request = new GetEntitiesRequest
        {
            TableName = tableName,
            Filters = filters ?? new List<AppFilter>()
        };
        return await _apiClient.PostAsync<GetEntitiesRequest, GetEntitiesResponse>(
            $"/api/ppdm39/data/{tableName}", request);
    }
}
```

## API Endpoint Patterns

### RESTful Conventions

The API follows RESTful conventions:

- **GET** `/api/resource` - List resources
- **GET** `/api/resource/{id}` - Get specific resource
- **POST** `/api/resource` - Create resource
- **PUT** `/api/resource/{id}` - Update resource
- **DELETE** `/api/resource/{id}` - Delete resource

### Query Parameters

Common query parameters:

- `connectionName` - Database connection name
- `userId` - User identifier for audit trails
- `page` - Page number for pagination
- `pageSize` - Items per page

### Request/Response Models

All API requests and responses use strongly-typed models:

```csharp
// Request model
public class CreateFieldRequest
{
    public string FieldName { get; set; }
    public string? Description { get; set; }
    // ...
}

// Response model
public class FieldResponse
{
    public string FieldId { get; set; }
    public string FieldName { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
```

## Service Registration

Services are registered in `Program.cs`:

```csharp
// API Client
builder.Services.AddHttpClient<ApiClient>(/* ... */);

// Application Services
builder.Services.AddScoped<IDataManagementService, DataManagementService>();
builder.Services.AddScoped<IProgressTrackingClient, ProgressTrackingClient>();
```

## Usage in Components

### Injecting Services

```razor
@inject IDataManagementService DataManagementService
@inject ApiClient ApiClient
```

### Calling Services

```razor
@code {
    private List<Field> _fields = new();
    private bool _isLoading = true;
    
    protected override async Task OnInitializedAsync()
    {
        try
        {
            _fields = await DataManagementService.GetFieldsAsync();
        }
        catch (Exception ex)
        {
            // Handle error
        }
        finally
        {
            _isLoading = false;
        }
    }
}
```

## Error Handling Patterns

### Service-Level Error Handling

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

### Component-Level Error Handling

```razor
@code {
    private string? _errorMessage;
    
    private async Task LoadData()
    {
        try
        {
            _errorMessage = null;
            var result = await DataManagementService.GetEntitiesAsync("FIELD");
            if (!result.Success)
            {
                _errorMessage = result.ErrorMessage ?? "Failed to load data";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error: {ex.Message}";
        }
    }
}
```

### User Feedback

```razor
@inject ISnackbar Snackbar

@code {
    private async Task SaveData()
    {
        try
        {
            var result = await DataManagementService.SaveEntityAsync(entity);
            if (result.Success)
            {
                Snackbar.Add("Data saved successfully", Severity.Success);
            }
            else
            {
                Snackbar.Add($"Error: {result.ErrorMessage}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error: {ex.Message}", Severity.Error);
        }
    }
}
```

## Progress Tracking

### SignalR Progress Updates

For long-running operations, use `ProgressTrackingClient`:

```csharp
public class DataManagementService
{
    private readonly IProgressTrackingClient? _progressTrackingClient;
    
    public async Task<OperationStartResponse> ImportFromCsvAsync(
        string tableName, 
        Stream csvStream,
        Action<ProgressUpdate>? onProgress = null)
    {
        var response = await _apiClient.PostAsync<OperationStartResponse>(url, content);
        
        // Subscribe to progress updates
        if (onProgress != null && response?.OperationId != null && _progressTrackingClient != null)
        {
            _progressTrackingClient.OnProgressUpdate += (progress) =>
            {
                if (progress.OperationId == response.OperationId)
                    onProgress(progress);
            };
            await _progressTrackingClient.JoinOperationAsync(response.OperationId);
        }
        
        return response;
    }
}
```

### Using Progress in Components

```razor
@code {
    private ProgressUpdate? _currentProgress;
    
    private async Task ImportData()
    {
        await DataManagementService.ImportFromCsvAsync(
            "FIELD",
            csvStream,
            onProgress: (progress) =>
            {
                _currentProgress = progress;
                StateHasChanged();
            });
    }
}
```

## Caching Strategies

### Connection-Level Caching

```csharp
public class DataManagementService
{
    private string? _currentConnectionName;
    private DateTime _lastRefreshTime = DateTime.MinValue;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);
    
    public async Task<List<Connection>> GetAllConnectionsAsync()
    {
        // Use cached data if recent
        if (_connections.Any() && DateTime.UtcNow - _lastRefreshTime < _cacheTimeout)
        {
            return _connections.ToList();
        }
        
        await RefreshConnectionsAsync();
        return _connections.ToList();
    }
}
```

### Component-Level Caching

```razor
@code {
    private List<Field>? _cachedFields;
    private DateTime? _cacheTime;
    
    private async Task<List<Field>> GetFieldsAsync()
    {
        if (_cachedFields != null && _cacheTime.HasValue && 
            DateTime.UtcNow - _cacheTime.Value < TimeSpan.FromMinutes(5))
        {
            return _cachedFields;
        }
        
        _cachedFields = await DataManagementService.GetFieldsAsync();
        _cacheTime = DateTime.UtcNow;
        return _cachedFields;
    }
}
```

## Retry Logic

### Implementing Retry

```csharp
public async Task<T?> GetWithRetryAsync<T>(string endpoint, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await _apiClient.GetAsync<T>(endpoint);
        }
        catch (HttpRequestException ex) when (attempt < maxRetries)
        {
            _logger.LogWarning(ex, "Attempt {Attempt} failed, retrying...", attempt);
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt))); // Exponential backoff
        }
    }
    throw new HttpRequestException("Max retries exceeded");
}
```

## Authentication Integration

### Token Management

The API client automatically includes authentication tokens from cookies. The API service validates these tokens and extracts user information.

### User Context

```csharp
// User ID is typically passed as query parameter
var url = $"/api/ppdm39/data/{tableName}/insert?userId={Uri.EscapeDataString(userId)}";
```

## Best Practices

### 1. Always Use Services

- Don't call `ApiClient` directly from components
- Use service layer for all API calls
- Services provide abstraction and error handling

### 2. Handle Errors Gracefully

- Always wrap API calls in try-catch
- Provide user-friendly error messages
- Log errors for debugging

### 3. Use Cancellation Tokens

- Pass `CancellationToken` for long-running operations
- Allows cancellation of requests
- Improves user experience

### 4. Validate Before Sending

- Validate data before API calls
- Reduces unnecessary network traffic
- Provides immediate user feedback

### 5. Use Loading States

- Show loading indicators during API calls
- Improves perceived performance
- Better user experience

### 6. Cache Appropriately

- Cache frequently accessed, rarely changing data
- Don't cache user-specific or time-sensitive data
- Invalidate cache on updates

## Related Documentation

- [Architecture](beep-oilgas-web-architecture.md)
- [Services](beep-oilgas-web-services.md)
- [Components](beep-oilgas-web-components.md)
- [Connection Management](beep-oilgas-web-connection-management.md)

