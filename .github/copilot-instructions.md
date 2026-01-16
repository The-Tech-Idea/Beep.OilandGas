# Beep.OilandGas — GitHub Copilot Instructions

Purpose: orient GitHub Copilot to write code that aligns with Beep.OilandGas architecture, DI patterns, and PPDM39 data access conventions.

## Essential Reading (Start Here)

1. **Architecture**: Three layers: Web (Blazor) → API (ASP.NET Core) → Data (Beep Framework + PPDM39).
2. **Single Database**: All lifecycle data (Exploration/Development/Production/Decommissioning) in PPDM39 schema.
3. **DI Order Matters**: `AddBeepServices` must come first; then metadata/defaults; then services that consume `IDMEEditor`.
4. **FieldOrchestrator**: Central coordinator for field-scoped operations; all endpoints under `/api/field/current/*`.

## Quick Commands

```bash
# Build
dotnet build Beep.OilandGas.sln

# Run API
dotnet run --project Beep.OilandGas.ApiService

# View logs
tail -f logs/beep-oilgas-api-*.txt
```

## Key Files to Understand

| File | Why | Read Lines |
|------|-----|-----------|
| `Beep.OilandGas.ApiService/Program.cs` | DI registration order; Beep setup; JWT config | 1–120 |
| `Beep.OilandGas.Web/Services/DataManagementService.cs` | Connection state management; cache patterns | 1–50 |
| `Beep.OilandGas.PPDM39.DataManagement/Services/Exploration/PPDMExplorationService.cs` | Service implementation example using PPDMGenericRepository | Entire file |
| `Beep.OilandGas.ApiService/Controllers/Field/FieldOrchestratorController.cs` | Field-scoped API pattern | 1–50 |

## Data Access Pattern (Canonical)

All CRUD must follow this template:

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

**Key Points**:
- Use `AppFilter` for all queries (database-agnostic).
- Let PPDMGenericRepository handle parameter delimiters.
- String IDs must be formatted: `_defaults.FormatIdForTable("TABLE_NAME", id)`.

## Service Registration Pattern

All new services must follow this in `Program.cs`:

```csharp
builder.Services.AddScoped<IMyNewService>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<MyNewService>();
    
    return new MyNewService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
});
```

**Constraints**:
- Place **after** `IPPDMMetadataRepository` and `IPPDM39DefaultsRepository` registration.
- Use factory pattern (`AddScoped<I...>(sp => new ...)`) — never parameterless.
- Inject only what the service constructor needs.

## DTO Organization

All DTOs and interfaces belong in **`Beep.OilandGas.PPDM39`**:

```
Beep.OilandGas.PPDM39/
├── Core/
│   ├── Interfaces/
│   │   └── IMyService.cs
│   └── DTOs/
│       └── MyServiceDTOs.cs
```

**Why**: PPDM39 is the hub; other projects depend on it. Spreading DTOs breaks discovery.

## Naming Conventions

- **Projects**: `Beep.OilandGas.{DomainName}` (e.g., `Beep.OilandGas.ProductionAccounting`).
- **Namespaces**: Interfaces in `.Core.Interfaces`; DTOs in `.Core.DTOs`; implementations in `.DataManagement.Services`.
- **Classes**: `I{Name}Service` for interfaces; `{Name}Service` for implementations.
- **DB Columns**: PPDM standard (e.g., `FIELD_ID`, `WELL_ID`, `ACTIVE_IND`, `CREATE_USER`, `CREATE_DATE`).

## Code Patterns to Follow

### Controller Pattern
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

### Logging Pattern
```csharp
Log.Information("Starting operation for field {FieldId}", fieldId);
try
{
    var result = await _service.DoWorkAsync(fieldId);
    Log.Information("Operation completed for field {FieldId}", fieldId);
    return result;
}
catch (Exception ex)
{
    Log.Error(ex, "Operation failed for field {FieldId}", fieldId);
    throw;
}
```

### Validation Pattern
```csharp
if (string.IsNullOrWhiteSpace(fieldId))
    throw new ArgumentNullException(nameof(fieldId), "Field ID is required");

var fieldExists = await _repo.GetByIdAsync(fieldId);
if (fieldExists == null)
    throw new OperationCanceledException($"Field {fieldId} not found");
```

## Common Anti-Patterns (Avoid)

❌ **Wrong**: Registering service before `IDMEEditor` → Startup crash.  
✅ **Right**: Follow the `Program.cs` registration order.

❌ **Wrong**: DTOs in random projects → Metadata discovery fails.  
✅ **Right**: All DTOs in `Beep.OilandGas.PPDM39/Core/DTOs`.

❌ **Wrong**: Using `ExecuteSql` for SELECT → No parameter delimiter safety.  
✅ **Right**: Use `GetEntityAsync` with `AppFilter` or `RunQuery` for complex SQL.

❌ **Wrong**: Creating database-specific code → Breaks portability.  
✅ **Right**: Use `IDMEEditor` abstraction; let Beep handle driver differences.

❌ **Wrong**: Separate databases per feature → Data fragmentation.  
✅ **Right**: Single PPDM39 schema for all lifecycle data.

