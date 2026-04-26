# Beep.OilandGas

A comprehensive **Oil & Gas Engineering and Data Management Platform** built on the PPDM 3.9 data model. The system provides end-to-end lifecycle management for petroleum assets — from exploration and development through production, reservoir management, economics, HSE compliance, and decommissioning.

## Architecture

The solution follows a **three-layer architecture**:

```
┌─────────────────────────────────────────────────┐
│  Web Layer (Blazor Server)                      │
│  Beep.OilandGas.Web                             │
│  • Business process pages                       │
│  • Data management UI                           │
│  • MudBlazor components                         │
└──────────────────────┬──────────────────────────┘
                       │ HTTP / SignalR
┌──────────────────────▼──────────────────────────┐
│  API Layer (ASP.NET Core)                       │
│  Beep.OilandGas.ApiService                      │
│  • Controllers (field-scoped endpoints)         │
│  • FieldOrchestrator (current field context)    │
│  • JWT Authentication                           │
└──────────────────────┬──────────────────────────┘
                       │ DI / Service calls
┌──────────────────────▼──────────────────────────┐
│  Data Layer (Beep Framework + PPDM 3.9)         │
│  • PPDMGenericRepository (metadata-driven CRUD) │
│  • IDMEEditor (database-agnostic abstraction)   │
│  • Single PPDM39 schema for all lifecycle data  │
└─────────────────────────────────────────────────┘
```

### Key Principles

- **Single Database**: All lifecycle data (Exploration/Development/Production/Decommissioning) in a unified PPDM39 schema.
- **Field-Scoped Operations**: All business operations are scoped to a current field via `IFieldOrchestrator`.
- **Metadata-Driven Data Access**: `PPDMGenericRepository` uses `IPPDMMetadataRepository` for table discovery and `AppFilter` for database-agnostic queries.
- **No Mock Data**: Every page loads from live API endpoints. No hardcoded lists or TODO stubs.

## Solution Structure

### Core Projects

| Project | Purpose |
|---------|---------|
| `Beep.OilandGas.ApiService` | ASP.NET Core API — controllers, DI setup, JWT auth |
| `Beep.OilandGas.Web` | Blazor Server UI — business process pages, data management |
| `Beep.OilandGas.PPDM39` | PPDM 3.9 models, interfaces, DTOs (central hub) |
| `Beep.OilandGas.PPDM39.DataManagement` | PPDM services, module setup, seed data, validation |
| `Beep.OilandGas.Models` | Shared models, core interfaces, entity base classes |
| `Beep.OilandGas.LifeCycle` | Field orchestrator, exploration services, phase coordination |

### Domain Projects (Oil & Gas Lifecycle)

| Project | Domain |
|---------|--------|
| `Beep.OilandGas.ProspectIdentification` | Exploration — prospects, leads, plays, seismic, risk assessment |
| `Beep.OilandGas.DevelopmentPlanning` | Development — well planning, facility design, FDP |
| `Beep.OilandGas.DrillingAndConstruction` | Drilling — well construction, operations, programs |
| `Beep.OilandGas.ProductionOperations` | Production — well operations, optimization, work orders |
| `Beep.OilandGas.ProductionAccounting` | Accounting — allocation, royalties, run tickets, reserves |
| `Beep.OilandGas.ProductionForecasting` | Forecasting — decline curves, production predictions |
| `Beep.OilandGas.NodalAnalysis` | Nodal analysis — well performance, IPR/VLP |
| `Beep.OilandGas.Reservoir` | Reservoir engineering — material balance, simulation |
| `Beep.OilandGas.EconomicAnalysis` | Economics — NPV, IRR, scenario analysis |
| `Beep.OilandGas.HSE` | Health, Safety & Environment — incidents, HAZOP, barriers |
| `Beep.OilandGas.Decommissioning` | Decommissioning — abandonment, cost estimates, restoration |
| `Beep.OilandGas.PermitsAndApplications` | Permits — regulatory applications, compliance, workflows |
| `Beep.OilandGas.LeaseAcquisition` | Land & leases — mineral rights, agreements, payments |

### Analysis & Engineering Tools

| Project | Purpose |
|---------|---------|
| `Beep.OilandGas.DCA` | Decline curve analysis |
| `Beep.OilandGas.WellTestAnalysis` | Well test interpretation |
| `Beep.OilandGas.GasLift` | Gas lift design and optimization |
| `Beep.OilandGas.SuckerRodPumping` | SRP design and analysis |
| `Beep.OilandGas.PlungerLift` | Plunger lift analysis |
| `Beep.OilandGas.PumpPerformance` | Pump performance evaluation |
| `Beep.OilandGas.PipelineAnalysis` | Pipeline hydraulics and pressure drop |
| `Beep.OilandGas.CompressorAnalysis` | Compressor performance |
| `Beep.OilandGas.FlashCalculations` | PVT and flash calculations |
| `Beep.OilandGas.GasProperties` | Gas property calculations |
| `Beep.OilandGas.OilProperties` | Oil property calculations |
| `Beep.OilandGas.EnhancedRecovery` | EOR methods — waterflood, gas injection |
| `Beep.OilandGas.Drawing` | Well schematics and diagram rendering |
| `Beep.OilandGas.HeatMap` | Heat map visualization |

### Infrastructure Projects

