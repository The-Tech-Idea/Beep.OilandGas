# Beep.OilandGas.Web Connection Management Documentation

## Overview

The connection management system allows users to connect to available databases configured in IDMEEditor. When users first log in, they can select an existing connection or create a new database through the API service.

## Architecture

### Connection Flow

```
User Login → Check Current Connection → 
    If No Connection → First-Login Wizard →
        Select Existing Connection OR Create New Database →
    Set Current Connection → Proceed to Application
```

### Components

1. **API Server**: Exposes IDMEEditor configured connections
2. **ConnectionService**: Client service for connection operations
3. **First-Login Wizard**: Multi-step wizard for connection setup
4. **Connection Selector**: Component for selecting connections
5. **Connection Test Dialog**: Component for testing connections

## API Endpoints

### Connection Management Endpoints

#### Get All Connections

**Endpoint**: `GET /api/connections`

**Response**:
```json
[
  {
    "connectionName": "PPDM39",
    "databaseType": "SQL Server",
    "server": "localhost",
    "database": "PPDM39",
    "isActive": true
  }
]
```

#### Get Connection Details

**Endpoint**: `GET /api/connections/{connectionName}`

**Response**:
```json
{
  "connectionName": "PPDM39",
  "databaseType": "SQL Server",
  "connectionString": "...",
  "isActive": true
}
```

#### Test Connection

**Endpoint**: `POST /api/connections/test`

**Request**:
```json
{
  "connectionName": "PPDM39"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Connection successful"
}
```

#### Set Current Connection

**Endpoint**: `POST /api/connections/set-current`

**Request**:
```json
{
  "connectionName": "PPDM39",
  "userId": "user123"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Connection set successfully"
}
```

#### Get Current Connection

**Endpoint**: `GET /api/connections/current`

**Response**:
```json
{
  "connectionName": "PPDM39"
}
```

#### Create New Connection

**Endpoint**: `POST /api/connections/create`

**Request**:
```json
{
  "connectionName": "NewDatabase",
  "databaseType": "SQL Server",
  "server": "localhost",
  "database": "NewPPDM39",
  "createDatabase": true
}
```

**Response**:
```json
{
  "success": true,
  "connectionName": "NewDatabase",
  "message": "Connection created successfully"
}
```

## Connection Service

### IConnectionService Interface

```csharp
public interface IConnectionService
{
    Task<List<ConnectionInfo>> GetAllConnectionsAsync();
    Task<ConnectionInfo?> GetConnectionAsync(string connectionName);
    Task<ConnectionTestResult> TestConnectionAsync(string connectionName);
    Task<SetConnectionResult> SetCurrentConnectionAsync(string connectionName, string userId);
    Task<ConnectionInfo?> GetCurrentConnectionAsync();
    Task<CreateConnectionResult> CreateConnectionAsync(CreateConnectionRequest request);
}
```

### ConnectionService Implementation

**Location**: `Services/ConnectionService.cs`

Uses `ApiClient` to communicate with connection API endpoints.

## First-Login Flow

### FirstLoginConnectionWizard Component

Multi-step wizard for first-login connection setup.

**Steps**:
1. **Welcome**: Introduction and overview
2. **Connection Selection**: List available connections OR create new
3. **Connection Test**: Test selected connection
4. **Database Creation** (if creating new): Database creation wizard
5. **Completion**: Set connection and proceed

**Usage**:
```razor
<FirstLoginConnectionWizard OnComplete="@OnConnectionSetupComplete" />
```

### ConnectionSelector Component

Component for selecting from available connections.

**Properties**:
- `Connections` (List<ConnectionInfo>) - Available connections
- `SelectedConnection` (string?) - Currently selected connection
- `OnConnectionSelected` (EventCallback) - Fired when connection is selected

**Usage**:
```razor
<ConnectionSelector Connections="@_connections"
                    SelectedConnection="@_selectedConnection"
                    OnConnectionSelected="@OnConnectionSelected" />
```

### ConnectionTestDialog Component

Dialog for testing database connections.