## Reference Docs (Canonical Truth)

- `.cursor/commands/architecture-patterns.md` — Three-layer architecture, controller/service/DTO patterns.
- `.cursor/commands/beep-dataaccess-generic-repository.md` — PPDMGenericRepository CRUD details.
- `.cursor/commands/data-access-patterns.md` — GetEntityAsync vs ExecuteSql vs RunQuery rules.
- `.cursor/commands/naming-conventions.md` — Project/namespace/file naming rules.
- `.cursor/commands/best-practices.md` — FASB compliance, accounting patterns.

## Build & Test

```bash
# Full build
dotnet build Beep.OilandGas.sln

# Run API locally (check http://localhost:5000/swagger)
dotnet run --project Beep.OilandGas.ApiService

# View recent logs
tail -f logs/beep-oilgas-api-*.txt

# Look for DI or startup errors
grep -i "error\|exception" logs/beep-oilgas-api-*.txt | head -20
```

## When Adding New Features

1. **Add DTOs/Interfaces** → `Beep.OilandGas.PPDM39/Core/{Interfaces,DTOs}`.
2. **Add Service** → `Beep.OilandGas.PPDM39.DataManagement/Services/{DomainName}/` + register in `Program.cs`.
3. **Add Controller** → `Beep.OilandGas.ApiService/Controllers/{DomainName}/`.
4. **Add Web UI** → `Beep.OilandGas.Web/Pages/PPDM39/{DomainName}/`.
5. **Test locally** → `dotnet build` → `dotnet run --project Beep.OilandGas.ApiService` → Check logs and Swagger.

## Web Architecture & Services

The Blazor Server app (`Beep.OilandGas.Web`) uses three core service layers:

1. **ApiClient** — Generic HTTP wrapper for all REST calls
   ```csharp
   var fields = await ApiClient.GetAsync<List<Field>>("/api/field/fields");
   var result = await ApiClient.PostAsync<Request, Response>("/endpoint", request);
   ```

2. **DataManagementService** — Centralized data + connection management
   - Current connection state (cached, 5-min timeout)
   - CRUD for any PPDM entity via `GetEntitiesAsync`, `InsertEntityAsync`, `UpdateEntityAsync`, `DeleteEntityAsync`
   - Import/export operations with progress
   - Validation, quality checks, versioning, auditing
   - All long-running operations return `OperationId` for SignalR progress tracking

3. **ProgressTrackingClient** — SignalR for real-time updates
   ```csharp
   @inject ProgressTrackingClient ProgressClient

   protected override async Task OnInitializedAsync()
   {
       await ProgressClient.JoinOperationAsync(operationId);
   }

   private void OnProgressUpdate(ProgressUpdate update)
   {
       PercentComplete = update.PercentComplete;
       StateHasChanged();
   }
   ```

## Validation & Quality Patterns

### Validation Service
```csharp
var validationService = new PPDMDataValidationService(editor, commonColumnHandler, defaults, metadata);
var result = await validationService.ValidateAsync(entity, "TABLE_NAME");

if (!result.IsValid)
    throw new InvalidOperationException($"Validation failed: {string.Join(", ", result.Errors)}");
```

### Data Quality Service
```csharp
var qualityService = new PPDMDataQualityService(editor, commonColumnHandler, defaults, metadata);
var score = await qualityService.CalculateQualityScoreAsync(entity, "TABLE_NAME");
// Returns: OverallScore (%), FieldScores for each column
```

### Data Versioning Service
```csharp
var versioningService = new PPDMDataVersioningService(editor, commonColumnHandler, defaults, metadata);
var snapshot = await versioningService.CreateVersionAsync("TABLE_NAME", entity, userId, "Version Label");
var versions = await versioningService.GetVersionsAsync("TABLE_NAME", entityId);
```

### Data Access Audit Service
```csharp
var auditService = new PPDMDataAccessAuditService(editor, commonColumnHandler, defaults, metadata);
var history = await auditService.GetAccessHistoryByEntityAsync("TABLE_NAME", id, startDate, endDate);
// Returns: Who accessed what, when, what operation (Read/Write/Delete/Export)
```

## LOV (List of Values) Management

All dropdowns and picklists use the **R_*** reference tables. Use `LOVManagementService`:

```csharp
var lovService = new LOVManagementService(editor, commonColumnHandler, defaults, metadata);

// Get all values of a type
var wellTypes = await lovService.GetLOVByTypeAsync("WELL_TYPE");

// Get values by category
var statusValues = await lovService.GetLOVByTypeAsync("WELL_STATUS", "OPERATIONAL_STATUS");

// Create new LOV entry
var request = new CreateLOVRequest 
{ 
    ValueType = "WELL_TYPE", 
    Value = "DIRECTIONAL", 
    Description = "Directional Well",
    Category = "WELL_CLASSIFICATION"
};
var lov = await lovService.CreateLOVAsync(request, userId);
```

## Common Columns Management

The `CommonColumnHandler` auto-sets these on every entity:

