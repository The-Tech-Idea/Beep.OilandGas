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

## Data Class Shape Rule (Table vs Projection)

Every class is either a **table class** (saved to DB) or a **projection class** (DTOs, results, requests). The shape rules differ:

### Table classes
- **Must** extend `ModelEntityBase`.
- **Scalar columns only** — no `List<T>`, `IEnumerable<T>`, `Dictionary<K,V>`, or nested object properties.
- Named in SCREAMING_SNAKE_CASE matching the PPDM table (e.g. `PROSPECT`, `PROSPECT_RISK_ASSESSMENT`).
- On Windows, if the SCREAMING_SNAKE_CASE filename would collide with a PascalCase sibling (e.g. `PROSPECT.cs` vs `Prospect.cs`), append `.Table.cs` to the filename only (`PROSPECT.Table.cs`), keeping the class name unchanged.
- Live in the feature project under `Data/Tables/` or the PPDM39 shared model library.

### Projection classes (DTOs, requests, responses, analysis results)
- May extend `ModelEntityBase` but are **never** passed to `InsertAsync`/`UpdateAsync`.
- May freely contain `List<T>`, `Dictionary<K,V>`, and nested objects.
- Named in PascalCase with a descriptive suffix: `...Request`, `...Response`, `...Result`, `...Summary`, `...Report`.
- Live under `Data/{Slice}/` in the feature project.

**Quick test**: if the class is ever passed to `repo.InsertAsync(entity)` or `repo.UpdateAsync(entity)`, it is a table class — remove all collections from it immediately.

## Where code lives: Models, feature projects, and PPDM39.DataManagement

### `Beep.OilandGas.Models` (shared only)

- Add types under `Beep.OilandGas.Models` **only when more than one feature project must use the same type** (shared PPDM table classes, cross-cutting API contracts, `Beep.OilandGas.Models.Core.Interfaces`).
- Use `ModelEntityBase` for shared models; it implements `IPPDMEntity` and PPDM audit columns. Do not create or reintroduce DTO **namespaces** — use `Beep.OilandGas.Models.Data` for shared model types.
- **Do not** place a single-domain’s **enums**, **reference-seed constants**, or **module-only** artifacts in Models for convenience. Those belong in the **owning feature project** (see below).

### Each feature project (e.g. `Beep.OilandGas.ChokeAnalysis`, `Beep.OilandGas.NodalAnalysis`, …)

- Owns that domain’s **enums**, **constants**, **reference seed catalog** (row definitions), and projections used **only** inside that domain.
- May define **`I{Domain}Service`** in **`{Feature}.Core.Interfaces`** when the contract uses **extension table types** (`ModelEntityBase`) that live **only** in that feature assembly (example: **`Beep.OilandGas.CompressorAnalysis.Core.Interfaces.ICompressorAnalysisService`** with **`COMPRESSOR_*`** types in **`Beep.OilandGas.CompressorAnalysis.Data`**). Shared wire DTOs used across API/Web/LifeCycle remain in **`Beep.OilandGas.Models.Data.Calculations`** where applicable.
- **Must ship `ModuleSetupBase` / `IModuleSetup`** (`Modules/{Domain}Module.cs`) when the project introduces **extension tables** (`ModelEntityBase` under `{Feature}.Data` / `{Feature}.Data/Tables`) **and/or** **domain-specific reference LOV seed rows**. Register **`EntityTypes`** for those extension tables only (not standard PPDM 3.9 core tables already covered by the main schema path). Implement **`SeedAsync`** with skip-if-exists inserts for **`R_*`** / catalog rows (mirror **ChokeAnalysis**, **CompressorAnalysis**, **EnhancedRecovery**). Pure libraries with **no** persistence surface are exempt — state that in the project README.
  - `EntityTypes` — **project-specific / extension** tables the feature introduces (not already covered as standard PPDM 3.9 core). Used so schema creation and metadata registration can pick up those types together with PPDM tooling. Do **not** list ordinary PPDM tables (`EQUIPMENT`, `WELL_EQUIPMENT`, `PDEN_FLOW_MEASUREMENT`, …) — they belong to the core PPDM model / database creation path.
  - `SeedAsync` — inserts reference rows for that domain (skip-if-exists patterns as with other modules)
- Modules are **auto-discovered** from loaded `Beep.OilandGas.*` assemblies (`AddDiscoveredModuleSetups()`); do **not** wire domain modules inside `PPDM39.DataManagement` beyond shared infrastructure.

### Schema for extension tables — entities + ModuleSetup, not hand-written SQL

- **Do not** add, edit, or ask agents to maintain DDL under `Beep.OilandGas.Models/Scripts/**` (or similar) for **feature extension** tables.
- **Do** define table classes (`ModelEntityBase`, scalar columns, correct table name), register them on the owning feature’s **`IModuleSetup.EntityTypes`**, implement **`SeedAsync`** when reference rows are required, and rely on **Beep / PPDM data-management tooling** (e.g. migration / `CreateSchemaFromEntitiesAsync`, database creator, or your pipeline) to materialize tables from entities.
- Agents treat **`Models/Scripts`** as infrastructure the project may ship or generate elsewhere — **not** something to extend per feature by writing new `.sql` files.