**Usage**:
```razor
var parameters = new DialogParameters { ["ConnectionName"] = connectionName };
var dialog = await DialogService.ShowAsync<ConnectionTestDialog>("Test Connection", parameters);
var result = await dialog.Result;
```

## First-Login Page

### FirstLogin.razor

**Route**: `/first-login`

Page that redirects users to connection setup if no connection is set.

**Logic**:
```razor
@code {
    protected override async Task OnInitializedAsync()
    {
        var currentConnection = await ConnectionService.GetCurrentConnectionAsync();
        if (currentConnection == null)
        {
            // Show connection wizard
            _showWizard = true;
        }
        else
        {
            // Redirect to dashboard
            NavigationManager.NavigateTo("/dashboard");
        }
    }
}
```

## Integration with Authentication

### Post-Login Redirect

After successful authentication, check for connection:

```razor
@inject IConnectionService ConnectionService
@inject NavigationManager NavigationManager

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender && _isAuthenticated)
    {
        var connection = await ConnectionService.GetCurrentConnectionAsync();
        if (connection == null)
        {
            NavigationManager.NavigateTo("/first-login");
        }
    }
}
```

## Database Creation

### Create Database Wizard

If user chooses to create a new database, the wizard guides them through:

1. **Database Type Selection**: SQL Server, PostgreSQL, etc.
2. **Connection Configuration**: Server, database name, credentials
3. **Database Creation**: Use DataManagement API to create database
4. **Connection Test**: Verify connection works
5. **Set as Current**: Set as current connection

**Integration**:
```razor
@code {
    private async Task CreateDatabase(CreateDatabaseRequest request)
    {
        var result = await DataManagementService.CreateDatabaseAsync(request);
        if (result.Success)
        {
            // Set as current connection
            await ConnectionService.SetCurrentConnectionAsync(
                result.ConnectionName, _userId);
        }
    }
}
```

## Connection Persistence

### User Preferences

Connection preferences are stored per user:

- User-specific connection selection
- Connection preferences in user profile
- Automatic connection restoration on login

### Connection Switching

Users can switch connections after initial setup:

```razor
@code {
    private async Task SwitchConnection(string connectionName)
    {
        var result = await ConnectionService.SetCurrentConnectionAsync(
            connectionName, _userId);
        if (result.Success)
        {
            // Refresh application state
            await RefreshApplicationState();
        }
    }
}
```

## Connection Validation

### Connection Test

Before setting a connection, always test it:

```razor
@code {
    private async Task<bool> TestConnection(string connectionName)
    {
        var result = await ConnectionService.TestConnectionAsync(connectionName);
        return result.Success;
    }
}
```

### Connection Status

Display connection status in UI:

```razor
<MudChip Color="@(_connectionStatus == "Connected" ? Color.Success : Color.Error)"
         Size="Size.Small">
    @_connectionStatus
</MudChip>
```

## Error Handling

### Connection Errors

Handle connection errors gracefully:

```razor
@code {
    private async Task SetConnection(string connectionName)
    {
        try
        {
            var result = await ConnectionService.SetCurrentConnectionAsync(
                connectionName, _userId);
            if (!result.Success)
            {
                Snackbar.Add($"Error: {result.ErrorMessage}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error setting connection: {ex.Message}", Severity.Error);
        }
    }
}
```

## Best Practices

### 1. Always Check Connection

Before data operations, verify connection is set:

```razor
var connection = await ConnectionService.GetCurrentConnectionAsync();
if (connection == null)
{
    NavigationManager.NavigateTo("/first-login");
    return;
}
```

### 2. Test Before Use

Always test connection before setting as current.

### 3. User Feedback

Provide clear feedback during connection operations:

```razor
<MudProgressLinear Indeterminate="@_isTesting" />
<MudAlert Severity="@_testResult.Success ? Severity.Success : Severity.Error">
    @_testResult.Message
</MudAlert>
```

### 4. Connection Caching

Cache connection information to avoid repeated API calls.

## Related Documentation

- [API Integration](beep-oilgas-web-api-integration.md)
- [Services](beep-oilgas-web-services.md)
- [Authentication](beep-oilgas-web-authentication.md)
- [Data Management](beep-oilgas-web-datamanagement.md)

