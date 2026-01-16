# Beep.OilandGas — Claude AI Assistance Guidelines

This guide helps Claude (and similar AI agents) make safe, well-scoped edits that align with Beep.OilandGas architecture and patterns.

## Essential Architecture Knowledge (Read First)

1. **Three-Layer Architecture**:
   - **Web Layer**: Blazor Server (Beep.OilandGas.Web) — UI and client-side caching
   - **API Layer**: ASP.NET Core (Beep.OilandGas.ApiService) — Controllers and business logic
   - **Data Layer**: Beep Framework (IDMEEditor, IDataSource) + PPDMGenericRepository — database abstraction

2. **Single PPDM39 Database Model**:
   - All lifecycle data (Exploration, Development, Production, Decommissioning) lives in one PPDM39 schema.
   - All DTOs, interfaces, and models must be in `Beep.OilandGas.PPDM39` project.
   - Use `PPDMGenericRepository` for all CRUD — it handles metadata-driven table resolution automatically.

3. **FieldOrchestrator Pattern**:
   - Manages current active field context; users work with one field at a time.
   - All field-scoped API endpoints live under `/api/field/current/*`.
   - Services automatically filter by `FIELD_ID` when called through FieldOrchestrator.

## Mandatory Pre-Edit Checklist

Before making any code change:

- [ ] Read `Beep.OilandGas.ApiService/Program.cs` lines 1–120 to confirm DI order (AddBeepServices must come before services that consume IDMEEditor).
- [ ] Confirm all new DTOs/interfaces live in `Beep.OilandGas.PPDM39/Core/` (not spread across projects).
- [ ] Verify service registration uses factory pattern: `AddScoped<IFoo>(sp => new Foo(sp.GetRequiredService<...>(), ...))`.
- [ ] If touching data access, use the PPDMGenericRepository pattern shown in `.cursor/commands/beep-dataaccess-generic-repository.md`.

## Data Access Pattern (Copy-Paste Template)

Use this pattern for any CRUD operation:

```csharp
// 1. Resolve metadata
var metadata = await _metadata.GetTableMetadataAsync("TABLE_NAME");
var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");

// 2. Create repository
var repo = new PPDMGenericRepository(
    _editor, _commonColumnHandler, _defaults, _metadata,
    entityType, _connectionName, "TABLE_NAME");

// 3. Query with AppFilter (PREFERRED)
var filters = new List<AppFilter>
{
    new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
};

// 4. Execute
var entities = await repo.GetAsync(filters);  // Get multiple
var entity = await repo.GetByIdAsync(id);     // Get single
await repo.InsertAsync(entity, userId);       // Create
await repo.UpdateAsync(entity, userId);       // Update
await repo.SoftDeleteAsync(id, userId);       // Delete
```

## Service Registration Pattern (Copy-Paste Template)

For any new service in `Program.cs`:

```csharp
// Register AFTER metadata/defaults, BEFORE services that depend on it
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

## Naming Conventions

- **Projects**: `Beep.OilandGas.{DomainName}` (e.g., `Beep.OilandGas.ProductionAccounting`)
- **Namespaces**: `Beep.OilandGas.{Project}.Core.Interfaces` for interfaces; `.Core.DTOs` for DTOs; `.DataManagement.Services` for implementations
- **Files**: `I{ServiceName}Service.cs` for interfaces; `{ServiceName}Service.cs` for implementations
- **DB IDs**: All string-based; use `_defaults.FormatIdForTable("TABLE_NAME", id)` to format

## Common Mistakes to Avoid

1. ❌ Registering service before `IDMEEditor` is available → Results in null ref at startup.
2. ❌ Adding DTOs to the wrong project (not `Beep.OilandGas.PPDM39`) → Breaks metadata discovery.
3. ❌ Using `ExecuteSql` for SELECT queries → Only use for DDL (CREATE, ALTER, DROP). Use `GetEntityAsync` or `RunQuery` instead.
4. ❌ Hardcoding SQL parameter delimiters → Use `dataSource.ParameterDelimiter` or let `AppFilter` handle it.
5. ❌ Creating separate databases per feature → Always use the single PPDM39 schema.

## Quick Verification Steps

```bash
# Build
dotnet build Beep.OilandGas.sln

# Run API (watch for DI startup errors)
dotnet run --project Beep.OilandGas.ApiService

# Check logs for trace
cat logs/beep-oilgas-api-*.txt | grep -i "error\|exception"
```

## Reference Documentation (Treat as Source-of-Truth)

- `.cursor/commands/architecture-patterns.md` — Three-layer patterns, service/API/DTO organization
- `.cursor/commands/beep-dataaccess-generic-repository.md` — PPDMGenericRepository CRUD examples
- `.cursor/commands/beep-dataaccess-overview.md` — Data access framework overview
- `.cursor/commands/naming-conventions.md` — Project, namespace, and file naming rules
- `.cursor/commands/data-access-patterns.md` — Detailed data access best practices (GetEntityAsync vs ExecuteSql, etc.)
- `.cursor/commands/best-practices.md` — FASB compliance, revenue recognition, accounting patterns

## Web Layer Patterns (Critical)

**ApiClient** (all HTTP calls go through this):
```csharp
// In Blazor pages
@inject ApiClient ApiClient

