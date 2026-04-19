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
| `Beep.OilandGas.PPDM39.DataManagement/Services/Well/WellServices.cs` | Central well service — CRUD, PPDM 3.9 facets, status init | Entire file |
| `Beep.OilandGas.PPDM39.DataManagement/Services/Well/WellServices.WellStatus.cs` | Well status queries and facet management | Entire file |

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

## WellServices — Central Well Operations Service

**Rule**: Any feature that touches a well (CRUD, status, structure, child entities) MUST go through `WellServices`. Do NOT create raw `PPDMGenericRepository` instances for `WELL`, `WELL_STATUS`, or `WELL_XREF` outside this service.

### Namespace & Files
```
Beep.OilandGas.PPDM39.DataManagement/Services/Well/
├── WellServices.cs              — core CRUD + constructor (5 repositories wired)
├── WellServices.WellStatus.cs   — status queries, facet management, initializeDefaultStatuses
├── WellServices.WellStructures.cs — WELL_XREF queries, child entity lookup
├── WellServices.Models.cs       — WellStatusInfo DTO
└── WellServices.Helpers.cs      — private helpers (status desc lookup, defaults)
```
Namespace: `Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL`

### PPDM 3.9 Well Facets (STATUS_TYPE System)
Well status in PPDM 3.9 is **multi-faceted** — each STATUS_TYPE is an independent dimension:

| Scope | STATUS_TYPEs |
|-------|-------------|
| **Well** | Business Interest, Business Life Cycle Phase, Business Intention, Operatorship, Outcome, Lahee Class, Play Type, Well Structure, Well Reporting Class, Fluid Type, Well Status |
| **Wellbore** | Business Interest, Role, Trajectory Type, Fluid Type, Wellbore Status |
| **Wellhead Stream** | Fluid Direction |

- Always pass `initializeDefaultStatuses: true` when creating a well — this seeds all 15 standard facets from `R_WELL_STATUS`.
- `GetCurrentWellStatusByUwiAsync(uwi)` → `Dictionary<string, WELL_STATUS>` — one entry per STATUS_TYPE, most recent effective date wins.
- `WellStatusInfo` DTO holds `WELL_STATUS` + `R_WELL_STATUS` description + `Facets` dictionary + typed `StatusType`/`StatusName`.

### Key Methods
```csharp
// Get well by UWI
var well = await _wellServices.GetByUwiAsync(uwi);

// Create well and seed default status facets
await _wellServices.CreateAsync(well, userId, initializeDefaultStatuses: true);

// Get all status records for a well (full history, all facets)
List<WELL_STATUS> history = await _wellServices.GetWellStatusByUwiAsync(uwi);

// Get current status — one per STATUS_TYPE (most recent wins)
Dictionary<string, WELL_STATUS> current = await _wellServices.GetCurrentWellStatusByUwiAsync(uwi);

// Get well structures (WELL_XREF grouped by XREF_TYPE)
Dictionary<string, List<WELL_XREF>> structures = await _wellServices.GetWellStructuresByUwiAsync(uwi);

// Get child entities via XREF
List<WELL_TUBULAR> tubulars = await _wellServices.GetChildEntitiesByWellStructureAsync<WELL_TUBULAR>(
    uwi, xrefType, "WELL_TUBULAR");
```