| Column | On Insert | On Update | Notes |
|--------|-----------|-----------|-------|
| `ROW_CREATED_BY` | ✓ | — | Set to userId |
| `ROW_CREATED_DATE` | ✓ | — | Set to DateTime.UtcNow |
| `ROW_CHANGED_BY` | ✓ | ✓ | Set to userId |
| `ROW_CHANGED_DATE` | ✓ | ✓ | Set to DateTime.UtcNow |
| `ACTIVE_IND` | ✓ | — | Set to 'Y' (soft delete sets to 'N') |
| `PPDM_GUID` | ✓ | — | Auto-generated GUID |

**Query tip:** Use `GetActiveAsync()` to automatically filter by `ACTIVE_IND = 'Y'`.

## Database Scripts & Creation

**Script Organization by Database Type:**
```
Scripts/
├── SqlServer/
│   ├── TAB/ (CREATE TABLE)
│   ├── PK/ (PRIMARY KEY)
│   └── FK/ (FOREIGN KEY)
├── PostgreSQL/
├── SQLite/
├── Oracle/
├── MySQL/
└── MariaDB/
```

**Database Creation Service:**
```csharp
var creatorService = new PPDMDatabaseCreatorService(editor, logger);
var options = new DatabaseCreationOptions
{
    DatabaseType = DatabaseType.SqlServer,
    ConnectionString = "...",
    ScriptsPath = @"C:\Scripts",
    LogFilePath = @"C:\Logs\creation.log"
};
var result = await creatorService.CreateDatabaseAsync(options);
// Returns: ScriptResults with success/error per script, overall status
```

**Seed Data from CSV:**
```csharp
var seeder = new PPDMSeederOrchestrator(editor, commonColumnHandler, defaults, metadata);
var summary = await seeder.SeedAllAsync(@"C:\SeedData\CSV", userId);
// Returns: TotalRecordsSeeded, ProcessedFiles, Errors
```

## Mapping Service (DTO ↔ PPDM Model)

All conversions are type-safe (no Dictionary-based operations):

```csharp
var mappingService = new PPDMMappingService();

// DTO → PPDM Model
var prospectRequest = new ProspectRequest { ProspectName = "Eagle Ford", ... };
var prospect = mappingService.ConvertDTOToPPDMModel<PROSPECT, ProspectRequest>(prospectRequest);

// PPDM Model → DTO
var prospectResponse = mappingService.ConvertPPDMModelToDTO<PROSPECT, ProspectResponse>(prospect);
// Auto-maps property names: ProspectName → PROSPECT_NAME
```

## FieldOrchestrator & Lifecycle

The `IFieldOrchestrator` manages the **single active field** per user session:

```csharp
// In API endpoint
[HttpPost("set-active")]
public async Task<IActionResult> SetActiveField([FromBody] SetActiveFieldRequest request)
{
    var result = await _fieldOrchestrator.SetActiveFieldAsync(request.FieldId);
    if (!result) return BadRequest("Field not found");
    return Ok(new { CurrentFieldId = _fieldOrchestrator.CurrentFieldId });
}

// In phase service (auto-filtered by field)
public async Task<List<object>> GetProspectsForCurrentFieldAsync()
{
    var fieldId = _fieldOrchestrator.CurrentFieldId;
    // Service automatically adds FIELD_ID filter
    return await _explorationService.GetProspectsForFieldAsync(fieldId);
}
```

All phase endpoints live under `/api/field/current/{phase}/{operation}`.

## Metadata & Table Discovery

The `IPPDMMetadataRepository` provides metadata for all PPDM tables:

```csharp
var metadata = await _metadata.GetTableMetadataAsync("WELL");

// Properties available:
// - TableName
// - EntityTypeName (for Type.GetType)
// - PrimaryKeyColumn
// - Columns (all columns in table)
// - Relationships (foreign keys)
// - SubjectArea
// - Module

var allTables = await _metadata.GetAllTablesAsync();
var explorationTables = await _metadata.GetTablesBySubjectAreaAsync("EXPLORATION");
```

## PPDM Integration Patterns

**Before creating new tables, check PPDM first:**
- `BUSINESS_ASSOCIATE` — Use for vendors, customers, suppliers (with `BA_TYPE`)
- `CONTRACT` — Use for sales/purchase/service contracts
- `WELL` — Use with `WELL_TYPE` for different well categories
- `R_*` tables — Reference/LOV tables (do NOT insert directly; use LOVManagementService)

## Authentication & Authorization

- All endpoints require `[Authorize]` attribute
- Extract `UserId`: `var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value`
- JWT token from Identity Server (`https://localhost:7062/` in dev)
- Token refresh and revocation handled by framework

## Component & Page Standards

**Blazor Components:**
- Located in `Beep.OilandGas.Web/Components/` or `Pages/`
- Use `@inject` for services: `@inject IDataManagementService DataService`
- Implement `IAsyncDisposable` if subscribing to events
- Use `StateHasChanged()` after async operations that don't re-render automatically

**Pages (Razor):**
- Located in `Pages/PPDM39/{Phase}/`
- Follow pattern: Load → Display → Handle interactions
- Use `@page "/ppdm39/phase/operation"` for routing
- Inject `DataManagementService`, `ApiClient`, `ProgressTrackingClient`
