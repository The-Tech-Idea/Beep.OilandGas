# Beep.OilandGas.UserManagement

Platform-agnostic user management and authorization library for Oil & Gas applications. Can be used in desktop applications, APIs, web services, and any .NET environment.

## Features

- **Platform-Agnostic**: No dependencies on ASP.NET Core (core library)
- **Industry Standards**: Built-in Oil & Gas roles and permissions
- **Row-Level Security**: Filter data by creator, source, tenant, or custom rules
- **Data Source Access Control**: Restrict access to databases and data sources
- **Source-Based Filtering**: Filter data by source system (SCADA, ManualEntry, etc.)
- **Extensible**: Easy to integrate with ASP.NET Core via separate integration library

## Architecture

### Core Library (`Beep.OilandGas.UserManagement`)
- Platform-agnostic services and interfaces
- Can be used directly in desktop apps, APIs, background services
- No ASP.NET Core dependencies

### ASP.NET Core Integration (`Beep.OilandGas.UserManagement.AspNetCore`)
- Web-specific integration for ASP.NET Core
- Authorization handlers and middleware
- Policy-based authorization support

## Quick Start

### Desktop Application

```csharp
using Beep.OilandGas.UserManagement.DependencyInjection;
using Beep.OilandGas.UserManagement.Core.Authorization;
using Beep.OilandGas.Models.Core.Interfaces.Security;

// Register services
var services = new ServiceCollection();
services.AddUserManagement()
    .AddScoped<IUserService, YourUserService>()
    .AddScoped<IAuthorizationService, AuthorizationService>()
    .AddScoped<IRoleService, YourRoleService>()
    .AddScoped<IPermissionService, YourPermissionService>();

var serviceProvider = services.BuildServiceProvider();

// Use authorization service
var authService = serviceProvider.GetRequiredService<IAuthorizationService>();
var hasPermission = await authService.UserHasPermissionAsync(userId, "Module:ProductionAccounting:Read");

// Use row-level security
var rlsProvider = serviceProvider.GetRequiredService<IRowLevelSecurityProvider>();
var filters = await rlsProvider.GetRowFiltersAsync(userId, "WELL");
```

### ASP.NET Core Web Application

```csharp
using Beep.OilandGas.UserManagement.DependencyInjection;
using Beep.OilandGas.UserManagement.AspNetCore.DependencyInjection;

// In Program.cs or Startup.cs
services.AddUserManagement()
    .AddUserManagementAspNetCore() // Adds ASP.NET Core integration
    .AddScoped<IUserService, YourUserService>()
    .AddScoped<IAuthorizationService, AuthorizationService>();

// Add permission-based policies
services.AddPermissionPolicy("ProductionAccounting:Read", "Module:ProductionAccounting:Read");

// Use in controllers
[Authorize(Policy = "ProductionAccounting:Read")]
public IActionResult GetProductionData() { }
```

### API Service (REST Endpoints)

The API controllers are available in `Beep.OilandGas.ApiService/Controllers/UserManagement/`:

- `GET /api/usermanagement/authorization/user/{userId}/permissions` - Get user permissions
- `POST /api/usermanagement/authorization/check-permission` - Check permission
- `GET /api/usermanagement/datasource/user/{userId}/accessible` - Get accessible data sources
- `GET /api/usermanagement/rls/user/{userId}/filters/{tableName}` - Get RLS filters

## Row-Level Security

### Creator-Based Filtering

Users see only rows they created:

```csharp
var rlsService = serviceProvider.GetRequiredService<RowLevelSecurityService>();
rlsService.AddRule(new RowLevelSecurityRule
{
    RuleType = RowLevelSecurityRuleType.Creator,
    TableName = "*", // Apply to all tables
    Priority = 1
});

// When querying data, filters are automatically applied
var filters = await rlsProvider.GetRowFiltersAsync(userId, "PRODUCTION");
// Returns: ROW_CREATED_BY = userId
```

### Source-Based Filtering

Users see only data from specific sources:

```csharp
rlsService.AddRule(new RowLevelSecurityRule
{
    RuleType = RowLevelSecurityRuleType.Source,
    TableName = "PRODUCTION",
    SourceValues = new[] { "SCADA", "ManualEntry" },
    Priority = 2
});
```

### Tenant-Based Filtering

Multi-tenant isolation:

```csharp
rlsService.AddRule(new RowLevelSecurityRule
{
    RuleType = RowLevelSecurityRuleType.Tenant,
    TableName = "*",
    TenantId = user.TENANT_ID,
    Priority = 3
});
```

## Industry Standards

### Standard Roles

```csharp
using Beep.OilandGas.UserManagement.Industry;

// Use standard roles
OilGasRoles.Administrator
OilGasRoles.PetroleumEngineer
OilGasRoles.ReservoirEngineer
OilGasRoles.Accountant
// ... and more
```

### Standard Permissions

```csharp
using Beep.OilandGas.UserManagement.Industry;

// Module permissions
OilGasPermissions.ProductionAccounting.Read
OilGasPermissions.NodalAnalysis.Execute

// Data source permissions
OilGasPermissions.DataSourceAccess("ProductionDB")

// Asset permissions
OilGasPermissions.AssetAccess("Well", "12345", "Read")
```

## Data Access Filtering

Combine all access control filters:

```csharp
var filterProvider = serviceProvider.GetRequiredService<IDataAccessFilterProvider>();
var allFilters = await filterProvider.CombineFiltersAsync(userId, "PRODUCTION", existingFilters);
// Combines: RLS filters + Source filters + Data source filters
```

## Project Structure

```
Beep.OilandGas.UserManagement/
├── Core/
│   ├── Authorization/     # Core authorization interfaces
│   ├── Authentication/     # User principal abstraction
│   ├── DataAccess/         # RLS and data access control
│   ├── Audit/              # Audit logging interfaces
│   ├── Session/            # Session management interfaces
│   └── Security/           # Password policy interfaces
├── Services/               # Service implementations
├── Industry/               # Oil & Gas standards
└── DependencyInjection/    # DI extensions
```

## Dependencies

- `Beep.OilandGas.Models` - Core models and interfaces
- `Microsoft.Extensions.DependencyInjection` - DI support
- `TheTechIdea.Beep.Report` - AppFilter support

## License

MIT