### Registration Pattern
```csharp
builder.Services.AddScoped<WellServices>(sp =>
{
    var editor = sp.GetRequiredService<IDMEEditor>();
    var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
    var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
    var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
    return new WellServices(editor, commonColumnHandler, defaults, metadata, connectionName);
});
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

❌ **Wrong**: `GetType().GetProperty("WELL_ID")` on results from a `typeof(WELL)` repository → brittle, wrong column names (`UWI` not `WELL_ID`), silently returns null.  
✅ **Right**: Use `typeof(WELL)` in `PPDMGenericRepository`; cast results with `OfType<WELL>()`; access `w.UWI`, `w.ASSIGNED_FIELD`, `w.FINAL_TD` directly.

**Rule**: When the PPDM39 entity type is known at compile time, NEVER use `GetType().GetProperty(...)` on the result objects. Use the typed class directly.

❌ **Wrong**: Creating `PPDMGenericRepository` for `WELL`, `WELL_STATUS`, or `WELL_XREF` directly in a controller or feature service.  
✅ **Right**: Inject `WellServices` and call its typed methods.

❌ **Wrong**: Setting a single `WELL_STATUS` row for a new well without initializing all 15 PPDM 3.9 STATUS_TYPE facets.  
✅ **Right**: Call `CreateAsync(well, userId, initializeDefaultStatuses: true)`.

❌ **Wrong**: Querying `WELL_STATUS` for "current" status without grouping by STATUS_TYPE (returns stale/duplicate rows).  
✅ **Right**: Use `GetCurrentWellStatusByUwiAsync` which returns one entry per STATUS_TYPE using the most recent EFFECTIVE_DATE.

Known compile-time types (always use `typeof(X)` + `OfType<X>()`):
- `WELL` — PK: `UWI` (not `WELL_ID`); field link: `ASSIGNED_FIELD` (not `FIELD_ID`); depth: `FINAL_TD`, `DRILL_TD`
- `WELL_STATUS` — PK: `UWI` + `STATUS_TYPE` + `STATUS_ID` + `EFFECTIVE_DATE`; never query without STATUS_TYPE context
- `WELL_XREF` — links well structures; key column: `XREF_TYPE` (values from `_defaults.GetWellboreXrefType()` etc.)
- `R_WELL_STATUS` — reference table for status descriptions; columns: `STATUS_TYPE`, `STATUS`, `LONG_NAME`
- `FACILITY` — PK: `FACILITY_ID`; field link: `PRIMARY_FIELD_ID`
- `WELL_TEST`, `WELL_TUBULAR`, `WELL_ACTIVITY`, `WELL_EQUIPMENT`, `PDEN_VOL_SUMMARY`, `POOL`, `PDEN`

Dynamic types (reflection acceptable — type not known at compile time):
- `WELL_ABANDONMENT`, `FACILITY_DECOMMISSIONING`, `ENVIRONMENTAL_RESTORATION`, `DRILLING_OPERATION` — loaded via `Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")`
- Generic helpers with `string propertyName` parameter — e.g., `GetStringValue(object entity, string fieldName)`

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

## UI/UX Design Rules (Oil & Gas)

> Full guidelines: `Plans/UI-UX-OilAndGas-Guidelines.md`  
> Business process reference: `Plans/BusinessProcessesPlan/PetroleumEngineerBusinessProcesses.md`

### Core Rules

1. **Business process pages simulate engineer workflows — NOT CRUD operations.**
   - Page titles must be verb phrases ("Optimize Well Performance"), not nouns ("Wells").
   - Every page must have at least one visible "next action" button.
   - If a page title contains "and", split it into two pages.

2. **SVG only for new images — no PNG/JPG icons.**
   - All new icon/image assets go in `wwwroot/imgs/icons/` as `.svg` files.
   - Existing PNGs are legacy; replace when touched.
   - SVGs must use `currentColor` (not hardcoded fill colors).

3. **Active field context always visible.**
   - Every business process page shows the active field name in its header.
   - Use `IFieldOrchestrator.CurrentFieldId` + field name display in page header.

4. **MudBlazor colors only — never hardcode.**
   - Use `Color.*` enum for all component colors.
   - Use MudBlazor CSS utilities (`pa-4`, `mb-2`, etc.) — no inline `style=""`.
   - Dark mode support is automatic when using `Color.*` and theme CSS variables.

5. **PPDMTreeView stays in Data Management — separate from engineer UX.**
   - Data management (PPDM TreeView, raw CRUD) lives at `/ppdm39/data-management`.
   - Engineer workflow pages do NOT link back to raw CRUD as a primary action.

6. **Navigation follows the petroleum engineer's work sequence:**
   ```
   Dashboard → Exploration → Development → Production → Reservoir → Economics → HSE & Compliance → Data Management
   ```

### Shared Component Rules

- Use `KpiCard` component (in `Components/Shared/`) for all KPI displays on dashboards.
- Use `StatusBadge` component for all status chips (well status, process status, etc.).
- Use `ProcessTimeline` component for showing process step history.
- Multi-step processes use `MudStepper` — maximum 5 steps per wizard.
- Kanban boards: maximum 6 columns; drag triggers a transition dialog (not silent).

### Page Patterns Reference

| Pattern | When to Use | MudBlazor Components |
|---------|-------------|----------------------|
| Dashboard | Section entry point | `MudGrid`, `KpiCard`, `MudChart` |
| Wizard | Multi-step approval or data entry | `MudStepper`, `MudForm` |
| Kanban Board | Item lifecycle (prospect, work order) | `MudGrid` columns, custom cards |
| Decision Page | Gate reviews, intervention decisions | Two-column layout, `MudPaper` |
| Monitoring Page | Live status, well performance | Status grid, `MudChart` line |
| Workbench | Complex analytical tools | Split panel, `MudTabs` |
