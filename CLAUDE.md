# Beep.OilandGas - Agent Assistance Guidelines

This guide helps agents make safe, well-scoped edits that align with Beep.OilandGas architecture and patterns.

## Essential Architecture Knowledge (Read First)

1. **Three-Layer Architecture**:
   - **Web Layer**: Blazor Server (`Beep.OilandGas.Web`)
   - **API Layer**: ASP.NET Core (`Beep.OilandGas.ApiService`)
   - **Data Layer**: Beep framework (`IDMEEditor`, `IDataSource`) + `PPDMGenericRepository`

2. **Single PPDM39 Database Model**:
   - All lifecycle data lives in one PPDM39 schema.
   - Use `PPDMGenericRepository` for CRUD; it resolves tables via metadata.

3. **FieldOrchestrator Pattern**:
   - Manages current active field context.
   - Field-scoped API endpoints live under `/api/field/current/*`.
   - Services filter by `FIELD_ID` when called through FieldOrchestrator.

## Data Classes (No DTOs)

- All shared models live in `Beep.OilandGas.Models/Data`.
- Use `ModelEntityBase` for any shared model; it implements `IPPDMEntity` and includes PPDM audit columns.
- Do not create or reintroduce DTO namespaces.

## WellServices — Mandatory for All Well Operations

Any feature touching a well MUST use `WellServices` from `Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL`. Never create raw `PPDMGenericRepository` instances for `WELL`, `WELL_STATUS`, or `WELL_XREF` outside it.

**File layout** (partial class):
```
Beep.OilandGas.PPDM39.DataManagement/Services/Well/
├── WellServices.cs               — core CRUD; wires 4 PPDMGenericRepositories
├── WellServices.WellStatus.cs    — status queries, facet management
├── WellServices.WellStructures.cs — WELL_XREF / child entity lookup
├── WellServices.Models.cs        — WellStatusInfo DTO
└── WellServices.Helpers.cs       — private helpers
```

**PPDM 3.9 Well Facets** — well status is multi-dimensional. 15 standard `STATUS_TYPE` facets must be seeded on creation:
- Well-level: Business Interest, Business Life Cycle Phase, Business Intention, Operatorship, Outcome, Lahee Class, Play Type, Well Structure, Well Reporting Class, Fluid Type, Well Status
- Wellbore-level: Business Interest, Role, Trajectory Type, Fluid Type, Wellbore Status
- Wellhead Stream: Fluid Direction

**Rules**:
1. Always pass `initializeDefaultStatuses: true` when calling `CreateAsync` for a new well.
2. Use `GetCurrentWellStatusByUwiAsync(uwi)` → `Dictionary<string, WELL_STATUS>` for current facet values (groups by STATUS_TYPE, most recent EFFECTIVE_DATE wins).
3. Use `GetWellStructuresByUwiAsync(uwi)` → `Dictionary<string, List<WELL_XREF>>` for wellbore/completion tree.
4. `WellStatusInfo` DTO carries `WELL_STATUS` + `R_WELL_STATUS` description + `Facets` dictionary — always prefer this over raw `WELL_STATUS` in API responses.

**Registration**:
```csharp
builder.Services.AddScoped<WellServices>(sp =>
    new WellServices(
        sp.GetRequiredService<IDMEEditor>(),
        sp.GetRequiredService<ICommonColumnHandler>(),
        sp.GetRequiredService<IPPDM39DefaultsRepository>(),
        sp.GetRequiredService<IPPDMMetadataRepository>(),
        connectionName));
```

## Mandatory Pre-Edit Checklist

Before making any code change:

- [ ] Read `Beep.OilandGas.ApiService/Program.cs` lines 1-120 to confirm DI order (AddBeepServices must come before services that consume IDMEEditor).
- [ ] If touching `Beep.OilandGas.Web` UI, read `Beep.OilandGas.Web/MudBlazor_Docs/README.md` and the relevant local MudBlazor component docs before editing `.razor`, layout, theme, navigation, dialogs, steppers, tabs, or data grids.
- [ ] Confirm all new interfaces live in `Beep.OilandGas.Models.Core.Interfaces`.
- [ ] Verify service registration uses factory pattern: `AddScoped<IFoo>(sp => new Foo(sp.GetRequiredService<...>(), ...))`.
- [ ] If touching data access, use the PPDMGenericRepository pattern shown in `.cursor/commands/beep-dataaccess-generic-repository.md`.
- [ ] If touching any well entity (`WELL`, `WELL_STATUS`, `WELL_XREF`), use `WellServices` — never create raw repositories for these tables outside it.
- [ ] If creating a new well, pass `initializeDefaultStatuses: true` to seed all 15 PPDM 3.9 STATUS_TYPE facets.
- [ ] If querying current well status, use `GetCurrentWellStatusByUwiAsync` — not a raw `WELL_STATUS` query without STATUS_TYPE grouping.

