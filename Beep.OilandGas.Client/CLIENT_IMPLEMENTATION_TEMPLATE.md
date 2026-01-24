# Client Implementation Template

This document provides a template and guidelines for implementing service classes in the `Beep.OilandGas.Client` SDK.

## Architecture Overview

The SDK uses a **unified service architecture** where each service:
- Extends `ServiceBase` for common HTTP/local functionality
- Has access to `BeepOilandGasApp` context (connection, user, data source)
- Supports both **Remote** (HTTP API) and **Local** (DI) access modes
- Uses **partial classes** for better code organization
- Uses **proper entity types** from `Beep.OilandGas.Models` (not `object`)

```
┌─────────────────────────────────────────────────────────────┐
│                    BeepOilandGasApp                         │
│                   (IBeepOilandGasApp)                       │
│  - Connection management                                    │
│  - User context                                             │
│  - Data source selection                                    │
├─────────────────────────────────────────────────────────────┤
│                      Services                               │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐       │
│  │ Analysis │ │  Pumps   │ │Properties│ │Calculat. │       │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘       │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐       │
│  │Production│ │LifeCycle │ │ Drilling │ │  Well    │       │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘       │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐       │
│  │Accounting│ │ Permits  │ │  Lease   │ │DataMgmt  │       │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘       │
└─────────────────────────────────────────────────────────────┘
```

## Implementation Pattern

### Interface Definition

```csharp
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.{Domain};
using Beep.OilandGas.Models.Data.{Domain};

namespace Beep.OilandGas.Client.App.Services.{ServiceName}
{
    /// <summary>
    /// Service interface for {ServiceName} operations
    /// </summary>
    public interface I{ServiceName}Service
    {
        #region {SubDomain}

        Task<EntityResult> GetEntityAsync(string id, CancellationToken cancellationToken = default);
        Task<EntityResult> CreateEntityAsync(EntityRequest request, CancellationToken cancellationToken = default);
        Task<EntityResult> UpdateEntityAsync(string id, EntityRequest request, CancellationToken cancellationToken = default);
        Task<List<EntityResult>> GetEntitiesAsync(string parentId, CancellationToken cancellationToken = default);
        Task<EntityResult> SaveEntityAsync(EntityResult entity, string? userId = null, CancellationToken cancellationToken = default);

        #endregion
    }
}
```

### Main Service Class (Partial)

```csharp
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App.Services.{ServiceName}
{
    /// <summary>
    /// Unified service for {ServiceName} operations
    /// </summary>
    internal partial class {ServiceName}Service : ServiceBase, I{ServiceName}Service
    {
        public {ServiceName}Service(BeepOilandGasApp app, ILogger<{ServiceName}Service>? logger = null)
            : base(app, logger)
        {
        }
    }
}
```

### Partial Class for Sub-Domain

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.{Domain};
using Beep.OilandGas.Models.Data.{Domain};

namespace Beep.OilandGas.Client.App.Services.{ServiceName}
{
    internal partial class {ServiceName}Service
    {
        #region {SubDomain}