var result = await ApiClient.GetAsync<List<Field>>("/api/field/fields");
var response = await ApiClient.PostAsync<CreateRequest, CreateResponse>("/api/field/create", request);
```

**DataManagementService** (centralized data + connection management):
- Gets/sets current connection
- CRUD operations for any PPDM entity
- Import/export with progress
- Validation, quality checks, versioning
- All returns have `OperationId` for SignalR progress tracking

**Services use SignalR** for real-time progress:
```csharp
// Join operation progress group
await _progressClient.JoinOperationAsync(operationId);

// UI component listens to OnProgressUpdate event
private async Task OnProgressUpdate(ProgressUpdate update)
{
    PercentComplete = update.PercentComplete;
    StatusMessage = update.Message;
    StateHasChanged();
}
```

## PPDM Data Access Complete Pattern

**Always use this chain:**
1. Get metadata: `var metadata = await _metadata.GetTableMetadataAsync("TABLE_NAME")`
2. Get entity type: `Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")`
3. Create repository: `new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, _connectionName, "TABLE_NAME")`
4. Build filters: Use `AppFilter` with `FieldName`, `Operator` ("=", ">", "<", "LIKE", etc.), `FilterValue`
5. Execute: `await repo.GetAsync(filters)` / `InsertAsync` / `UpdateAsync` / `DeleteAsync` / `GetByIdAsync`

**Common columns auto-handled:**
- `ROW_CREATED_BY`, `ROW_CREATED_DATE` (on insert)
- `ROW_CHANGED_BY`, `ROW_CHANGED_DATE` (on update)
- `ACTIVE_IND` (soft delete sets to 'N')
- Use `GetActiveAsync()` to filter by `ACTIVE_IND = 'Y'`

## Authentication & Security

- All API endpoints require `[Authorize]` attribute
- JWT token from Identity Server (configured in `Program.cs`)
- For dev: Identity Server runs at `https://localhost:7062/`
- Extract `UserId` in controllers: `var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value`

## API Controllers Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MyController : ControllerBase
{
    private readonly IMyService _service;
    private readonly ILogger<MyController> _logger;
    
    [HttpPost("operation")]
    public async Task<IActionResult> DoOperationAsync([FromBody] OperationRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Log.Information("Operation started for user {UserId}", userId);
        var result = await _service.ExecuteAsync(request, userId);
        return Ok(result);
    }
}
```

## Service Validation Pattern

```csharp
// Validate inputs first
if (string.IsNullOrWhiteSpace(id))
    throw new ArgumentNullException(nameof(id));

// Use validation service
var validationService = new PPDMDataValidationService(editor, commonColumnHandler, defaults, metadata);
var validationResult = await validationService.ValidateAsync(entity, "TABLE_NAME");

if (!validationResult.IsValid)
    throw new InvalidOperationException($"Validation failed: {string.Join(", ", validationResult.Errors)}");
```

## LOV Management (List of Values)

```csharp
// Get LOV values for dropdown
var lovService = new LOVManagementService(editor, commonColumnHandler, defaults, metadata);
var wellTypes = await lovService.GetLOVByTypeAsync("WELL_TYPE");

// Each LOV has: Value, Description, Category, Type
foreach (var lov in wellTypes)
{
    // Use lov.Value in database, lov.Description in UI
}
```

## Versioning & Audit

```csharp
// Track versions of entities
var versioningService = new PPDMDataVersioningService(editor, commonColumnHandler, defaults, metadata);
var snapshot = await versioningService.CreateVersionAsync("WELL", well, userId, "Initial Version");

// Get audit trail
var auditService = new PPDMDataAccessAuditService(editor, commonColumnHandler, defaults, metadata);
var history = await auditService.GetAccessHistoryByEntityAsync("WELL", "WELL-001");
```

## Database Scripts & Seeding

- All DDL scripts in database-specific folders (SQL Server, PostgreSQL, SQLite, etc.)
- Scripts execute in order: TAB (tables) → PK (primary keys) → FK (foreign keys)
- Seed LOV data from CSV files using `PPDMSeederOrchestrator`
- Use `PPDMDatabaseCreatorService` to automate database creation with progress tracking

## FieldOrchestrator & Lifecycle

- Users set **one active field** per session
- All phase services filter by `FIELD_ID` automatically
- Phase services live in `PPDM39.DataManagement/Services/{Exploration,Development,Production,Decommissioning}`
- API endpoints: `/api/field/current/{phase}/{operation}` (field context injected)

## Quality & Data Health

- Use `PPDMDataQualityService` to score entities (completeness %)
- `PPDMDataQualityDashboardService` provides table-wide quality metrics
- Quality issues are categorized (missing fields, invalid formats, business rule violations)

## When in Doubt

- Check `Beep.OilandGas.PPDM39.DataManagement/Services/` for existing service implementation examples.
- Look at `Beep.OilandGas.ApiService/Controllers/` for controller patterns.
- Review `.cursor/commands/` files for the exact pattern you're trying to implement.
- All **web** patterns in `.cursor/commands/beep-oilgas-web-*.md`
- All **data** patterns in `.cursor/commands/beep-dataaccess-*.md`