| Project | Purpose |
|---------|---------|
| `Beep.OilandGas.Client` | HTTP client library for API consumption |
| `Beep.OilandGas.DataManager` | Data management utilities |
| `Beep.OilandGas.UserManagement` | Identity, roles, user personas |
| `Beep.OilandGas.UserManagement.AspNetCore` | ASP.NET Core identity integration |
| `Beep.OilandGas.ApiService.Tests` | API integration and unit tests |
| `Beep.OilandGas.PermitsAndApplications.Tests` | Permit workflow tests |

## Data Access Pattern (Canonical)

All CRUD operations follow this metadata-driven template:

```csharp
// Get metadata and entity type
var metadata = await _metadata.GetTableMetadataAsync("TABLE_NAME");
var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");

// Create repository
var repo = new PPDMGenericRepository(
    _editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "TABLE_NAME");

// Build filters using AppFilter (database-agnostic)
var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
};

// Execute
var entities = await repo.GetAsync(filters);        // Query
var single = await repo.GetByIdAsync(id);           // Get by ID
await repo.InsertAsync(entity, userId);             // Insert
await repo.UpdateAsync(entity, userId);             // Update
await repo.SoftDeleteAsync(id, userId);             // Soft delete
```

## Service Registration Pattern

Services are registered in `Program.cs` using the factory pattern:

```csharp
builder.Services.AddScoped<IMyService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<MyService>();
    
    return new MyService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});
```

**Registration order matters**: `AddBeepServices` → metadata/defaults → domain services.

## Quick Start

### Prerequisites

- .NET 9 SDK or later
- A supported database (SQL Server, PostgreSQL, SQLite, Oracle, MySQL/MariaDB)
- Identity Server for authentication (dev: `https://localhost:7062/`)

### Build

```bash
dotnet build Beep.OilandGas.sln
```

### Run API

```bash
dotnet run --project Beep.OilandGas.ApiService
```

Swagger UI: `http://localhost:5000/swagger`

### Run Web

```bash
dotnet run --project Beep.OilandGas.Web
```

### View Logs

```bash
tail -f logs/beep-oilgas-api-*.txt
```

## Key Conventions

### Naming

- **Projects**: `Beep.OilandGas.{DomainName}` (e.g., `Beep.OilandGas.ProductionAccounting`)
- **Interfaces**: `I{Name}Service` in `.Core.Interfaces`
- **Implementations**: `{Name}Service` in `.DataManagement.Services`
- **DTOs**: PascalCase with suffix `Request`, `Response`, `Result`, `Summary` — in `Beep.OilandGas.PPDM39/Core/DTOs`
- **Table Classes**: PPDM SCREAMING_SNAKE_CASE, extend `ModelEntityBase`, scalar properties only

### Controller Pattern

All field-scoped endpoints use `/api/field/current/*`:

```csharp
[ApiController]
[Route("api/field/current")]
[Authorize]
public class MyController : ControllerBase
{
    private readonly IFieldOrchestrator _fieldOrchestrator;
    private readonly IMyService _service;
    
    [HttpGet("{id}")]
    public async Task<ActionResult<MyDto>> GetAsync(string id)
    {
        var fieldId = _fieldOrchestrator.CurrentFieldId;
        var result = await _service.GetByIdAsync(fieldId, id);
        return Ok(result);
    }
}
```

### Web Services

- **ApiClient**: Generic HTTP wrapper for REST calls
- **DataManagementService**: Centralized CRUD, connection state, import/export
- **ProgressTrackingClient**: SignalR for real-time operation progress

### UI/UX

- MudBlazor components with `Color.*` enum (no hardcoded colors)
- Dark mode support via theme CSS variables
- SVG icons only (no PNG/JPG for new assets)
- Business process pages follow engineer workflows (not CRUD)
- Navigation: Dashboard → Exploration → Development → Production → Reservoir → Economics → HSE → Data Management

## Documentation

| Document | Location |
|----------|----------|
| Copilot Instructions | `.github/copilot-instructions.md` |
| Architecture Patterns | `.cursor/commands/architecture-patterns.md` |
| Data Access Patterns | `.cursor/commands/data-access-patterns.md` |
| Naming Conventions | `.cursor/commands/naming-conventions.md` |
| Best Practices | `.cursor/commands/best-practices.md` |
| UI/UX Guidelines | `Plans/UI-UX-OilAndGas-Guidelines.md` |
| Business Processes | `Plans/BusinessProcessesPlan/PetroleumEngineerBusinessProcesses.md` |
| MudBlazor Docs | `Beep.OilandGas.Web/MudBlazor_Docs/README.md` |

## Database Scripts

Scripts are organized by database type under `Scripts/`:

```
Scripts/
├── SqlServer/    (TAB, PK, FK subdirectories)
├── PostgreSQL/
├── SQLite/
├── Oracle/
├── MySQL/
└── MariaDB/
```

## Authentication

- JWT tokens from Identity Server
- All API endpoints require `[Authorize]`
- User ID extracted: `User.FindFirst(ClaimTypes.NameIdentifier)?.Value`
- Token refresh and revocation handled by framework

## Adding New Features

1. **DTOs/Interfaces** → `Beep.OilandGas.PPDM39/Core/{Interfaces,DTOs}`
2. **Service** → `Beep.OilandGas.PPDM39.DataManagement/Services/{DomainName}/` + register in `Program.cs`
3. **Controller** → `Beep.OilandGas.ApiService/Controllers/{DomainName}/`
4. **Web UI** → `Beep.OilandGas.Web/Pages/PPDM39/{DomainName}/`
5. **Test** → `dotnet build` → `dotnet run --project Beep.OilandGas.ApiService` → Check Swagger and logs

## License

See [LICENSE.txt](LICENSE.txt) for details.