## Data Access Pattern (Copy-Paste Template)

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
- **Namespaces**: `Beep.OilandGas.Models.Core.Interfaces` for interfaces; `Beep.OilandGas.Models.Data` for shared models; `.DataManagement.Services` for implementations
- **Files**: `I{ServiceName}Service.cs` for interfaces; `{ServiceName}Service.cs` for implementations
- **DB IDs**: All string-based; use `_defaults.FormatIdForTable("TABLE_NAME", id)` to format

## Common Mistakes to Avoid

1. Registering service before `IDMEEditor` is available -> startup null refs.
2. Reintroducing DTO namespaces -> all shared models are `Beep.OilandGas.Models.Data`.
3. Using `ExecuteSql` for SELECT queries -> only use for DDL; use `GetEntityAsync` or `RunQuery` instead.
4. Hardcoding SQL parameter delimiters -> use `dataSource.ParameterDelimiter` or `AppFilter`.
5. Creating separate databases per feature -> always use the single PPDM39 schema.
6. Creating raw `PPDMGenericRepository` for `WELL`/`WELL_STATUS`/`WELL_XREF` outside `WellServices` -> duplicate logic, missed facet init.
7. Querying `WELL_STATUS` without grouping by `STATUS_TYPE` -> stale/duplicate current-status results. Use `GetCurrentWellStatusByUwiAsync`.
8. Creating a new well without `initializeDefaultStatuses: true` -> well missing its 15 PPDM 3.9 status facets.

## Quick Verification Steps

```bash
dotnet build Beep.OilandGas.sln
dotnet run --project Beep.OilandGas.ApiService
```

## Known Compile-Time Types

Always use `typeof(X)` + `.OfType<X>()` for these — never use `GetType().GetProperty(...)` on their results:

- `WELL` — PK: `UWI` (not `WELL_ID`); field link: `ASSIGNED_FIELD`; depth: `FINAL_TD`, `DRILL_TD`
- `WELL_STATUS` — PK: `UWI` + `STATUS_TYPE` + `STATUS_ID` + `EFFECTIVE_DATE`; always group by STATUS_TYPE
- `WELL_XREF` — key column: `XREF_TYPE` (values from `_defaults.GetWellboreXrefType()` etc.)
- `R_WELL_STATUS` — columns: `STATUS_TYPE`, `STATUS`, `LONG_NAME`
- `FACILITY` — PK: `FACILITY_ID`; field link: `PRIMARY_FIELD_ID`
- `WELL_TEST`, `WELL_TUBULAR`, `WELL_ACTIVITY`, `WELL_EQUIPMENT`, `PDEN_VOL_SUMMARY`, `POOL`, `PDEN`

## Reference Documentation (Treat as Source-of-Truth)

- `.cursor/commands/architecture-patterns.md` - Three-layer patterns, service/API organization
- `.cursor/commands/beep-dataaccess-generic-repository.md` - PPDMGenericRepository CRUD examples
- `.cursor/commands/beep-dataaccess-overview.md` - Data access framework overview
- `.cursor/commands/naming-conventions.md` - Project, namespace, and file naming rules
- `.cursor/commands/data-access-patterns.md` - Data access best practices
- `.cursor/commands/best-practices.md` - FASB compliance, revenue recognition, accounting patterns

## Web Layer Patterns (Critical)

### MudBlazor UI Prerequisite

- Before changing Blazor UI structure or component markup, start with `Beep.OilandGas.Web/MudBlazor_Docs/README.md`.
- For shell/layout work, read `Layouts.txt`, `Theme.txt`, `Services.txt`, `AppBar.txt`, `Drawer.txt`, and `PopOver.txt` first.
- For shared widgets and workbenches, read the matching local docs such as `DataGrid.txt`, `Dialog.txt`, `Tabs.txt`, `Stepper.txt`, `Grid.txt`, `NavMenu.txt`, and `Button.txt` before editing.
- Do not guess MudBlazor parameter names when a local doc exists.

**ApiClient**:
```csharp
@inject ApiClient ApiClient
var result = await ApiClient.GetAsync<List<Field>>("/api/field/fields");
```

**DataManagementService**:
- Centralized data + connection management
- CRUD operations for any PPDM entity
- Import/export with progress
- Validation, quality checks, versioning

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
        var result = await _service.ExecuteAsync(request, userId);
        return Ok(result);
    }
}
```