### `Beep.OilandGas.PPDM39.DataManagement` (PPDM infrastructure only)

- **Purpose**: PPDM plumbing — metadata, `PPDMGenericRepository`, shared seeders that operate on **Models**-level enums/tables (e.g. `PPDMReferenceDataSeeder`, `EnumReferenceDataSeeder`), `ModuleSetupBase` **base class**, `ModuleSetupOrchestrator`, WellServices, LOV helpers, etc.
- **Project references**: **`Beep.OilandGas.Models`** and **`Beep.OilandGas.PPDM39`** (and Beep packages). **Do not** add project references to calculation/engineering feature projects (**ChokeAnalysis**, **NodalAnalysis**, **GasLift**, **PipelineAnalysis**, etc.).
- **No cross-talk in code or comments** that depends on or advertises a specific domain calculator assembly from this project; domain reference data and domain module setup live in the **feature project** that owns the module.

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
- [ ] Confirm **shared** cross-library interfaces live in `Beep.OilandGas.Models.Core.Interfaces` unless the API surface depends **only** on feature-local extension tables — then the interface may live in `{Feature}.Core.Interfaces` (see **CompressorAnalysis**).
- [ ] Verify service registration uses factory pattern: `AddScoped<IFoo>(sp => new Foo(sp.GetRequiredService<...>(), ...))`.
- [ ] If touching data access, use the PPDMGenericRepository pattern shown in `.cursor/commands/beep-dataaccess-generic-repository.md`.
- [ ] If adding domain enums/seeds/modules: put them in the **feature project** (`ModuleSetupBase` + `Modules/{Domain}Module.cs`), not in `PPDM39.DataManagement`; keep **Models** for **cross-project shared** types only. When adding extension **`R_*`** / analysis tables, update **`EntityTypes`** and **`SeedAsync`** on that feature’s module.
- [ ] Never add a **project reference** from `PPDM39.DataManagement` to a calculation/feature project (ChokeAnalysis, NodalAnalysis, …).
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
- **Namespaces**: `Beep.OilandGas.Models.Core.Interfaces` for **shared** interfaces; feature **`Core.Interfaces`** when the contract is tied to feature-local extension entities; `Beep.OilandGas.Models.Data` for shared models; `.DataManagement.Services` for implementations
- **Files**: `I{ServiceName}Service.cs` for interfaces; `{ServiceName}Service.cs` for implementations
- **DB IDs**: All string-based; use `_defaults.FormatIdForTable("TABLE_NAME", id)` to format

## Common Mistakes to Avoid

1. Registering service before `IDMEEditor` is available -> startup null refs.
2. Putting **every** type in `Beep.OilandGas.Models` -> only **cross-project shared** models belong there; domain enums/constants/seeds stay in the **feature project**.
3. Adding a **project reference** from **`PPDM39.DataManagement`** to **ChokeAnalysis** / **NodalAnalysis** / other calculation projects -> DataManagement stays PPDM + Models only.
4. Using `ExecuteSql` for SELECT queries -> only use for DDL; use `GetEntityAsync` or `RunQuery` instead.
5. Hardcoding SQL parameter delimiters -> use `dataSource.ParameterDelimiter` or `AppFilter`.
6. Creating separate databases per feature -> always use the single PPDM39 schema.
7. Creating raw `PPDMGenericRepository` for `WELL`/`WELL_STATUS`/`WELL_XREF` outside `WellServices` -> duplicate logic, missed facet init.
8. Querying `WELL_STATUS` without grouping by `STATUS_TYPE` -> stale/duplicate current-status results. Use `GetCurrentWellStatusByUwiAsync`.
9. Creating a new well without `initializeDefaultStatuses: true` -> well missing its 15 PPDM 3.9 status facets.
10. Adding `List<T>`, `Dictionary<K,V>`, or nested object properties to a **table class** (one passed to `InsertAsync`/`UpdateAsync`) -> data layer cannot serialize collections to columns; remove them and use a projection class instead.
11. On Windows, creating both `PROSPECT.cs` and `Prospect.cs` in the same folder -> filesystem collision; use `PROSPECT.Table.cs` for the table class filename.
12. Authoring new **`Scripts/**/*.sql`** for extension/feature tables instead of **entity types + `ModuleSetupBase.EntityTypes` + tooling-driven schema creation** -> wrong workflow for this repo; see *Schema for extension tables* above.

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

- `.cursor/commands/architecture-patterns.md` - Three-layer patterns, service/API organization; **Extension tables** subsection (ModuleSetup, no hand SQL for feature tables)
- `.cursor/commands/beep-dataaccess-generic-repository.md` - PPDMGenericRepository CRUD examples
- `.cursor/commands/beep-dataaccess-overview.md` - Data access framework overview; extension-table note at top
- `.cursor/commands/beep-dataaccess-database-creation.md` - Script-based creator vs **entity-driven** extension schema
- `.cursor/commands/database-script-generation.md` - Reference patterns only; **not** for agent edits to `Models/Scripts` for feature extensions (see *Schema for extension tables* above)
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