        public async Task<EntityResult> GetEntityAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<EntityResult>($"/api/{endpoint}/{Uri.EscapeDataString(id)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<EntityResult> CreateEntityAsync(EntityRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<EntityRequest, EntityResult>("/api/{endpoint}/create", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<EntityResult> UpdateEntityAsync(string id, EntityRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PutAsync<EntityRequest, EntityResult>($"/api/{endpoint}/{Uri.EscapeDataString(id)}", request, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<List<EntityResult>> GetEntitiesAsync(string parentId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(parentId)) throw new ArgumentNullException(nameof(parentId));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<EntityResult>>($"/api/{endpoint}/parent/{Uri.EscapeDataString(parentId)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<EntityResult> SaveEntityAsync(EntityResult entity, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams("/api/{endpoint}/save", queryParams);
                return await PostAsync<EntityResult, EntityResult>(endpoint, entity, cancellationToken);
            }
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        #endregion
    }
}
```

## Steps to Implement a New Service

1. **Examine the Controller**: Read the corresponding controller file in `Beep.OilandGas.ApiService/Controllers/`
2. **Identify Endpoints**: List all HTTP methods (GET, POST, PUT, DELETE) and their routes
3. **Map Entity Types**: Use proper entity types from `Beep.OilandGas.Models`:
   - `Models/{Domain}/` - Model classes for calculations
   - `Data/{Domain}/` - PPDM-style entity classes for persistence
4. **Create Interface**: Define `I{ServiceName}Service.cs` with proper entity types
5. **Create Main Class**: Create `{ServiceName}Service.cs` extending `ServiceBase`
6. **Create Partial Classes**: Create `{ServiceName}Service.{SubDomain}.cs` for each sub-domain
7. **Register in App**: Add lazy initialization in `BeepOilandGasApp.cs`
8. **Update Interface**: Add property in `IBeepOilandGasApp.cs`

## Entity Type Guidelines

### DO Use Proper Entity Types

```csharp
// CORRECT - Using specific entity types
Task<ChokeFlowResult> CalculateChokeFlowAsync(GasChokeProperties request, CancellationToken ct);
Task<DRILLING_OPERATION> GetDrillingStatusAsync(string wellId, CancellationToken ct);
Task<List<PROSPECT>> GetProspectsAsync(string areaId, CancellationToken ct);
```

### DON'T Use Object Types

```csharp
// WRONG - Avoid using object
Task<object> CalculateChokeFlowAsync(object request, CancellationToken ct);
Task<object> GetDrillingStatusAsync(string wellId, CancellationToken ct);
```

### Entity Type Sources

| Source | Usage | Example |
|--------|-------|---------|
| `Beep.OilandGas.Models.{Domain}` | Calculation models | `ChokeFlowResult`, `FlashConditions` |
| `Beep.OilandGas.Models.Data.{Domain}` | PPDM entities | `DRILLING_OPERATION`, `PROSPECT` |

### Creating New Entities

If an entity doesn't exist, create it in `Beep.OilandGas.Models`:

1. **Entity Class**: Create in `Data/{Domain}/` following PPDM pattern
2. **SQL Scripts**: Create TAB, PK, FK scripts in `Scripts/Sqlserver/{Domain}/`

## File Organization

```
App/Services/{ServiceName}/
├── I{ServiceName}Service.cs          # Interface with all method signatures
├── {ServiceName}Service.cs           # Main partial class with constructor
├── {ServiceName}Service.{Sub1}.cs    # Partial class for sub-domain 1
├── {ServiceName}Service.{Sub2}.cs    # Partial class for sub-domain 2
└── {ServiceName}Service.{Sub3}.cs    # Partial class for sub-domain 3
```

## Current Services Implementation Status

| Service | Interface | Implementation | Entity Types |
|---------|-----------|----------------|--------------|
| Analysis | ✅ | ✅ | ✅ |
| Pumps | ✅ | ✅ | ✅ |
| Properties | ✅ | ✅ | ✅ |
| Calculations | ✅ | ✅ | ✅ |
| Production | ✅ | ✅ | ✅ |
| LifeCycle | ✅ | ✅ | ✅ |
| Drilling | ✅ | ✅ | ✅ |
| Well | ✅ | ⚠️ Needs entity update | ⚠️ |
| DataManagement | ✅ | ⚠️ Needs entity update | ⚠️ |
| Field | ✅ | ⚠️ Needs entity update | ⚠️ |
| Operations | ✅ | ⚠️ Needs entity update | ⚠️ |
| Accounting | ✅ | ⚠️ Needs entity update | ⚠️ |
| AccessControl | ✅ | ⚠️ Needs entity update | ⚠️ |
| Connection | ✅ | ⚠️ Needs entity update | ⚠️ |
| Permits | ✅ | ⚠️ Needs entity update | ⚠️ |
| Lease | ✅ | ⚠️ Needs entity update | ⚠️ |

## Common Patterns

### Route Mapping
- Controller route: `[Route("api/[controller]")]` → Service endpoint: `/api/{controllername}`
- Method route: `[HttpPost("design")]` → Service endpoint: `/api/{controllername}/design`

### Query Parameters
```csharp
var queryParams = new Dictionary<string, string>();
if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
var endpoint = BuildRequestUriWithParams("/api/{endpoint}", queryParams);
```

### Error Handling
- Validate required parameters (throw `ArgumentException` or `ArgumentNullException`)
- Let `ServiceBase` handle HTTP errors automatically

### Access Mode Check
```csharp
if (AccessMode == ServiceAccessMode.Remote)
    return await GetAsync<EntityType>("/api/endpoint", cancellationToken);
// For local mode, inject and call the actual service
throw new InvalidOperationException("Local mode not yet implemented");
```

## Notes

- All services inherit from `ServiceBase`
- All services have access to `BeepOilandGasApp` context via `App` property
- All services support `CancellationToken` for async operations
- Use `Uri.EscapeDataString()` for URL path parameters
- Use `BuildRequestUriWithParams()` for query parameters
- Follow naming conventions: `Get{X}Async`, `Create{X}Async`, `Update{X}Async`, `Delete{X}Async`, `Save{X}Async`
- Use proper entity types from `Beep.OilandGas.Models` - **never use `object`**
