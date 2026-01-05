# Beep.OilandGas.Client

Client SDK for accessing Beep Oil & Gas services. Provides a unified interface (`BeepOilandGasApp`) for accessing services both remotely (via HTTP API) and locally (via direct service injection).

## Overview

The `BeepOilandGasApp` (AppClass) is **the single, unified entry point** for accessing all Beep Oil & Gas services. It works in two modes:

- **Remote Mode**: Uses HTTP API client to call services via REST API
- **Local Mode**: Uses direct service injection to access services directly (no HTTP calls)

The same code works in both modes, making it easy to:
- Use the same application code whether accessing services remotely or locally
- Switch modes via configuration without code changes
- Test locally without needing the API server running
- Deploy flexibly - same application can work in both modes

## Installation

```bash
dotnet add package Beep.OilandGas.Client
```

## Quick Start

### Remote Mode (HTTP API)

```csharp
// In Program.cs
using Beep.OilandGas.Client.DependencyInjection;
using Beep.OilandGas.Client.App;

var builder = WebApplication.CreateBuilder(args);

// Register AppClass in remote mode
builder.Services.AddBeepOilandGasApp(new AppOptions
{
    AccessMode = ServiceAccessMode.Remote,
    ApiBaseUrl = "https://api.example.com",
    Username = "user",
    Password = "pass",
    DefaultConnectionName = "PPDM39"
});

// Use in your services
public class MyService
{
    private readonly IBeepOilandGasApp _app;
    
    public MyService(IBeepOilandGasApp app)
    {
        _app = app;
    }
    
    public async Task DoWork()
    {
        // Works via HTTP API
        var wells = await _app.Well.CompareWellsAsync(...);
        var connections = await _app.Connection.GetAllConnectionsAsync();
        
        // Specify connection name if needed
        var production = await _app.DataManagement.PPDM39Data
            .GetEntitiesAsync("WELL", filters, connectionName: "PPDM39_Production");
    }
}
```

### Local Mode (Direct DI)

```csharp
// In Program.cs
using Beep.OilandGas.Client.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Ensure all services are registered
builder.Services.AddScoped<IWellComparisonService, WellComparisonService>();
builder.Services.AddScoped<IPPDMProductionService, PPDMProductionService>();
builder.Services.AddSingleton<IDMEEditor, DMEEditor>();
// ... other services

// Register AppClass in local mode
builder.Services.AddBeepOilandGasAppLocal(options =>
{
    options.DefaultConnectionName = "PPDM39";
});

// Use in your services - same code as remote mode!
public class MyService
{
    private readonly IBeepOilandGasApp _app;
    
    public MyService(IBeepOilandGasApp app)
    {
        _app = app;
    }
    
    public async Task DoWork()
    {
        // Works via direct service calls (no HTTP)
        var wells = await _app.Well.CompareWellsAsync(...);
        var connections = await _app.Connection.GetAllConnectionsAsync();
    }
}
```

### Auto Mode (Configuration-based)

```csharp
// In appsettings.json
{
  "BeepOilandGas": {
    "AccessMode": "Auto",
    "ApiBaseUrl": "https://api.example.com",
    "UseLocalServices": true
  }
}

// In Program.cs
builder.Services.AddBeepOilandGasAppAuto(configuration);
```

## Features

### Connection Management

The AppClass provides comprehensive connection management:

```csharp
// Get all connections
var connections = await _app.Connection.GetAllConnectionsAsync();

// Get specific connection
var connection = await _app.Connection.GetConnectionAsync("PPDM39");

// Test connection
var result = await _app.Connection.TestConnectionAsync("PPDM39");

// Set current connection
await _app.Connection.SetCurrentConnectionAsync("ProductionDB");

// Get current connection
var current = await _app.Connection.GetCurrentConnectionAsync();
```

### Multi-Connection Support

All client methods support an optional `connectionName` parameter:

```csharp
// Use default connection
var data = await _app.DataManagement.PPDM39Data.GetEntitiesAsync("WELL", request);

// Use specific connection
var data = await _app.DataManagement.PPDM39Data.GetEntitiesAsync(
    "WELL", 
    request, 
    connectionName: "ProductionDB");
```

### Authentication

#### Token Provider Pattern

```csharp
var tokenProvider = new TokenProvider("your-access-token");
var apiClient = new BeepOilandGasApiClient(httpClient, options, tokenProvider);
```

#### Credentials Pattern

```csharp
builder.Services.AddBeepOilandGasApiClientWithCredentials(
    options => {
        options.BaseUrl = "https://api.example.com";
    },
    identityServerUrl: "https://identity.example.com",
    username: "user",
    password: "pass"
);
```

The SDK handles token refresh automatically for credentials-based authentication.

## Available Clients

- **Well**: Well comparison and operations
- **Connection**: Connection management
- **DataManagement**: PPDM39 data operations (CRUD, Setup, Validation, Quality, Workflow)
- **Accounting**: Production accounting, costs, revenue, royalty, GL
- **Calculations**: Flash calculations, gas lift, nodal analysis, etc.
- **Field**: Field orchestrator, exploration, development, production
- **Operations**: Drilling, production operations, enhanced recovery
- **Properties**: Oil properties, gas properties, heat maps
- **Pumps**: Hydraulic pumps, plunger lift, sucker rod pumping
- **AccessControl**: Access control, asset hierarchy, user profiles

## Error Handling

The SDK provides specific exception types:

```csharp
try
{
    var result = await _app.Well.CompareWellsAsync(request);
}
catch (ApiAuthenticationException ex)
{
    // Authentication failed
}
catch (ApiException ex)
{
    // HTTP error occurred
    Console.WriteLine($"Status: {ex.StatusCode}, Message: {ex.Message}");
}
catch (ApiTimeoutException ex)
{
    // Request timed out
}
catch (ApiClientException ex)
{
    // General client error
}
```

## Configuration

### ApiClientOptions

- `BaseUrl` (required): Base URL of the API server
- `Timeout`: Request timeout (default: 30 seconds)
- `DefaultConnectionName`: Default connection name (default: "PPDM39")
- `MaxRetries`: Maximum retry attempts (default: 3)
- `RetryDelay`: Delay between retries (default: 1 second)
- `EnableLogging`: Enable request/response logging (default: false)

### AppOptions

- `AccessMode`: Remote, Local, or Auto (default: Auto)
- `ApiBaseUrl`: API base URL (required for Remote mode)
- `Username` / `Password`: Credentials for authentication
- `DefaultConnectionName`: Default connection name (default: "PPDM39")
- `UseLocalServices`: Use local services if available (for Auto mode)
- `ClientId` / `ClientSecret`: OAuth client credentials
- `IdentityServerUrl`: Identity server URL for token acquisition
- `Timeout`: Request timeout (default: 30 seconds)

## Advanced Usage

### Custom HttpClient Configuration

```csharp
builder.Services.AddHttpClient<BeepOilandGasApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.Timeout = TimeSpan.FromMinutes(5);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    // Custom handler configuration
});
```

### Logging

```csharp
builder.Services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

// Enable request/response logging
var options = new ApiClientOptions
{
    BaseUrl = "https://api.example.com",
    EnableLogging = true
};
```

## Local Mode Data Source Management

In local mode, the AppClass integrates with `DataSourceLifecycleHelper` for proper data source lifecycle management:

- Automatic data source creation from connection properties
- Connection pooling and caching
- Connection validation
- Proper disposal and cleanup

The `DataSourceManager` handles all data source operations and integrates with `IDMEEditor` and `ConfigEditor`.

## Architecture

### Remote Mode Flow

```
Application → BeepOilandGasApp → BeepOilandGasApiClient → HTTP API → Services
```

### Local Mode Flow

```
Application → BeepOilandGasApp → Service Adapters → Direct Service Injection → Services
```

## Development

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Creating NuGet Package

```bash
dotnet pack
```

## License

MIT

## Support

For issues and questions, please use the project repository issue tracker.

